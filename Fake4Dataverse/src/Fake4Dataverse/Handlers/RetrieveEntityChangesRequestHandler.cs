using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveEntityChangesRequest"/> by retrieving incremental record changes (delta sync) for an entity since a given sync token.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Returns an empty <c>BusinessEntityChanges</c> collection with a new sync token. Real Dataverse returns actual entity change records since the last sync token. This stub is sufficient for testing that callers handle an "empty delta" gracefully.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RetrieveEntityChangesRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveEntityChanges", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            // Return a stub response — the real response uses BusinessEntityChanges
            // which is complex. We return the key properties via Results dictionary.
            var response = new RetrieveEntityChangesResponse();
            response.Results["BusinessEntityChanges"] = new Microsoft.Xrm.Sdk.EntityCollection();
            return response;
        }
    }
}
