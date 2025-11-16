using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes a plugin package from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataversePluginPackage", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataversePluginPackageCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin package to remove.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin package to remove")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to suppress errors if the package doesn't exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the package does not exist")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (ShouldProcess($"Plugin Package ID: {Id}", "Remove"))
            {
                try
                {
                    Connection.Delete("pluginpackage", Id);
                    WriteVerbose($"Removed plugin package with ID: {Id}");
                }
                catch (Exception ex)
                {
                    if (!IfExists)
                    {
                        throw;
                    }
                    WriteVerbose($"Plugin package with ID {Id} may not exist: {ex.Message}");
                }
            }
        }
    }
}
