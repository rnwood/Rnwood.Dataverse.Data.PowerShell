using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "DataversePrivilegesRole", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(AddPrivilegesRoleResponse))]
    ///<summary>Executes AddPrivilegesRoleRequest SDK message.</summary>
    public class AddDataversePrivilegesRoleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RoleId parameter")]
        public Guid RoleId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Privileges parameter")]
        public Microsoft.Crm.Sdk.Messages.RolePrivilege[] Privileges { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new AddPrivilegesRoleRequest();
            request.RoleId = RoleId;            request.Privileges = Privileges;
            if (ShouldProcess("Executing AddPrivilegesRoleRequest", "AddPrivilegesRoleRequest"))
            {
                var response = (AddPrivilegesRoleResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
