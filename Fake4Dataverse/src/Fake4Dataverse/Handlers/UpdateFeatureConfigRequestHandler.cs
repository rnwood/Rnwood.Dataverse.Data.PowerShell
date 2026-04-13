using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="UpdateFeatureConfigRequest"/>.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Returns a structurally valid response with no side effects.</para>
    /// </remarks>
    internal sealed class UpdateFeatureConfigRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "UpdateFeatureConfig", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service) =>
            new UpdateFeatureConfigResponse();
    }
}