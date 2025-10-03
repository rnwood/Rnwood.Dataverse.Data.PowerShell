using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "DataverseListMembersList", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(AddListMembersListResponse))]
    ///<summary>Executes AddListMembersListRequest SDK message.</summary>
    public class AddDataverseListMembersListCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ListId parameter")]
        public Guid ListId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "MemberIds parameter")]
        public Guid[] MemberIds { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new AddListMembersListRequest();
            request.ListId = ListId;            request.MemberIds = MemberIds;
            if (ShouldProcess("Executing AddListMembersListRequest", "AddListMembersListRequest"))
            {
                var response = (AddListMembersListResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
