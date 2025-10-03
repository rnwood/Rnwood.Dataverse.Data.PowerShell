using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "DataverseActivitiesList", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CreateActivitiesListResponse))]
    ///<summary>Executes CreateActivitiesListRequest SDK message.</summary>
    public class NewDataverseActivitiesListCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ListId parameter")]
        public Guid ListId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FriendlyName parameter")]
        public String FriendlyName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Activity parameter")]
        public PSObject Activity { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TemplateId parameter")]
        public Guid TemplateId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Propagate parameter")]
        public Boolean Propagate { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OwnershipOptions parameter")]
        public Microsoft.Crm.Sdk.Messages.PropagationOwnershipOptions OwnershipOptions { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Owner parameter")]
        public object Owner { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "sendEmail parameter")]
        public Boolean sendEmail { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PostWorkflowEvent parameter")]
        public Boolean PostWorkflowEvent { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QueueId parameter")]
        public Guid QueueId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CreateActivitiesListRequest();
            request.ListId = ListId;            request.FriendlyName = FriendlyName;            if (Activity != null)
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
            request.TemplateId = TemplateId;            request.Propagate = Propagate;            request.OwnershipOptions = OwnershipOptions;            if (Owner != null)
            {
                request.Owner = DataverseTypeConverter.ToEntityReference(Owner, null, "Owner");
            }
            request.sendEmail = sendEmail;            request.PostWorkflowEvent = PostWorkflowEvent;            request.QueueId = QueueId;
            if (ShouldProcess("Executing CreateActivitiesListRequest", "CreateActivitiesListRequest"))
            {
                var response = (CreateActivitiesListResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
