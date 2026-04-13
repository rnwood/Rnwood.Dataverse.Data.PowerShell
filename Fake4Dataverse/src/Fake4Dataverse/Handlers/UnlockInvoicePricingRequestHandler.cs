using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>UnlockInvoicePricingRequest</c> by setting <c>ispricelocked</c> to <c>false</c> on the invoice.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Sets the <c>ispricelocked</c> attribute to <c>false</c> on the target invoice record,
    /// enabling price recalculation. In the fake this persists the flag for assertion.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class UnlockInvoicePricingRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "UnlockInvoicePricing", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var invoiceId = (Guid)request["InvoiceId"];
            var update = new Entity("invoice", invoiceId)
            {
                ["ispricelocked"] = false
            };
            service.Update(update);
            return new OrganizationResponse { ResponseName = "UnlockInvoicePricing" };
        }
    }
}
