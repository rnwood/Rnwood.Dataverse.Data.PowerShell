using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Copy, "DataverseCampaignResponse", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CopyCampaignResponseResponse))]
    ///<summary>Executes CopyCampaignResponseRequest SDK message.</summary>
    public class CopyDataverseCampaignResponseCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "CampaignResponseId parameter")]
        public object CampaignResponseId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CopyCampaignResponseRequest();
            if (CampaignResponseId != null)
            {
                request.CampaignResponseId = DataverseTypeConverter.ToEntityReference(CampaignResponseId, null, "CampaignResponseId");
            }

            if (ShouldProcess("Executing CopyCampaignResponseRequest", "CopyCampaignResponseRequest"))
            {
                var response = (CopyCampaignResponseResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
