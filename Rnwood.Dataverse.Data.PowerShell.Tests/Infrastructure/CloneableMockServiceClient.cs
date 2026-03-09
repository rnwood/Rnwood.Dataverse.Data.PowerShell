using System;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Rnwood.Dataverse.Data.PowerShell.Commands;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// A ServiceClient wrapper that supports cloning for mock connections.
    /// This allows parallel operations in tests to clone mock connections properly.
    /// </summary>
    internal class CloneableMockServiceClient : ICloneableServiceClient
    {
        private static ConstructorInfo? _internalConstructor;
        private readonly ServiceClient _serviceClient;
        private readonly IOrganizationService _service;
        private readonly HttpClient _httpClient;
        private readonly string _instanceUri;
        private readonly Version _sdkVersion;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new CloneableMockServiceClient using ServiceClient's internal constructor.
        /// </summary>
        public static ServiceClient Create(
            IOrganizationService service,
            HttpClient httpClient,
            string instanceUri,
            Version sdkVersion,
            ILogger logger)
        {
            if (_internalConstructor == null)
            {
                _internalConstructor = typeof(ServiceClient).GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(IOrganizationService), typeof(HttpClient), typeof(string), typeof(Version), typeof(ILogger) },
                    null);

                if (_internalConstructor == null)
                {
                    throw new InvalidOperationException("Could not find ServiceClient internal constructor");
                }
            }

            // Create ServiceClient instance using reflection
            var serviceClient = (ServiceClient)_internalConstructor.Invoke(
                new object[] { service, httpClient, instanceUri, sdkVersion, logger });

            // Set mock properties
            SetMockProperties(serviceClient, instanceUri, sdkVersion);

            // Create wrapper that implements ICloneableServiceClient
            var wrapper = new CloneableMockServiceClient(serviceClient, service, httpClient, instanceUri, sdkVersion, logger);
            
            // Store the wrapper in the ServiceClient so we can find it later
            // We'll use a ConditionalWeakTable for this
            WrapperRegistry.Register(serviceClient, wrapper);

            return serviceClient;
        }

        private CloneableMockServiceClient(
            ServiceClient serviceClient,
            IOrganizationService service,
            HttpClient httpClient,
            string instanceUri,
            Version sdkVersion,
            ILogger logger)
        {
            _serviceClient = serviceClient;
            _service = service;
            _httpClient = httpClient;
            _instanceUri = instanceUri;
            _sdkVersion = sdkVersion;
            _logger = logger;
        }

        /// <summary>
        /// Clones this mock connection by creating a new instance that shares the same
        /// underlying IOrganizationService (which is already thread-safe via ThreadSafeOrganizationService).
        /// </summary>
        public ServiceClient CloneServiceClient()
        {
            // Create a new ServiceClient instance with the same parameters
            // The underlying IOrganizationService is thread-safe, so we can share it
            return Create(_service, _httpClient, _instanceUri, _sdkVersion, _logger);
        }

        private static void SetMockProperties(ServiceClient client, string instanceUri, Version sdkVersion)
        {
            var uri = new Uri(instanceUri);

            // Set properties using the helper methods from MockServiceClientFactory
            MockServiceClientFactory.SetPrivateProperty(client, "ConnectedOrgUniqueName", "fakeorg");
            MockServiceClientFactory.SetPrivateProperty(client, "ConnectedOrgFriendlyName", "Fake Organization");
            MockServiceClientFactory.SetPrivateProperty(client, "ConnectedOrgVersion", sdkVersion);

            // Try to set URI-related fields
            MockServiceClientFactory.TrySetUriField(client, uri);
        }
    }

    /// <summary>
    /// Registry to track CloneableMockServiceClient wrappers for ServiceClient instances.
    /// </summary>
    internal static class WrapperRegistry
    {
        private static readonly ConditionalWeakTable<ServiceClient, ICloneableServiceClient> _registry =
            new ConditionalWeakTable<ServiceClient, ICloneableServiceClient>();

        public static void Register(ServiceClient serviceClient, ICloneableServiceClient wrapper)
        {
            _registry.Add(serviceClient, wrapper);
        }

        public static bool TryGetWrapper(ServiceClient serviceClient, out ICloneableServiceClient wrapper)
        {
            return _registry.TryGetValue(serviceClient, out wrapper);
        }
    }
}
