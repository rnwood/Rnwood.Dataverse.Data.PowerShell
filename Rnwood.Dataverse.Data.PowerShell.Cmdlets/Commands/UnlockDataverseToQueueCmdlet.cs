using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Unlock, "DataverseToQueue", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ReleaseToQueueResponse))]
    ///<summary>Executes ReleaseToQueueRequest SDK message.</summary>
    public class UnlockDataverseToQueueCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QueueItemId parameter")]
        public Guid QueueItemId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ReleaseToQueueRequest();
            request.QueueItemId = QueueItemId;
            if (ShouldProcess("Executing ReleaseToQueueRequest", "ReleaseToQueueRequest"))
            {
                var response = (ReleaseToQueueResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
