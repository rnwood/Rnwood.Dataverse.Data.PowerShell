using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse
{
    /// <summary>
    /// Registry that maps <see cref="OrganizationRequest"/> types to handlers.
    /// Thread-safe: uses copy-on-write for the handler list.
    /// </summary>
    /// <remarks>
    /// When a plain (untyped) <see cref="OrganizationRequest"/> is received, the registry
    /// automatically converts it to its strongly-typed equivalent (e.g. <c>WhoAmIRequest</c>)
    /// by matching <see cref="OrganizationRequest.RequestName"/> to known SDK request types.
    /// This mirrors real Dataverse behaviour where both typed and untyped requests are accepted.
    /// </remarks>
    public sealed class OrganizationRequestHandlerRegistry
    {
        private volatile IOrganizationRequestHandler[] _handlers = Array.Empty<IOrganizationRequestHandler>();
        private readonly object _writeLock = new object();

        /// <summary>
        /// Lazy-initialized map from RequestName → typed OrganizationRequest subclass.
        /// Built once by scanning SDK assemblies.
        /// </summary>
        private static readonly Lazy<ConcurrentDictionary<string, Type>> RequestTypeMap =
            new Lazy<ConcurrentDictionary<string, Type>>(BuildRequestTypeMap);

        /// <summary>
        /// Registers a handler. Later registrations take priority (matched last-wins).
        /// </summary>
        public void Register(IOrganizationRequestHandler handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            lock (_writeLock)
            {
                var current = _handlers;
                var next = new IOrganizationRequestHandler[current.Length + 1];
                Array.Copy(current, next, current.Length);
                next[current.Length] = handler;
                _handlers = next;
            }
        }

        internal OrganizationResponse Execute(OrganizationRequest request, IOrganizationService service)
        {
            var handlers = _handlers; // snapshot
            // Walk backwards so later registrations override earlier ones.
            for (int i = handlers.Length - 1; i >= 0; i--)
            {
                if (handlers[i].CanHandle(request))
                    return handlers[i].Handle(request, service);
            }

            throw new NotSupportedException($"No handler registered for request type '{request.GetType().Name}' (RequestName: '{request.RequestName}').");
        }

        /// <summary>
        /// Executes a request using only the built-in typed handlers, skipping custom API handlers.
        /// This is useful when a custom API override needs to fall back to the default behaviour.
        /// </summary>
        public OrganizationResponse ExecuteSkippingCustomApis(OrganizationRequest request, IOrganizationService service)
        {
            var handlers = _handlers; // snapshot
            for (int i = handlers.Length - 1; i >= 0; i--)
            {
                if (handlers[i] is Handlers.CustomApiRequestHandler)
                    continue;
                if (handlers[i].CanHandle(request))
                    return handlers[i].Handle(request, service);
            }

            throw new NotSupportedException($"No built-in handler registered for request type '{request.GetType().Name}' (RequestName: '{request.RequestName}').");
        }


        /// <summary>
        /// Scans known SDK assemblies for all <see cref="OrganizationRequest"/> subclasses and
        /// builds a lookup from RequestName to Type.
        /// </summary>
        private static ConcurrentDictionary<string, Type> BuildRequestTypeMap()
        {
            var map = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            // Scan assemblies that contain typed request classes
            var assemblies = new HashSet<Assembly>();

            // Microsoft.Xrm.Sdk (CreateRequest, UpdateRequest, etc.)
            assemblies.Add(typeof(Microsoft.Xrm.Sdk.Messages.CreateRequest).Assembly);

            // Microsoft.Crm.Sdk.Messages (WhoAmIRequest, AssignRequest, etc.)
            try
            {
                assemblies.Add(typeof(Microsoft.Crm.Sdk.Messages.WhoAmIRequest).Assembly);
            }
            catch
            {
                // Assembly might not be loaded
            }

            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray()!;
                }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract || !typeof(OrganizationRequest).IsAssignableFrom(type))
                        continue;
                    if (type == typeof(OrganizationRequest))
                        continue;

                    try
                    {
                        var instance = (OrganizationRequest)Activator.CreateInstance(type)!;
                        if (!string.IsNullOrEmpty(instance.RequestName))
                            map.TryAdd(instance.RequestName, type);
                    }
                    catch
                    {
                        // Some types may not have parameterless constructors
                    }
                }
            }

            return map;
        }
    }
}
