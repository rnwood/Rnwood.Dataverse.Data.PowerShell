using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseUserPrivileges")]
    [OutputType(typeof(RetrieveUserPrivilegesResponse))]
    ///<summary>Executes RetrieveUserPrivilegesRequest SDK message.</summary>
    public class GetDataverseUserPrivilegesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UserId parameter")]
        public Guid UserId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveUserPrivilegesRequest();
            request.UserId = UserId;
            var response = (RetrieveUserPrivilegesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
