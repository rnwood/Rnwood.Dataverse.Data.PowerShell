using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseRolePrivilegesRole")]
    [OutputType(typeof(RetrieveRolePrivilegesRoleResponse))]
    ///<summary>Executes RetrieveRolePrivilegesRoleRequest SDK message.</summary>
    public class GetDataverseRolePrivilegesRoleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RoleId parameter")]
        public Guid RoleId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveRolePrivilegesRoleRequest();
            request.RoleId = RoleId;
            var response = (RetrieveRolePrivilegesRoleResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
