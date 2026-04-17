using Microsoft.PowerPlatform.Dataverse.Client;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Interface for ServiceClient instances that support custom cloning logic.
    /// This is primarily used for mock connections in tests.
    /// </summary>
    public interface ICloneableServiceClient
    {
        /// <summary>
        /// Creates a clone of this ServiceClient for use in parallel operations.
        /// </summary>
        /// <returns>A new ServiceClient instance that can be used concurrently</returns>
        ServiceClient CloneServiceClient();
    }

    /// <summary>
    /// Interface for ServiceClient wrappers that can execute REST requests for mock or custom connections.
    /// This is primarily used by test infrastructure where ServiceClient.ExecuteWebRequest is not fully initialised.
    /// </summary>
    public interface IRestCapableServiceClient
    {
        /// <summary>
        /// Executes a Dataverse Web API request.
        /// </summary>
        HttpResponseMessage ExecuteWebRequest(HttpMethod method, string path, string body, Dictionary<string, List<string>> headers, CancellationToken cancellationToken);
    }
}
