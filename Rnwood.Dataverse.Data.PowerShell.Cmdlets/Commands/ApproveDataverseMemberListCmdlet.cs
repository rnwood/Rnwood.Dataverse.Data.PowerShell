using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Approve, "DataverseMemberList")]
    [OutputType(typeof(QualifyMemberListResponse))]
    ///<summary>Executes QualifyMemberListRequest SDK message.</summary>
    public class ApproveDataverseMemberListCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ListId parameter")]
        public Guid ListId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "MembersId parameter")]
        public Guid[] MembersId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OverrideorRemove parameter")]
        public Boolean OverrideorRemove { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new QualifyMemberListRequest();
            request.ListId = ListId;            request.MembersId = MembersId;            request.OverrideorRemove = OverrideorRemove;
            var response = (QualifyMemberListResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
