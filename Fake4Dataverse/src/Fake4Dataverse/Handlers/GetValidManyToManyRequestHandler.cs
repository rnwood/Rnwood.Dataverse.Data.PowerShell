using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="GetValidManyToManyRequest"/> — returns the set of entity names that can participate in N:N relationships.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns all entity names currently registered in the metadata store. Real Dataverse excludes certain system entities from N:N relationships.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.ValidateWithMetadata"/> — when <see langword="false"/> (default), returns an empty collection (no entities registered).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class GetValidManyToManyRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "GetValidManyToMany", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            // Return all registered entities as valid for N:N
            var allEntities = store.GetAllEntityMetadataInfo();
            var entityNames = allEntities
                .Select(e =>
                {
                    var em = new EntityMetadata();
                    em.LogicalName = e.LogicalName;
                    return em;
                })
                .ToArray();

            var response = new GetValidManyToManyResponse();
            response.Results["EntityMetadata"] = entityNames;
            return response;
        }
    }
}
