using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseTeamPrivileges")]
    [OutputType(typeof(RetrieveTeamPrivilegesResponse))]
    ///<summary>Executes RetrieveTeamPrivilegesRequest SDK message.</summary>
    public class GetDataverseTeamPrivilegesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TeamId parameter")]
        public Guid TeamId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveTeamPrivilegesRequest();
            request.TeamId = TeamId;
            var response = (RetrieveTeamPrivilegesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
