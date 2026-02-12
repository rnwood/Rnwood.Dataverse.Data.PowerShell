using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Rnwood.Dataverse.Data.PowerShell.Commands;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// Implementation of IDataverseConnection for mock ServiceClient instances.
    /// Provides custom Clone() implementation that creates new ServiceClient instances
    /// sharing the same thread-safe IOrganizationService.
    /// </summary>
    internal class MockDataverseConnection : IDataverseConnection
    {
        private readonly ServiceClient _serviceClient;
        private readonly IOrganizationService _service;
        private readonly HttpClient _httpClient;
        private readonly string _instanceUri;
        private readonly Version _sdkVersion;
        private readonly ILogger _logger;

        public MockDataverseConnection(
            ServiceClient serviceClient,
            IOrganizationService service,
            HttpClient httpClient,
            string instanceUri,
            Version sdkVersion,
            ILogger logger)
        {
            _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _instanceUri = instanceUri ?? throw new ArgumentNullException(nameof(instanceUri));
            _sdkVersion = sdkVersion ?? throw new ArgumentNullException(nameof(sdkVersion));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ServiceClient ServiceClient => _serviceClient;

        public IDataverseConnection Clone()
        {
            // Create a new ServiceClient instance that shares the same underlying IOrganizationService
            // The IOrganizationService is already wrapped with ThreadSafeOrganizationService, so it's safe to share
            var clonedClient = MockServiceClientFactory.Create(_service, _httpClient, _instanceUri, _sdkVersion, _logger);
            return new MockDataverseConnection(clonedClient, _service, _httpClient, _instanceUri, _sdkVersion, _logger);
        }
    }
}
