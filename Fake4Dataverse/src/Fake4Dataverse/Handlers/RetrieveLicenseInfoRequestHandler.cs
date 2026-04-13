using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveLicenseInfoRequest"/> by returning default license information.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Returns default license counts (100 available, 0 used, 100 granted). Suitable for testing code that reads license capacity.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RetrieveLicenseInfoRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveLicenseInfo", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var response = new RetrieveLicenseInfoResponse();
            response.Results["AvailableCount"] = 100;
            response.Results["GrantedLicenseCount"] = 100;
            return response;
        }
    }
}
