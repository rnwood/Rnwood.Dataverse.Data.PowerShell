using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "DataverseTeamMembers", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(AddMembersTeamResponse))]
    ///<summary>Adds members to a team.</summary>
    public class AddDataverseTeamMembersCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the team to add members to")]
        public Guid TeamId { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Array of user IDs to add as team members")]
        public Guid[] MemberIds { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            AddMembersTeamRequest request = new AddMembersTeamRequest
            {
                TeamId = TeamId,
                MemberIds = MemberIds
            };

            if (ShouldProcess($"Team {TeamId}", $"Add {MemberIds.Length} member(s)"))
            {
                AddMembersTeamResponse response = (AddMembersTeamResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
