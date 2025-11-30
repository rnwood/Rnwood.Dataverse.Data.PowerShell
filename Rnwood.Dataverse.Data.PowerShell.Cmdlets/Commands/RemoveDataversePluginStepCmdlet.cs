using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes an SDK message processing step (plugin step) from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataversePluginStep", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataversePluginStepCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin step to remove.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin step to remove")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to suppress errors if the step doesn't exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the step does not exist")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (ShouldProcess($"Plugin Step ID: {Id}", "Remove"))
            {
                try
                {
                    QueryHelpers.DeleteWithThrottlingRetry(Connection, "sdkmessageprocessingstep", Id);
                    WriteVerbose($"Removed plugin step with ID: {Id}");
                }
                catch (Exception ex)
                {
                    if (!IfExists)
                    {
                        throw;
                    }
                    WriteVerbose($"Plugin step with ID {Id} may not exist: {ex.Message}");
                }
            }
        }
    }
}
