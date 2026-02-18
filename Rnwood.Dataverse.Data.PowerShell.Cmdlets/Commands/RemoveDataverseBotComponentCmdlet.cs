using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Deletes a Copilot Studio bot component from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseBotComponent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseBotComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the bot component ID to delete.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Bot component ID (GUID) to delete.")]
        public Guid BotComponentId { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!ShouldProcess($"Bot component with ID {BotComponentId}", "Delete"))
            {
                return;
            }

            try
            {
                Connection.Delete("botcomponent", BotComponentId);
                WriteVerbose($"Deleted bot component with ID: {BotComponentId}");
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "DeleteBotComponentFailed",
                    ErrorCategory.InvalidOperation,
                    BotComponentId));
            }
        }
    }
}
