using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseIsBackOfficeInstalled")]
    [OutputType(typeof(IsBackOfficeInstalledResponse))]
    ///<summary>Executes IsBackOfficeInstalledRequest SDK message.</summary>
    public class InvokeDataverseIsBackOfficeInstalledCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new IsBackOfficeInstalledRequest();

            var response = (IsBackOfficeInstalledResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
