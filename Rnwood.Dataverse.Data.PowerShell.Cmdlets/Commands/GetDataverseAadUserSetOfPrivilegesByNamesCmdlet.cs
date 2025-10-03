using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseAadUserSetOfPrivilegesByNames")]
    [OutputType(typeof(RetrieveAadUserSetOfPrivilegesByNamesResponse))]
    ///<summary>Executes RetrieveAadUserSetOfPrivilegesByNamesRequest SDK message.</summary>
    public class GetDataverseAadUserSetOfPrivilegesByNamesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DirectoryObjectId parameter")]
        public Guid DirectoryObjectId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PrivilegeNames parameter")]
        public String[] PrivilegeNames { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveAadUserSetOfPrivilegesByNamesRequest();
            request.DirectoryObjectId = DirectoryObjectId;            request.PrivilegeNames = PrivilegeNames;
            var response = (RetrieveAadUserSetOfPrivilegesByNamesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
