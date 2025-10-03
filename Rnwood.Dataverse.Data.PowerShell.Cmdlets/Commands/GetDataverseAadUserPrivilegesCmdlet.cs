using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseAadUserPrivileges")]
    [OutputType(typeof(RetrieveAadUserPrivilegesResponse))]
    ///<summary>Executes RetrieveAadUserPrivilegesRequest SDK message.</summary>
    public class GetDataverseAadUserPrivilegesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DirectoryObjectId parameter")]
        public Guid DirectoryObjectId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveAadUserPrivilegesRequest();
            request.DirectoryObjectId = DirectoryObjectId;
            var response = (RetrieveAadUserPrivilegesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
