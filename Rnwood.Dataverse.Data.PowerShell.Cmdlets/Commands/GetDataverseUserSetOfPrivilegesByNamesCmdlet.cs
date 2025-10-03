using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseUserSetOfPrivilegesByNames")]
    [OutputType(typeof(RetrieveUserSetOfPrivilegesByNamesResponse))]
    ///<summary>Executes RetrieveUserSetOfPrivilegesByNamesRequest SDK message.</summary>
    public class GetDataverseUserSetOfPrivilegesByNamesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UserId parameter")]
        public Guid UserId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PrivilegeNames parameter")]
        public String[] PrivilegeNames { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveUserSetOfPrivilegesByNamesRequest();
            request.UserId = UserId;            request.PrivilegeNames = PrivilegeNames;
            var response = (RetrieveUserSetOfPrivilegesByNamesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
