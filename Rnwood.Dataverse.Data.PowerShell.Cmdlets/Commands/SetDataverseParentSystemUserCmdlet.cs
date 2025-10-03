using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseParentSystemUser", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SetParentSystemUserResponse))]
    ///<summary>Executes SetParentSystemUserRequest SDK message.</summary>
    public class SetDataverseParentSystemUserCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UserId parameter")]
        public Guid UserId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ParentId parameter")]
        public Guid ParentId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "KeepChildUsers parameter")]
        public Boolean KeepChildUsers { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SetParentSystemUserRequest();
            request.UserId = UserId;            request.ParentId = ParentId;            request.KeepChildUsers = KeepChildUsers;
            if (ShouldProcess("Executing SetParentSystemUserRequest", "SetParentSystemUserRequest"))
            {
                var response = (SetParentSystemUserResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
