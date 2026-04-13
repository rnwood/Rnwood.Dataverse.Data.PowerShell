using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="LocalTimeFromUtcTimeRequest"/> by returning the input time unchanged.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Returns the input <see cref="LocalTimeFromUtcTimeRequest.UtcTime"/> as-is without performing timezone conversion. Timezone code resolution is not implemented.</para>
    /// </remarks>
    internal sealed class LocalTimeFromUtcTimeRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "LocalTimeFromUtcTime", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var utcTimeRequest = OrganizationRequestTypeAdapter.AsTyped<LocalTimeFromUtcTimeRequest>(request);

            var response = new LocalTimeFromUtcTimeResponse();
            response.Results["LocalTime"] = utcTimeRequest.UtcTime;
            return response;
        }
    }
}
