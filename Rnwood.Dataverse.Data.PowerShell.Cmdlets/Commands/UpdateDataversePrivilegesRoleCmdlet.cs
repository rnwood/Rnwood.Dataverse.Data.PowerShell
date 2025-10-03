using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Update, "DataversePrivilegesRole", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ReplacePrivilegesRoleResponse))]
    ///<summary>Executes ReplacePrivilegesRoleRequest SDK message.</summary>
    public class UpdateDataversePrivilegesRoleCmdlet : OrganizationServiceCmdlet
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

            var request = new ReplacePrivilegesRoleRequest();
            request.RoleId = RoleId;            request.Privileges = Privileges;
            if (ShouldProcess("Executing ReplacePrivilegesRoleRequest", "ReplacePrivilegesRoleRequest"))
            {
                var response = (ReplacePrivilegesRoleResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
