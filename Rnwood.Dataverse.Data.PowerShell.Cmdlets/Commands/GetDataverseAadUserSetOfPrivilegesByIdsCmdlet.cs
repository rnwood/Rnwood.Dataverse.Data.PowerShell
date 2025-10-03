using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseAadUserSetOfPrivilegesByIds")]
    [OutputType(typeof(RetrieveAadUserSetOfPrivilegesByIdsResponse))]
    ///<summary>Executes RetrieveAadUserSetOfPrivilegesByIdsRequest SDK message.</summary>
    public class GetDataverseAadUserSetOfPrivilegesByIdsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DirectoryObjectId parameter")]
        public Guid DirectoryObjectId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PrivilegeIds parameter")]
        public Guid[] PrivilegeIds { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveAadUserSetOfPrivilegesByIdsRequest();
            request.DirectoryObjectId = DirectoryObjectId;            request.PrivilegeIds = PrivilegeIds;
            var response = (RetrieveAadUserSetOfPrivilegesByIdsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
