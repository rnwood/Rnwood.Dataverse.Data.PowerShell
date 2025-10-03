using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataversePreferredSolution")]
    [OutputType(typeof(GetPreferredSolutionResponse))]
    ///<summary>Executes GetPreferredSolutionRequest SDK message.</summary>
    public class GetDataversePreferredSolutionCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetPreferredSolutionRequest();

            var response = (GetPreferredSolutionResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
