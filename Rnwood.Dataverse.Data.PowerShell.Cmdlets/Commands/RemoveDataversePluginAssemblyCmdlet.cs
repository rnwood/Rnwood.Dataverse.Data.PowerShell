using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes a plugin assembly from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataversePluginAssembly", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataversePluginAssemblyCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin assembly to remove.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin assembly to remove")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to suppress errors if the assembly doesn't exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the assembly does not exist")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (ShouldProcess($"Plugin Assembly ID: {Id}", "Remove"))
            {
                try
                {
                    Connection.Delete("pluginassembly", Id);
                    WriteVerbose($"Removed plugin assembly with ID: {Id}");
                }
                catch (Exception ex)
                {
                    if (!IfExists)
                    {
                        throw;
                    }
                    WriteVerbose($"Plugin assembly with ID {Id} may not exist: {ex.Message}");
                }
            }
        }
    }
}
