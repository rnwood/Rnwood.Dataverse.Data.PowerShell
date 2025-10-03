using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseUserPrivilegeByPrivilegeName")]
    [OutputType(typeof(RetrieveUserPrivilegeByPrivilegeNameResponse))]
    ///<summary>Executes RetrieveUserPrivilegeByPrivilegeNameRequest SDK message.</summary>
    public class GetDataverseUserPrivilegeByPrivilegeNameCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UserId parameter")]
        public Guid UserId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PrivilegeName parameter")]
        public String PrivilegeName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveUserPrivilegeByPrivilegeNameRequest();
            request.UserId = UserId;            request.PrivilegeName = PrivilegeName;
            var response = (RetrieveUserPrivilegeByPrivilegeNameResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
