using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Deletes a Copilot Studio bot from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseBot", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseBotCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the bot ID to delete.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Bot ID (GUID) to delete.")]
        public Guid BotId { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!ShouldProcess($"Bot with ID {BotId}", "Delete"))
            {
                return;
            }

            try
            {
                Connection.Delete("bot", BotId);
                WriteVerbose($"Deleted bot with ID: {BotId}");
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "DeleteBotFailed",
                    ErrorCategory.InvalidOperation,
                    BotId));
            }
        }
    }
}
