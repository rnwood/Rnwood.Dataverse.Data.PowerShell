using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseUsersPrivilegesThroughTeams")]
    [OutputType(typeof(RetrieveUsersPrivilegesThroughTeamsResponse))]
    ///<summary>Executes RetrieveUsersPrivilegesThroughTeamsRequest SDK message.</summary>
    public class GetDataverseUsersPrivilegesThroughTeamsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UserId parameter")]
        public Guid UserId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExcludeOrgDisabledPrivileges parameter")]
        public Boolean ExcludeOrgDisabledPrivileges { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IncludeSetupUserFiltering parameter")]
        public Boolean IncludeSetupUserFiltering { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveUsersPrivilegesThroughTeamsRequest();
            request.UserId = UserId;            request.ExcludeOrgDisabledPrivileges = ExcludeOrgDisabledPrivileges;            request.IncludeSetupUserFiltering = IncludeSetupUserFiltering;
            var response = (RetrieveUsersPrivilegesThroughTeamsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
