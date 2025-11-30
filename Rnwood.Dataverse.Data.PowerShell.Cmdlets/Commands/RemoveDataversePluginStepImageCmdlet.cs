using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes an SDK message processing step image (plugin step image) from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataversePluginStepImage", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataversePluginStepImageCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin step image to remove.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin step image to remove")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to suppress errors if the image doesn't exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the image does not exist")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (ShouldProcess($"Plugin Step Image ID: {Id}", "Remove"))
            {
                try
                {
                    QueryHelpers.DeleteWithThrottlingRetry(Connection, "sdkmessageprocessingstepimage", Id);
                    WriteVerbose($"Removed plugin step image with ID: {Id}");
                }
                catch (Exception ex)
                {
                    if (!IfExists)
                    {
                        throw;
                    }
                    WriteVerbose($"Plugin step image with ID {Id} may not exist: {ex.Message}");
                }
            }
        }
    }
}
