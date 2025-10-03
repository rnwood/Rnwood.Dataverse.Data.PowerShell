using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseItemCampaignActivity", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RemoveItemCampaignActivityResponse))]
    ///<summary>Executes RemoveItemCampaignActivityRequest SDK message.</summary>
    public class RemoveDataverseItemCampaignActivityCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CampaignActivityId parameter")]
        public Guid CampaignActivityId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ItemId parameter")]
        public Guid ItemId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RemoveItemCampaignActivityRequest();
            request.CampaignActivityId = CampaignActivityId;            request.ItemId = ItemId;
            if (ShouldProcess("Executing RemoveItemCampaignActivityRequest", "RemoveItemCampaignActivityRequest"))
            {
                var response = (RemoveItemCampaignActivityResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
