using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseDependenciesForUninstall")]
    [OutputType(typeof(RetrieveDependenciesForUninstallResponse))]
    ///<summary>Executes RetrieveDependenciesForUninstallRequest SDK message.</summary>
    public class GetDataverseDependenciesForUninstallCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SolutionUniqueName parameter")]
        public String SolutionUniqueName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveDependenciesForUninstallRequest();
            request.SolutionUniqueName = SolutionUniqueName;
            var response = (RetrieveDependenciesForUninstallResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
