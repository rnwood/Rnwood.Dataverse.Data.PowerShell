using Microsoft.PowerPlatform.Dataverse.Client;

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
}
