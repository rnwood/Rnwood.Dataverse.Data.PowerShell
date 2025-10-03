using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Copy, "DataverseAsPatch", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CloneAsPatchResponse))]
    ///<summary>Executes CloneAsPatchRequest SDK message.</summary>
    public class CopyDataverseAsPatchCmdlet : OrganizationServiceCmdlet
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

            var request = new CloneAsPatchRequest();
            request.ParentSolutionUniqueName = ParentSolutionUniqueName;            request.DisplayName = DisplayName;            request.VersionNumber = VersionNumber;
            if (ShouldProcess("Executing CloneAsPatchRequest", "CloneAsPatchRequest"))
            {
                var response = (CloneAsPatchResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
