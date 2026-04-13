using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveDeploymentLicenseTypeRequest"/> by returning a default license type.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Returns <c>"OnPremise"</c> as the license type. This value is suitable for testing code that branches on deployment type.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RetrieveDeploymentLicenseTypeRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveDeploymentLicenseType", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var response = new RetrieveDeploymentLicenseTypeResponse();
            response.Results["licenseType"] = "OnPremise";
            return response;
        }
    }
}
