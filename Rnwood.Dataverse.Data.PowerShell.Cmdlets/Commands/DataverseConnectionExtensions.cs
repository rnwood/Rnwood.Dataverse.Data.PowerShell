using System;
using System.Collections.Concurrent;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Provides extension methods and utilities for working with Dataverse connections.
    /// </summary>
    internal static class DataverseConnectionExtensions
    {
        // Registry to track IDataverseConnection wrappers for ServiceClient instances
        private static readonly ConcurrentDictionary<ServiceClient, IDataverseConnection> _connectionRegistry 
            = new ConcurrentDictionary<ServiceClient, IDataverseConnection>();

        /// <summary>
        /// Registers a ServiceClient with its IDataverseConnection wrapper.
        /// This allows the connection to be cloned properly even if it's a mock connection.
        /// </summary>
        public static void RegisterConnection(ServiceClient serviceClient, IDataverseConnection connection)
        {
            _connectionRegistry[serviceClient] = connection;
        }

        /// <summary>
        /// Attempts to clone a ServiceClient, using the registered IDataverseConnection if available.
        /// </summary>
        /// <param name="serviceClient">The ServiceClient to clone</param>
        /// <returns>A cloned ServiceClient, or throws if cloning is not supported</returns>
        public static ServiceClient CloneConnection(ServiceClient serviceClient)
        {
            // First check if this is a registered connection with custom clone support
            if (_connectionRegistry.TryGetValue(serviceClient, out var connection))
            {
                var cloned = connection.Clone();
                return cloned.ServiceClient;
            }

            // Otherwise, use the standard Clone() method
            // This may throw NotImplementedException for mock connections that aren't registered
            return serviceClient.Clone();
        }

        /// <summary>
        /// Attempts to clone a ServiceClient, returning null if cloning is not supported.
        /// </summary>
        /// <param name="serviceClient">The ServiceClient to clone</param>
        /// <returns>A cloned ServiceClient, or null if cloning is not supported</returns>
        public static ServiceClient TryCloneConnection(ServiceClient serviceClient)
        {
            try
            {
                return CloneConnection(serviceClient);
            }
            catch (NotImplementedException)
            {
                return null;
            }
            catch (Exception ex) when (ex.Message.Contains("On-Premises Connections are not supported") ||
                                        ex.InnerException is NotImplementedException)
            {
                return null;
            }
        }
    }
}
