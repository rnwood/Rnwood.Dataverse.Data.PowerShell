using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "DataverseItemCampaignActivity", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(AddItemCampaignActivityResponse))]
    ///<summary>Executes AddItemCampaignActivityRequest SDK message.</summary>
    public class AddDataverseItemCampaignActivityCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CampaignActivityId parameter")]
        public Guid CampaignActivityId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ItemId parameter")]
        public Guid ItemId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityName parameter")]
        public String EntityName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new AddItemCampaignActivityRequest();
            request.CampaignActivityId = CampaignActivityId;            request.ItemId = ItemId;            request.EntityName = EntityName;
            if (ShouldProcess("Executing AddItemCampaignActivityRequest", "AddItemCampaignActivityRequest"))
            {
                var response = (AddItemCampaignActivityResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
