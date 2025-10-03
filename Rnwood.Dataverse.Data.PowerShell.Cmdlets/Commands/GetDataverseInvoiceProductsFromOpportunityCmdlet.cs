using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseInvoiceProductsFromOpportunity")]
    [OutputType(typeof(GetInvoiceProductsFromOpportunityResponse))]
    ///<summary>Executes GetInvoiceProductsFromOpportunityRequest SDK message.</summary>
    public class GetDataverseInvoiceProductsFromOpportunityCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OpportunityId parameter")]
        public Guid OpportunityId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "InvoiceId parameter")]
        public Guid InvoiceId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetInvoiceProductsFromOpportunityRequest();
            request.OpportunityId = OpportunityId;            request.InvoiceId = InvoiceId;
            var response = (GetInvoiceProductsFromOpportunityResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
