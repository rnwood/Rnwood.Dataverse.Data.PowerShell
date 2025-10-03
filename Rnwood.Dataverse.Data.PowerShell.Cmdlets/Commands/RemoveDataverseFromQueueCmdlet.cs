using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseFromQueue", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RemoveFromQueueResponse))]
    ///<summary>Executes RemoveFromQueueRequest SDK message.</summary>
    public class RemoveDataverseFromQueueCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QueueItemId parameter")]
        public Guid QueueItemId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RemoveFromQueueRequest();
            request.QueueItemId = QueueItemId;
            if (ShouldProcess("Executing RemoveFromQueueRequest", "RemoveFromQueueRequest"))
            {
                var response = (RemoveFromQueueResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
