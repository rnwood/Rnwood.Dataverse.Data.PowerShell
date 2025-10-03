using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataversePrivilegeRole", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RemovePrivilegeRoleResponse))]
    ///<summary>Executes RemovePrivilegeRoleRequest SDK message.</summary>
    public class RemoveDataversePrivilegeRoleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RoleId parameter")]
        public Guid RoleId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PrivilegeId parameter")]
        public Guid PrivilegeId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RemovePrivilegeRoleRequest();
            request.RoleId = RoleId;            request.PrivilegeId = PrivilegeId;
            if (ShouldProcess("Executing RemovePrivilegeRoleRequest", "RemovePrivilegeRoleRequest"))
            {
                var response = (RemovePrivilegeRoleResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
