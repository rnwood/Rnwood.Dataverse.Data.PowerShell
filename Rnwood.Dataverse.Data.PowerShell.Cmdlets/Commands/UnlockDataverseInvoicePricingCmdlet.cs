using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Unlock, "DataverseInvoicePricing")]
    [OutputType(typeof(UnlockInvoicePricingResponse))]
    ///<summary>Unlocks pricing for an invoice to allow automatic price recalculations.</summary>
    public class UnlockDataverseInvoicePricingCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the invoice to unlock pricing for")]
        [Alias("Id")]
        public Guid InvoiceId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            UnlockInvoicePricingRequest request = new UnlockInvoicePricingRequest
            {
                InvoiceId = InvoiceId
            };

            UnlockInvoicePricingResponse response = (UnlockInvoicePricingResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
