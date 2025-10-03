using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseAadUserRoles")]
    [OutputType(typeof(RetrieveAadUserRolesResponse))]
    ///<summary>Executes RetrieveAadUserRolesRequest SDK message.</summary>
    public class GetDataverseAadUserRolesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DirectoryObjectId parameter")]
        public Guid DirectoryObjectId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ColumnSet parameter")]
        public Microsoft.Xrm.Sdk.Query.ColumnSet ColumnSet { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveAadUserRolesRequest();
            request.DirectoryObjectId = DirectoryObjectId;            request.ColumnSet = ColumnSet;
            var response = (RetrieveAadUserRolesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
