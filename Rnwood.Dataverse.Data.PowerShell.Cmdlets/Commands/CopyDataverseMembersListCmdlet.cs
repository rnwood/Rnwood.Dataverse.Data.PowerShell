using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Copy, "DataverseMembersList", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CopyMembersListResponse))]
    ///<summary>Executes CopyMembersListRequest SDK message.</summary>
    public class CopyDataverseMembersListCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SourceListId parameter")]
        public Guid SourceListId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TargetListId parameter")]
        public Guid TargetListId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CopyMembersListRequest();
            request.SourceListId = SourceListId;            request.TargetListId = TargetListId;
            if (ShouldProcess("Executing CopyMembersListRequest", "CopyMembersListRequest"))
            {
                var response = (CopyMembersListResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
