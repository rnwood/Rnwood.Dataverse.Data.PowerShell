using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="GetValidReferencingEntitiesRequest"/> — returns entity names that can be the child (referencing) side of a 1:N relationship with the specified entity.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns all registered entity names. Real Dataverse restricts based on entity capabilities.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.ValidateWithMetadata"/> — when <see langword="false"/> (default), returns an empty collection.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class GetValidReferencingEntitiesRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "GetValidReferencingEntities", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            // Return all registered entities as valid referencing entities
            var allEntities = store.GetAllEntityMetadataInfo();
            var entityNames = allEntities.Select(e => e.LogicalName).ToArray();

            var response = new GetValidReferencingEntitiesResponse();
            response.Results["EntityNames"] = entityNames;
            return response;
        }
    }
}
