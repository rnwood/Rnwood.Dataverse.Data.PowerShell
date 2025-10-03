using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "DataverseQuoteFromOpportunity", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(GenerateQuoteFromOpportunityResponse))]
    ///<summary>Executes GenerateQuoteFromOpportunityRequest SDK message.</summary>
    public class NewDataverseQuoteFromOpportunityCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OpportunityId parameter")]
        public Guid OpportunityId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ColumnSet parameter")]
        public Microsoft.Xrm.Sdk.Query.ColumnSet ColumnSet { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "ProcessInstanceId parameter")]
        public object ProcessInstanceId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GenerateQuoteFromOpportunityRequest();
            request.OpportunityId = OpportunityId;            request.ColumnSet = ColumnSet;            if (ProcessInstanceId != null)
            {
                request.ProcessInstanceId = DataverseTypeConverter.ToEntityReference(ProcessInstanceId, null, "ProcessInstanceId");
            }

            if (ShouldProcess("Executing GenerateQuoteFromOpportunityRequest", "GenerateQuoteFromOpportunityRequest"))
            {
                var response = (GenerateQuoteFromOpportunityResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
