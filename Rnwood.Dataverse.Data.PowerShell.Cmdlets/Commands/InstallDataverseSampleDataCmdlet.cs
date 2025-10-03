using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Install, "DataverseSampleData", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(InstallSampleDataResponse))]
    ///<summary>Executes InstallSampleDataRequest SDK message.</summary>
    public class InstallDataverseSampleDataCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new InstallSampleDataRequest();

            if (ShouldProcess("Executing InstallSampleDataRequest", "InstallSampleDataRequest"))
            {
                var response = (InstallSampleDataResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
