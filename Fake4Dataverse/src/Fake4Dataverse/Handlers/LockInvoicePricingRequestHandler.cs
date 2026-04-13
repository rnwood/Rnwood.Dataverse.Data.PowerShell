using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>LockInvoicePricingRequest</c> by setting <c>ispricelocked</c> to <c>true</c> on the invoice.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Sets the <c>ispricelocked</c> attribute to <c>true</c> on the target invoice record.
    /// In real Dataverse this prevents price recalculation on the invoice; in the fake it persists the flag for assertion.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class LockInvoicePricingRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "LockInvoicePricing", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var invoiceId = (Guid)request["InvoiceId"];
            var update = new Entity("invoice", invoiceId)
            {
                ["ispricelocked"] = true
            };
            service.Update(update);
            return new OrganizationResponse { ResponseName = "LockInvoicePricing" };
        }
    }
}
