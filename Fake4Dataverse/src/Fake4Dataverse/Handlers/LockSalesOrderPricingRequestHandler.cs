using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>LockSalesOrderPricingRequest</c> by setting <c>ispricelocked</c> to <c>true</c> on the sales order.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Sets the <c>ispricelocked</c> attribute to <c>true</c> on the target sales order record.
    /// In real Dataverse this prevents price recalculation; in the fake it persists the flag for assertion.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class LockSalesOrderPricingRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "LockSalesOrderPricing", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var salesOrderId = (Guid)request["SalesOrderId"];
            var update = new Entity("salesorder", salesOrderId)
            {
                ["ispricelocked"] = true
            };
            service.Update(update);
            return new OrganizationResponse { ResponseName = "LockSalesOrderPricing" };
        }
    }
}
