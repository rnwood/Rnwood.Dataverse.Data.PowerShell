using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataversePrivilegeSet")]
    [OutputType(typeof(RetrievePrivilegeSetResponse))]
    ///<summary>Executes RetrievePrivilegeSetRequest SDK message.</summary>
    public class GetDataversePrivilegeSetCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrievePrivilegeSetRequest();

            var response = (RetrievePrivilegeSetResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
