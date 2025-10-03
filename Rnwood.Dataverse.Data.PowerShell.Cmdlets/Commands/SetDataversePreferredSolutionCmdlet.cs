using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataversePreferredSolution", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SetPreferredSolutionResponse))]
    ///<summary>Executes SetPreferredSolutionRequest SDK message.</summary>
    public class SetDataversePreferredSolutionCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SolutionId parameter")]
        public Guid SolutionId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SetPreferredSolutionRequest();
            request.SolutionId = SolutionId;
            if (ShouldProcess("Executing SetPreferredSolutionRequest", "SetPreferredSolutionRequest"))
            {
                var response = (SetPreferredSolutionResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
