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
        private const string SetParameterSet = "Set";
        private const string ClearParameterSet = "Clear";

        /// <summary>
        /// Gets or sets the Dataverse connection to set as default.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = SetParameterSet, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet.")]
        public ServiceClient Connection { get; set; }

        /// <summary>
        /// Clears the default connection when specified.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ClearParameterSet, HelpMessage = "Clear the default Dataverse connection.")]
        public SwitchParameter Clear { get; set; }

        /// <summary>
        /// Sets the default connection programmatically (for tests).
        /// </summary>
        public static void SetDefault(ServiceClient connection) => DefaultConnectionManager.DefaultConnection = connection;

        /// <summary>
        /// Clears the default connection programmatically (for tests).
        /// </summary>
        public static void ClearDefault() => DefaultConnectionManager.ClearDefaultConnection();

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (Clear.IsPresent)
            {
                DefaultConnectionManager.ClearDefaultConnection();
                WriteVerbose("Default Dataverse connection cleared.");
                return;
            }

            if (Connection == null)
            {
                ThrowTerminatingError(new ErrorRecord(new System.InvalidOperationException("No connection provided"), "NoConnection", ErrorCategory.InvalidArgument, null));
                return;
            }

            DefaultConnectionManager.DefaultConnection = Connection;
            WriteVerbose("Default Dataverse connection set.");
        }
    }
}