using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Uninstall, "DataverseSampleData", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(UninstallSampleDataResponse))]
    ///<summary>Executes UninstallSampleDataRequest SDK message.</summary>
    public class UninstallDataverseSampleDataCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new UninstallSampleDataRequest();

            if (ShouldProcess("Executing UninstallSampleDataRequest", "UninstallSampleDataRequest"))
            {
                var response = (UninstallSampleDataResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
