using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>UnlockSalesOrderPricingRequest</c> by setting <c>ispricelocked</c> to <c>false</c> on the sales order.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Sets the <c>ispricelocked</c> attribute to <c>false</c> on the target sales order record,
    /// enabling price recalculation. In the fake this persists the flag for assertion.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class UnlockSalesOrderPricingRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "UnlockSalesOrderPricing", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var salesOrderId = (Guid)request["SalesOrderId"];
            var update = new Entity("salesorder", salesOrderId)
            {
                ["ispricelocked"] = false
            };
            service.Update(update);
            return new OrganizationResponse { ResponseName = "UnlockSalesOrderPricing" };
        }
    }
}
