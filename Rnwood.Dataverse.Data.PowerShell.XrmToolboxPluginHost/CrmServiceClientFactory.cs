using Microsoft.Xrm.Sdk.WebServiceClient;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.IO;
using System.IO.Pipes;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPluginHost
{
    /// <summary>
    /// Factory for creating CrmServiceClient with external token via named pipe.
    /// Uses OrganizationWebProxyClient internally since the older SDK version doesn't have
    /// the token provider constructor pattern.
    /// </summary>
    static class CrmServiceClientFactory
    {
        /// <summary>
        /// Creates a CrmServiceClient using an access token from the named pipe.
        /// </summary>
        /// <param name="url">The Dataverse organization URL</param>
        /// <param name="pipeName">The named pipe to use for token retrieval</param>
        /// <returns>A configured CrmServiceClient</returns>
        public static CrmServiceClient Create(string url, string pipeName)
        {
            string token = GetTokenFromPipe(pipeName);
            
            // Build the organization service URI
            // The URL might be:
            // - Base URL: https://org.crm.dynamics.com/
            // - Full service URL: https://org.crm.dynamics.com/XRMServices/2011/Organization.svc
            // - Web endpoint URL: https://org.crm.dynamics.com/XRMServices/2011/Organization.svc/web
            
            Uri baseUri = new Uri(url);
            Uri orgServiceUri;
            
            string path = baseUri.AbsolutePath.TrimEnd('/');
            
            if (path.EndsWith("/Organization.svc/web", StringComparison.OrdinalIgnoreCase))
            {
                // Already the correct web endpoint
                orgServiceUri = baseUri;
            }
            else if (path.EndsWith("/Organization.svc", StringComparison.OrdinalIgnoreCase))
            {
                // Need to add /web
                orgServiceUri = new Uri(url.TrimEnd('/') + "/web");
            }
            else
            {
                // Base URL - need to add the full path
                string baseUrl = baseUri.GetLeftPart(UriPartial.Authority);
                orgServiceUri = new Uri(baseUrl + "/XRMServices/2011/Organization.svc/web");
            }
            
            Console.WriteLine($"Creating OrganizationWebProxyClient for: {orgServiceUri}");
            
            // Create an OrganizationWebProxyClient with the access token
            var webProxyClient = new OrganizationWebProxyClient(orgServiceUri, false);
            webProxyClient.HeaderToken = token;
            
            Console.WriteLine($"OrganizationWebProxyClient created with token ({token?.Length ?? 0} characters)");
            
            // Create CrmServiceClient from the web proxy client
            var client = new CrmServiceClient(webProxyClient);
            
            Console.WriteLine($"CrmServiceClient created, IsReady: {client.IsReady}");
            
            if (!client.IsReady)
            {
                Console.WriteLine($"Connection not ready, error: {client.LastCrmError}");
                throw new InvalidOperationException($"Failed to create CrmServiceClient: {client.LastCrmError}");
            }
            
            return client;
        }

        private static string GetTokenFromPipe(string pipeName)
        {
            try
            {
                Console.WriteLine("Fetching token from named pipe...");
                using (var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.In))
                {
                    pipeClient.Connect(30000);

                    using (var reader = new StreamReader(pipeClient))
                    {
                        string token = reader.ReadToEnd();
                        Console.WriteLine($"Token received from named pipe ({token?.Length ?? 0} characters)");
                        return token;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve token from named pipe: {ex.Message}");
                throw;
            }
        }
    }
}

