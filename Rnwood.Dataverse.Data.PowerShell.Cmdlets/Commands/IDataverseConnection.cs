using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Interface for Dataverse connections that support cloning.
    /// This abstraction allows mock connections to provide custom Clone() implementations.
    /// </summary>
    public interface IDataverseConnection
    {
        /// <summary>
        /// Gets the underlying ServiceClient instance.
        /// </summary>
        ServiceClient ServiceClient { get; }

        /// <summary>
        /// Creates a clone of this connection for use in parallel operations.
        /// </summary>
        /// <returns>A new connection instance that can be used concurrently</returns>
        IDataverseConnection Clone();
    }
}
