using Microsoft.PowerPlatform.Dataverse.Client;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Utility methods for argument completers.
    /// </summary>
    internal static class ArgumentCompleterUtils
    {
        /// <summary>
        /// Extracts the ServiceClient connection from the bound parameters.
        /// Looks for a parameter named "Connection" (case-insensitive) and handles both direct ServiceClient instances
        /// and PSObject wrappers containing a ServiceClient as the base object.
        /// If no "Connection" parameter is found, uses the default connection from Get-DataverseConnection.
        /// </summary>
        /// <param name="fakeBoundParameters">The dictionary of bound parameters provided by PowerShell.</param>
        /// <returns>The ServiceClient instance if found, otherwise the default connection.</returns>
        public static ServiceClient GetConnection(IDictionary fakeBoundParameters)
        {
            if (fakeBoundParameters == null)
            {
                return DefaultConnectionManager.DefaultConnection;
            }

            if (fakeBoundParameters.Contains("Connection"))
            {
                var value = fakeBoundParameters["Connection"];

                if (value is ServiceClient sc)
                {
                    return sc;
                }
                else if (value is PSObject pso && pso.BaseObject is ServiceClient sc2)
                {
                    return sc2;
                }

                return null;
            }



            return DefaultConnectionManager.DefaultConnection;
        }
    }
}