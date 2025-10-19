using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Sets the specified Dataverse connection as the default connection for cmdlets that don't specify a connection.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseConnectionAsDefault")]
    public class SetDataverseConnectionAsDefaultCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the Dataverse connection to set as default.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet.")]
        public ServiceClient Connection { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            DefaultConnectionManager.DefaultConnection = Connection;
            WriteVerbose("Default Dataverse connection set.");
        }
    }
}