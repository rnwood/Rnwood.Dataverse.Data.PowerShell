using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RecalculateRequest"/> by recalculating the estimated and actual values on an opportunity.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Acknowledges the recalculation request. In the fake, opportunity values are not automatically computed from line items, but the request completes successfully allowing tests to verify the request pipeline.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RecalculateRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "Recalculate", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var recalcRequest = OrganizationRequestTypeAdapter.AsTyped<RecalculateRequest>(request);
            var target = recalcRequest.Target;

            if (target == null)
                throw DataverseFault.InvalidArgumentFault("Target is required for Recalculate.");

            // Verify the target record exists
            if (service is FakeOrganizationService fakeService)
            {
                if (!fakeService.Environment.Store.Exists(target.LogicalName, target.Id))
                    throw DataverseFault.EntityNotFound(target.LogicalName, target.Id);
            }

            return new OrganizationResponse { ResponseName = "Recalculate" };
        }
    }
}
