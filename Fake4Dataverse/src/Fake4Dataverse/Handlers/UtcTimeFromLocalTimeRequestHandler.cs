using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="UtcTimeFromLocalTimeRequest"/> by returning the input time unchanged.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Returns the input <see cref="UtcTimeFromLocalTimeRequest.LocalTime"/> as-is without performing timezone conversion. Timezone code resolution is not implemented.</para>
    /// </remarks>
    internal sealed class UtcTimeFromLocalTimeRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "UtcTimeFromLocalTime", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var localTimeRequest = OrganizationRequestTypeAdapter.AsTyped<UtcTimeFromLocalTimeRequest>(request);

            var response = new UtcTimeFromLocalTimeResponse();
            response.Results["UtcTime"] = localTimeRequest.LocalTime;
            return response;
        }
    }
}
