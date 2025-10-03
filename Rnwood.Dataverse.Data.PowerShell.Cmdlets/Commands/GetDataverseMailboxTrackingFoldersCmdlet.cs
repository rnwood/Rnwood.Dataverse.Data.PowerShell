using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseMailboxTrackingFolders")]
    [OutputType(typeof(RetrieveMailboxTrackingFoldersResponse))]
    ///<summary>Executes RetrieveMailboxTrackingFoldersRequest SDK message.</summary>
    public class GetDataverseMailboxTrackingFoldersCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "MailboxId parameter")]
        public String MailboxId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveMailboxTrackingFoldersRequest();
            request.MailboxId = MailboxId;
            var response = (RetrieveMailboxTrackingFoldersResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
