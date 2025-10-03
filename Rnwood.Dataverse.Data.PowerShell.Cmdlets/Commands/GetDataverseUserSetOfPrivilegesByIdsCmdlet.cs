using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseUserSetOfPrivilegesByIds")]
    [OutputType(typeof(RetrieveUserSetOfPrivilegesByIdsResponse))]
    ///<summary>Executes RetrieveUserSetOfPrivilegesByIdsRequest SDK message.</summary>
    public class GetDataverseUserSetOfPrivilegesByIdsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UserId parameter")]
        public Guid UserId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PrivilegeIds parameter")]
        public Guid[] PrivilegeIds { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveUserSetOfPrivilegesByIdsRequest();
            request.UserId = UserId;            request.PrivilegeIds = PrivilegeIds;
            var response = (RetrieveUserSetOfPrivilegesByIdsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
