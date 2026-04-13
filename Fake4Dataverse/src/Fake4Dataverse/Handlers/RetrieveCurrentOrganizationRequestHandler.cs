using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>RetrieveCurrentOrganizationRequest</c> by returning the fake organization details.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Returns organization details sourced from <see cref="FakeDataverseEnvironment"/> properties (<c>OrganizationId</c>, <c>OrganizationName</c>, <c>Version</c>, etc.). Configure these via the environment constructor or properties.</para>
    /// <para><strong>Configuration:</strong> None — reads directly from <see cref="FakeDataverseEnvironment"/> properties.</para>
    /// </remarks>
    internal sealed class RetrieveCurrentOrganizationRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveCurrentOrganization", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var fakeService = service as FakeOrganizationService;
            var detail = new Entity("organization");
            detail["organizationid"] = fakeService?.Environment.OrganizationId ?? Guid.Empty;
            detail["name"] = fakeService?.Environment.OrganizationName ?? "FakeOrganization";
            detail["uniquename"] = fakeService?.Environment.OrganizationName ?? "FakeOrganization";

            var response = new OrganizationResponse { ResponseName = "RetrieveCurrentOrganization" };
            response["Detail"] = detail;
            return response;
        }
    }
}
