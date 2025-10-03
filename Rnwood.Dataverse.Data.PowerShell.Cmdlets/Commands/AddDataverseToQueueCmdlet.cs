using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "DataverseToQueue", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(AddToQueueResponse))]
    ///<summary>Executes AddToQueueRequest SDK message.</summary>
    public class AddDataverseToQueueCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SourceQueueId parameter")]
        public Guid SourceQueueId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DestinationQueueId parameter")]
        public Guid DestinationQueueId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QueueItemProperties parameter")]
        public PSObject QueueItemProperties { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new AddToQueueRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            request.SourceQueueId = SourceQueueId;            request.DestinationQueueId = DestinationQueueId;            if (QueueItemProperties != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in QueueItemProperties.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.QueueItemProperties = entity;
            }

            if (ShouldProcess("Executing AddToQueueRequest", "AddToQueueRequest"))
            {
                var response = (AddToQueueResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
