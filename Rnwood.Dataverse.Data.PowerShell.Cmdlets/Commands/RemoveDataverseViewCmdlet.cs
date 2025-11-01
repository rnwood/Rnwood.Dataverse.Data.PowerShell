using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes a view (savedquery or userquery) from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseView", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseViewCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the view to remove.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the view to remove")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets whether this is a system view (savedquery) or personal view (userquery). Default is personal view.
        /// </summary>
        [Parameter(HelpMessage = "Remove a system view (savedquery) instead of a personal view (userquery)")]
        public SwitchParameter SystemView { get; set; }

        /// <summary>
        /// If specified, the cmdlet will not raise an error if the view does not exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the view does not exist")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Processes each record in the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string entityName = SystemView ? "savedquery" : "userquery";

            if (ShouldProcess($"{(SystemView ? "System" : "Personal")} view with ID '{Id}'", "Remove"))
            {
                try
                {
                    Connection.Delete(entityName, Id);
                    WriteVerbose($"Removed {(SystemView ? "system" : "personal")} view with ID: {Id}");
                }
                catch (Exception ex)
                {
                    if (IfExists)
                    {
                        WriteVerbose($"View with ID {Id} does not exist or could not be deleted: {ex.Message}");
                    }
                    else
                    {
                        WriteError(new ErrorRecord(ex, "RemoveDataverseViewError", ErrorCategory.InvalidOperation, Id));
                        throw;
                    }
                }
            }
        }
    }
}
