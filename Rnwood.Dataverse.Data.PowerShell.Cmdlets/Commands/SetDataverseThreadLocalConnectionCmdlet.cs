using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Sets the thread-local Dataverse connection for the current thread.
    /// This is primarily used internally by Invoke-DataverseParallel for worker threads.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseThreadLocalConnection")]
    public class SetDataverseThreadLocalConnectionCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the Dataverse connection to set as thread-local default.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet.")]
        public ServiceClient Connection { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            DefaultConnectionManager.ThreadLocalConnection = Connection;
            WriteVerbose("Thread-local Dataverse connection set.");
        }
    }
}
