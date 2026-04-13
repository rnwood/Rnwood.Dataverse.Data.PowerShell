using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="IsDataEncryptionActiveRequest"/> — returns whether organization-level field-level encryption is active.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Always returns <see langword="false"/>. Real Dataverse reflects the actual encryption configuration.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class IsDataEncryptionActiveRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "IsDataEncryptionActive", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var response = new IsDataEncryptionActiveResponse();
            response.Results["IsActive"] = false;
            return response;
        }
    }
}
