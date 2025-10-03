using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Copy, "DataverseAsSolution", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CloneAsSolutionResponse))]
    ///<summary>Executes CloneAsSolutionRequest SDK message.</summary>
    public class CopyDataverseAsSolutionCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ParentSolutionUniqueName parameter")]
        public String ParentSolutionUniqueName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DisplayName parameter")]
        public String DisplayName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "VersionNumber parameter")]
        public String VersionNumber { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CloneAsSolutionRequest();
            request.ParentSolutionUniqueName = ParentSolutionUniqueName;            request.DisplayName = DisplayName;            request.VersionNumber = VersionNumber;
            if (ShouldProcess("Executing CloneAsSolutionRequest", "CloneAsSolutionRequest"))
            {
                var response = (CloneAsSolutionResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
