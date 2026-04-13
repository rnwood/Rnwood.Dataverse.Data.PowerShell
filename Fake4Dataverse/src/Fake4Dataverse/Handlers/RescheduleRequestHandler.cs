using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RescheduleRequest"/> by updating the scheduling attributes on the target appointment or service activity.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Updates the target record with the attributes provided in <c>Target</c> (typically <c>scheduledstart</c>, <c>scheduledend</c>, and <c>location</c>). Does not perform availability validation or resource scheduling.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RescheduleRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "Reschedule", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var rescheduleRequest = OrganizationRequestTypeAdapter.AsTyped<RescheduleRequest>(request);
            var target = rescheduleRequest.Target;

            if (target == null)
                throw DataverseFault.InvalidArgumentFault("Target is required for Reschedule.");

            service.Update(target);

            var response = new RescheduleResponse();
            response.Results["ValidationResult"] = new ValidationResult { ValidationSuccess = true };
            return response;
        }
    }
}
