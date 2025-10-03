using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommunications.Send, "DataverseCampaignActivity", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(DistributeCampaignActivityResponse))]
    ///<summary>Executes DistributeCampaignActivityRequest SDK message.</summary>
    public class SendDataverseCampaignActivityCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CampaignActivityId parameter")]
        public Guid CampaignActivityId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Propagate parameter")]
        public Boolean Propagate { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Activity parameter")]
        public PSObject Activity { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TemplateId parameter")]
        public Guid TemplateId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OwnershipOptions parameter")]
        public Microsoft.Crm.Sdk.Messages.PropagationOwnershipOptions OwnershipOptions { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Owner parameter")]
        public object Owner { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SendEmail parameter")]
        public Boolean SendEmail { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QueueId parameter")]
        public Guid QueueId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PostWorkflowEvent parameter")]
        public Boolean PostWorkflowEvent { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new DistributeCampaignActivityRequest();
            request.CampaignActivityId = CampaignActivityId;            request.Propagate = Propagate;            if (Activity != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in Activity.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.Activity = entity;
            }
            request.TemplateId = TemplateId;            request.OwnershipOptions = OwnershipOptions;            if (Owner != null)
            {
                request.Owner = DataverseTypeConverter.ToEntityReference(Owner, null, "Owner");
            }
            request.SendEmail = SendEmail;            request.QueueId = QueueId;            request.PostWorkflowEvent = PostWorkflowEvent;
            if (ShouldProcess("Executing DistributeCampaignActivityRequest", "DistributeCampaignActivityRequest"))
            {
                var response = (DistributeCampaignActivityResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
