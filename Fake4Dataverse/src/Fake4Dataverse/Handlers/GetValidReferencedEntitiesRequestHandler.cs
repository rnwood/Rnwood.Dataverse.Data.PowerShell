using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="GetValidReferencedEntitiesRequest"/> — returns entity names that can be the primary (referenced) side of a 1:N relationship with the specified entity.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns all registered entity names regardless of relationship constraints. Real Dataverse restricts based on entity capabilities.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.ValidateWithMetadata"/> — when <see langword="false"/> (default), returns an empty collection.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class GetValidReferencedEntitiesRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "GetValidReferencedEntities", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            // Return all registered entities as valid referenced entities
            var allEntities = store.GetAllEntityMetadataInfo();
            var entityNames = allEntities.Select(e => e.LogicalName).ToArray();

            var response = new GetValidReferencedEntitiesResponse();
            response.Results["EntityNames"] = entityNames;
            return response;
        }
    }
}
