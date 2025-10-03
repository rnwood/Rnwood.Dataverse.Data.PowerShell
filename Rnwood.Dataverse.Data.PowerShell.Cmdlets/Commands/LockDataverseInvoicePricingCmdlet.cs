using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Lock, "DataverseInvoicePricing")]
    [OutputType(typeof(LockInvoicePricingResponse))]
    ///<summary>Locks pricing for an invoice to prevent automatic price recalculations.</summary>
    public class LockDataverseInvoicePricingCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the invoice to lock pricing for")]
        [Alias("Id")]
        public Guid InvoiceId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            LockInvoicePricingRequest request = new LockInvoicePricingRequest
            {
                InvoiceId = InvoiceId
            };

            LockInvoicePricingResponse response = (LockInvoicePricingResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
