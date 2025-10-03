using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseUserQueues")]
    [OutputType(typeof(RetrieveUserQueuesResponse))]
    ///<summary>Executes RetrieveUserQueuesRequest SDK message.</summary>
    public class GetDataverseUserQueuesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UserId parameter")]
        public Guid UserId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IncludePublic parameter")]
        public Boolean IncludePublic { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveUserQueuesRequest();
            request.UserId = UserId;            request.IncludePublic = IncludePublic;
            var response = (RetrieveUserQueuesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
