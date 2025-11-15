using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Waits for any in-progress Dataverse publish or solution operations to complete.
    /// This cmdlet polls the msdyn_solutionhistory table to check for operations that are Started (0) or Queued (2).
    /// </summary>
    [Cmdlet(VerbsLifecycle.Wait, "DataversePublish")]
    public class WaitDataversePublishCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the maximum time to wait in seconds. Default is 300 seconds (5 minutes).
        /// </summary>
        [Parameter(HelpMessage = "Maximum time to wait in seconds. Default is 300 seconds (5 minutes).")]
        public int MaxWaitSeconds { get; set; } = 300;

        /// <summary>
        /// Gets or sets the interval between polls in seconds. Default is 2 seconds.
        /// </summary>
        [Parameter(HelpMessage = "Interval between polls in seconds. Default is 2 seconds.")]
        public int PollIntervalSeconds { get; set; } = 2;

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose($"Waiting for Dataverse publish operations to complete (max wait: {MaxWaitSeconds}s, poll interval: {PollIntervalSeconds}s)...");

            PublishHelpers.WaitForPublishComplete(Connection, WriteVerbose, MaxWaitSeconds, PollIntervalSeconds);

            WriteVerbose("Publish wait complete.");
        }
    }
}
