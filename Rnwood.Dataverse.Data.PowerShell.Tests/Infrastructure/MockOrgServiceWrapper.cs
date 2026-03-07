using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Net.Http;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// Factory for creating mock ServiceClient for SQL4Cds tests.
    /// Delegates to MockServiceClientFactory which handles all reflection details.
    /// </summary>
    internal static class MockSql4CdsServiceClientFactory
    {
        /// <summary>
        /// Creates a mock ServiceClient for SQL4Cds that returns valid ConnectedOrgUriActual.
        /// Uses the existing MockServiceClientFactory infrastructure.
        /// </summary>
        public static ServiceClient CreateForSql4Cds(IOrganizationService innerService)
        {
            var uri = "https://fakeorg.crm.dynamics.com";
            var httpClient = new HttpClient(new FakeHttpMessageHandler());
            var logger = LoggerFactory.Create(b => { }).CreateLogger<ServiceClient>();
            var sdkVersion = new Version(9, 2);
            
            // Use the existing MockServiceClientFactory which already handles
            // setting all the required properties via reflection
            return MockServiceClientFactory.Create(
                innerService,
                httpClient,
                uri,
                sdkVersion,
                logger
            );
        }
    }
}
