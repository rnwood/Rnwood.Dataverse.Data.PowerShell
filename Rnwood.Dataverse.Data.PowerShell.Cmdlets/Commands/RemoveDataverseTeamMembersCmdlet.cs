using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseTeamMembers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(RemoveMembersTeamResponse))]
    ///<summary>Removes members from a team.</summary>
    public class RemoveDataverseTeamMembersCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the team to remove members from")]
        public Guid TeamId { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Array of user IDs to remove from team members")]
        public Guid[] MemberIds { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            RemoveMembersTeamRequest request = new RemoveMembersTeamRequest
            {
                TeamId = TeamId,
                MemberIds = MemberIds
            };

            if (ShouldProcess($"Team {TeamId}", $"Remove {MemberIds.Length} member(s)"))
            {
                RemoveMembersTeamResponse response = (RemoveMembersTeamResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
