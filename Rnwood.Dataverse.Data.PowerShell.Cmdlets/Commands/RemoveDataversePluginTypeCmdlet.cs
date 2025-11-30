using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes a plugin type from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataversePluginType", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataversePluginTypeCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin type to remove.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin type to remove")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to suppress errors if the type doesn't exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the type does not exist")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (ShouldProcess($"Plugin Type ID: {Id}", "Remove"))
            {
                try
                {
                    QueryHelpers.DeleteWithThrottlingRetry(Connection, "plugintype", Id);
                    WriteVerbose($"Removed plugin type with ID: {Id}");
                }
                catch (Exception ex)
                {
                    if (!IfExists)
                    {
                        throw;
                    }
                    WriteVerbose($"Plugin type with ID {Id} may not exist: {ex.Message}");
                }
            }
        }
    }
}
