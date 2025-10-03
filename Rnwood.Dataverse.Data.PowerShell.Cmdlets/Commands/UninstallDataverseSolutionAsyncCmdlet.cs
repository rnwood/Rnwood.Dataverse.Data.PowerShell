using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Uninstall, "DataverseSolutionAsync", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(UninstallSolutionAsyncResponse))]
    ///<summary>Executes UninstallSolutionAsyncRequest SDK message.</summary>
    public class UninstallDataverseSolutionAsyncCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SolutionUniqueName parameter")]
        public String SolutionUniqueName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new UninstallSolutionAsyncRequest();
            request.SolutionUniqueName = SolutionUniqueName;
            if (ShouldProcess("Executing UninstallSolutionAsyncRequest", "UninstallSolutionAsyncRequest"))
            {
                var response = (UninstallSolutionAsyncResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
