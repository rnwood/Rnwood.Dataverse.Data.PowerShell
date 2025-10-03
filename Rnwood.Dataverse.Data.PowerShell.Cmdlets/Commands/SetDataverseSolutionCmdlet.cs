using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseSolution", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(StageSolutionResponse))]
    ///<summary>Executes StageSolutionRequest SDK message.</summary>
    public class SetDataverseSolutionCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CustomizationFile parameter")]
        public Byte[] CustomizationFile { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new StageSolutionRequest();
            request.CustomizationFile = CustomizationFile;
            if (ShouldProcess("Executing StageSolutionRequest", "StageSolutionRequest"))
            {
                var response = (StageSolutionResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
