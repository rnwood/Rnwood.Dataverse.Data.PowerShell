using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Convert, "DataverseOwnerTeamToAccessTeam", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ConvertOwnerTeamToAccessTeamResponse))]
    ///<summary>Executes ConvertOwnerTeamToAccessTeamRequest SDK message.</summary>
    public class ConvertDataverseOwnerTeamToAccessTeamCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TeamId parameter")]
        public Guid TeamId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ConvertOwnerTeamToAccessTeamRequest();
            request.TeamId = TeamId;
            if (ShouldProcess("Executing ConvertOwnerTeamToAccessTeamRequest", "ConvertOwnerTeamToAccessTeamRequest"))
            {
                var response = (ConvertOwnerTeamToAccessTeamResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
