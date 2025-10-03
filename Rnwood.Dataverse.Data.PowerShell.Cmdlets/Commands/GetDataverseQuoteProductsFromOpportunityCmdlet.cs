using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseQuoteProductsFromOpportunity")]
    [OutputType(typeof(GetQuoteProductsFromOpportunityResponse))]
    ///<summary>Executes GetQuoteProductsFromOpportunityRequest SDK message.</summary>
    public class GetDataverseQuoteProductsFromOpportunityCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OpportunityId parameter")]
        public Guid OpportunityId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QuoteId parameter")]
        public Guid QuoteId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetQuoteProductsFromOpportunityRequest();
            request.OpportunityId = OpportunityId;            request.QuoteId = QuoteId;
            var response = (GetQuoteProductsFromOpportunityResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
