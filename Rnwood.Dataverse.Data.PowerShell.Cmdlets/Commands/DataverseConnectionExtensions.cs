using System;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Provides extension methods for cloning Dataverse connections.
    /// </summary>
    public static class DataverseConnectionExtensions
    {
        // Internal hook for test infrastructure to provide custom clone implementations
        internal static Func<ServiceClient, ICloneableServiceClient> GetCloneableWrapper { get; set; }

        /// <summary>
        /// Attempts to clone a ServiceClient. If the connection implements ICloneableServiceClient,
        /// uses the custom clone logic. Otherwise, uses the standard Clone() method.
        /// </summary>
        /// <param name="serviceClient">The ServiceClient to clone</param>
        /// <returns>A cloned ServiceClient</returns>
        public static ServiceClient CloneConnection(ServiceClient serviceClient)
        {
            // Check if this connection implements custom clone logic directly
            if (serviceClient is ICloneableServiceClient cloneable)
            {
                return cloneable.CloneServiceClient();
            }

            // Check if test infrastructure registered a wrapper for this connection
            if (GetCloneableWrapper != null)
            {
                var wrapper = GetCloneableWrapper(serviceClient);
                if (wrapper != null)
                {
                    return wrapper.CloneServiceClient();
                }
            }

            // Otherwise, use the standard Clone() method
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
