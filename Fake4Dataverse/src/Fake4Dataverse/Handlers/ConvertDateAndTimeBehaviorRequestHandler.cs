using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="ConvertDateAndTimeBehaviorRequest"/> — converts date/time attribute behavior (e.g., Local to UTC).
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>No-op; returns a fake async job <see cref="EntityReference"/>. Real Dataverse converts all historical data for the attribute. No metadata or data conversion is performed.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class ConvertDateAndTimeBehaviorRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "ConvertDateAndTimeBehavior", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            // In the fake, this is a no-op that acknowledges the conversion request
            var response = new ConvertDateAndTimeBehaviorResponse();
            response.Results["JobId"] = System.Guid.NewGuid();
            return response;
        }
    }
}
