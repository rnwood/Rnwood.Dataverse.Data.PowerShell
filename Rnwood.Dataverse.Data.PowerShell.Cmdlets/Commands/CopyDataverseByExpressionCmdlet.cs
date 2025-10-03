using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Copy, "DataverseByExpression", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PropagateByExpressionResponse))]
    ///<summary>Executes PropagateByExpressionRequest SDK message.</summary>
    public class CopyDataverseByExpressionCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QueryExpression parameter")]
        public Microsoft.Xrm.Sdk.Query.QueryBase QueryExpression { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FriendlyName parameter")]
        public String FriendlyName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExecuteImmediately parameter")]
        public Boolean ExecuteImmediately { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Activity parameter")]
        public PSObject Activity { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TemplateId parameter")]
        public Guid TemplateId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OwnershipOptions parameter")]
        public Microsoft.Crm.Sdk.Messages.PropagationOwnershipOptions OwnershipOptions { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PostWorkflowEvent parameter")]
        public Boolean PostWorkflowEvent { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Owner parameter")]
        public object Owner { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SendEmail parameter")]
        public Boolean SendEmail { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QueueId parameter")]
        public Guid QueueId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new PropagateByExpressionRequest();
            request.QueryExpression = QueryExpression;            request.FriendlyName = FriendlyName;            request.ExecuteImmediately = ExecuteImmediately;            if (Activity != null)
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
            request.TemplateId = TemplateId;            request.OwnershipOptions = OwnershipOptions;            request.PostWorkflowEvent = PostWorkflowEvent;            if (Owner != null)
            {
                request.Owner = DataverseTypeConverter.ToEntityReference(Owner, null, "Owner");
            }
            request.SendEmail = SendEmail;            request.QueueId = QueueId;
            if (ShouldProcess("Executing PropagateByExpressionRequest", "PropagateByExpressionRequest"))
            {
                var response = (PropagateByExpressionResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
