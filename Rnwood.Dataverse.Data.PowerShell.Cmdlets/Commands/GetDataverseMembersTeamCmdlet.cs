using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseMembersTeam")]
    [OutputType(typeof(RetrieveMembersTeamResponse))]
    ///<summary>Executes RetrieveMembersTeamRequest SDK message.</summary>
    public class GetDataverseMembersTeamCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityId parameter")]
        public Guid EntityId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "MemberColumnSet parameter")]
        public Microsoft.Xrm.Sdk.Query.ColumnSet MemberColumnSet { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveMembersTeamRequest();
            request.EntityId = EntityId;            request.MemberColumnSet = MemberColumnSet;
            var response = (RetrieveMembersTeamResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
