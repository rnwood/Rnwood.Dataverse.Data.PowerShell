using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// Factory for creating mock ServiceClient instances.
    /// The created ServiceClient does not support Clone(), which causes Invoke-DataverseParallel
    /// to fall back to using the original connection with thread-safe synchronization.
    /// </summary>
    internal static class MockServiceClientFactory
    {
        private static ConstructorInfo? _constructor;

        /// <summary>
        /// Creates a new ServiceClient using the internal constructor.
        /// The service parameter should be wrapped with ThreadSafeOrganizationService for parallel operations.
        /// </summary>
        public static ServiceClient Create(
            IOrganizationService service,
            HttpClient httpClient,
            string instanceUri,
            Version sdkVersion,
            ILogger logger)
        {
            if (_constructor == null)
            {
                _constructor = typeof(ServiceClient).GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(IOrganizationService), typeof(HttpClient), typeof(string), typeof(Version), typeof(ILogger) },
                    null);

                if (_constructor == null)
                {
                    throw new InvalidOperationException("Could not find ServiceClient internal constructor");
                }
            }

            return (ServiceClient)_constructor.Invoke(new object[] { service, httpClient, instanceUri, sdkVersion, logger });
        }
    }
}
