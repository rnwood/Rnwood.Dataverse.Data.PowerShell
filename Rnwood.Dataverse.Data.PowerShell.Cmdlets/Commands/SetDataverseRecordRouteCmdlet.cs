using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseRecordRoute", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RouteToResponse))]
    ///<summary>Routes a queue item to a queue, user, or team.</summary>
    public class SetDataverseRecordRouteCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Reference to the queue item or record to route. Can be an EntityReference, PSObject, or Guid with TableName.")]
        public object Target { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the table when Target is specified as a Guid")]
        [Alias("EntityName", "LogicalName")]
        public string TableName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "ID of the queue item to route")]
        public Guid QueueItemId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            RouteToRequest request = new RouteToRequest();

            // Convert Target to EntityReference
            request.Target = DataverseTypeConverter.ToEntityReference(Target, TableName, "Target");
            request.QueueItemId = QueueItemId;

            if (ShouldProcess($"Queue item {QueueItemId}", $"Route to {request.Target.LogicalName} {request.Target.Id}"))
            {
                RouteToResponse response = (RouteToResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
