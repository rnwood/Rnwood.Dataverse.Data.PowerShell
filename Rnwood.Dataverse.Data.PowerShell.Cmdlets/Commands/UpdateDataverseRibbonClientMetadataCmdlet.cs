using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Update, "DataverseRibbonClientMetadata", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(UpdateRibbonClientMetadataResponse))]
    ///<summary>Executes UpdateRibbonClientMetadataRequest SDK message.</summary>
    public class UpdateDataverseRibbonClientMetadataCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new UpdateRibbonClientMetadataRequest();

            if (ShouldProcess("Executing UpdateRibbonClientMetadataRequest", "UpdateRibbonClientMetadataRequest"))
            {
                var response = (UpdateRibbonClientMetadataResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
