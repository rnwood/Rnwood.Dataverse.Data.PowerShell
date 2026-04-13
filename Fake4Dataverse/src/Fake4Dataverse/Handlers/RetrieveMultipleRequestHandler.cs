using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveMultipleRequest"/> by delegating to <see cref="IOrganizationService.RetrieveMultiple"/>, which evaluates the query against the in-memory store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Delegates entirely to the service layer; <see cref="Microsoft.Xrm.Sdk.Query.QueryExpression"/>, FetchXml, and <see cref="Microsoft.Xrm.Sdk.Query.QueryByAttribute"/> are all supported.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.EnforceSecurityRoles"/> — row-level security filtering applies when enabled.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class RetrieveMultipleRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveMultiple", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var rmRequest = OrganizationRequestTypeAdapter.AsTyped<RetrieveMultipleRequest>(request);
            var result = service.RetrieveMultiple(rmRequest.Query);
            return new RetrieveMultipleResponse { Results = { ["EntityCollection"] = result } };
        }
    }
}
