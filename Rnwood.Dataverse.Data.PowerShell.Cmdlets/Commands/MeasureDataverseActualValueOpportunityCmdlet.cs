using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsDiagnostic.Measure, "DataverseActualValueOpportunity")]
    [OutputType(typeof(CalculateActualValueOpportunityResponse))]
    ///<summary>Executes CalculateActualValueOpportunityRequest SDK message.</summary>
    public class MeasureDataverseActualValueOpportunityCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OpportunityId parameter")]
        public Guid OpportunityId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CalculateActualValueOpportunityRequest();
            request.OpportunityId = OpportunityId;
            var response = (CalculateActualValueOpportunityResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
