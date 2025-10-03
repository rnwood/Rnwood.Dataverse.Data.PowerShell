using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseUserPrivilegeByPrivilegeId")]
    [OutputType(typeof(RetrieveUserPrivilegeByPrivilegeIdResponse))]
    ///<summary>Executes RetrieveUserPrivilegeByPrivilegeIdRequest SDK message.</summary>
    public class GetDataverseUserPrivilegeByPrivilegeIdCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UserId parameter")]
        public Guid UserId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PrivilegeId parameter")]
        public Guid PrivilegeId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveUserPrivilegeByPrivilegeIdRequest();
            request.UserId = UserId;            request.PrivilegeId = PrivilegeId;
            var response = (RetrieveUserPrivilegeByPrivilegeIdResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
