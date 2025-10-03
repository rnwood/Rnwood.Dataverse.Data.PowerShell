using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "DataverseItemCampaign", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(AddItemCampaignResponse))]
    ///<summary>Executes AddItemCampaignRequest SDK message.</summary>
    public class AddDataverseItemCampaignCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CampaignId parameter")]
        public Guid CampaignId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityId parameter")]
        public Guid EntityId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityName parameter")]
        public String EntityName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new AddItemCampaignRequest();
            request.CampaignId = CampaignId;            request.EntityId = EntityId;            request.EntityName = EntityName;
            if (ShouldProcess("Executing AddItemCampaignRequest", "AddItemCampaignRequest"))
            {
                var response = (AddItemCampaignResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
