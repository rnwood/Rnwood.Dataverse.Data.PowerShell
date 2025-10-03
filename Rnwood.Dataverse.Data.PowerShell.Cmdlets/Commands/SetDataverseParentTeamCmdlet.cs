using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseParentTeam", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SetParentTeamResponse))]
    ///<summary>Executes SetParentTeamRequest SDK message.</summary>
    public class SetDataverseParentTeamCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TeamId parameter")]
        public Guid TeamId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "BusinessId parameter")]
        public Guid BusinessId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DoNotMoveAllRecords parameter")]
        public Boolean DoNotMoveAllRecords { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SetParentTeamRequest();
            request.TeamId = TeamId;            request.BusinessId = BusinessId;            request.DoNotMoveAllRecords = DoNotMoveAllRecords;
            if (ShouldProcess("Executing SetParentTeamRequest", "SetParentTeamRequest"))
            {
                var response = (SetParentTeamResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
