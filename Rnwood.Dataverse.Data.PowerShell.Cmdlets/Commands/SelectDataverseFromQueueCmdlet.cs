using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Select, "DataverseFromQueue")]
    [OutputType(typeof(PickFromQueueResponse))]
    ///<summary>Executes PickFromQueueRequest SDK message.</summary>
    public class SelectDataverseFromQueueCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QueueItemId parameter")]
        public Guid QueueItemId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "WorkerId parameter")]
        public Guid WorkerId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RemoveQueueItem parameter")]
        public Boolean RemoveQueueItem { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new PickFromQueueRequest();
            request.QueueItemId = QueueItemId;            request.WorkerId = WorkerId;            request.RemoveQueueItem = RemoveQueueItem;
            var response = (PickFromQueueResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
