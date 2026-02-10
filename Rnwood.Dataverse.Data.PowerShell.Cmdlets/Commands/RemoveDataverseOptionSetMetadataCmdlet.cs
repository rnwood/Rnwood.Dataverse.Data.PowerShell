using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Management.Automation;
using System.ServiceModel;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Deletes a global option set from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseOptionSetMetadata", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseOptionSetMetadataCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the name of the global option set to delete.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the global option set to delete")]
        public string Name { get; set; }

        /// <summary>
        /// If specified, the cmdlet will not raise an error if the option set does not exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the option set does not exist")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!ShouldProcess($"Global option set '{Name}'", "Delete"))
            {
                return;
            }

            try
            {
                var request = new DeleteOptionSetRequest
                {
                    Name = Name
                };

                WriteVerbose($"Deleting global option set '{Name}'");

                Connection.Execute(request);

                WriteVerbose($"Global option set '{Name}' deleted successfully");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (IfExists && QueryHelpers.IsNotFoundException(ex))
                {
                    WriteVerbose($"Global option set '{Name}' does not exist: {ex.Message}");
                    return;
                }
                else
                {
                    throw;
                }
            }
            catch (FaultException ex)
            {
                if (IfExists && QueryHelpers.IsNotFoundException(ex))
                {
                    WriteVerbose($"Global option set '{Name}' does not exist: {ex.Message}");
                    return;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
