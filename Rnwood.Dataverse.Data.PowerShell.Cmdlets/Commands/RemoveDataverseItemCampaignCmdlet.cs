using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseItemCampaign", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RemoveItemCampaignResponse))]
    ///<summary>Executes RemoveItemCampaignRequest SDK message.</summary>
    public class RemoveDataverseItemCampaignCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CampaignId parameter")]
        public Guid CampaignId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityId parameter")]
        public Guid EntityId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RemoveItemCampaignRequest();
            request.CampaignId = CampaignId;            request.EntityId = EntityId;
            if (ShouldProcess("Executing RemoveItemCampaignRequest", "RemoveItemCampaignRequest"))
            {
                var response = (RemoveItemCampaignResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
