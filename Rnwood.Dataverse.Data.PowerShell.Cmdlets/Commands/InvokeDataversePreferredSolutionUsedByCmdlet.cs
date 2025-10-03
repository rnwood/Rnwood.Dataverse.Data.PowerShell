using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataversePreferredSolutionUsedBy")]
    [OutputType(typeof(PreferredSolutionUsedByResponse))]
    ///<summary>Executes PreferredSolutionUsedByRequest SDK message.</summary>
    public class InvokeDataversePreferredSolutionUsedByCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SolutionId parameter")]
        public Guid SolutionId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new PreferredSolutionUsedByRequest();
            request.SolutionId = SolutionId;
            var response = (PreferredSolutionUsedByResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
