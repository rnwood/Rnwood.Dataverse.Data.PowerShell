using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveRelationshipRequest"/> by retrieving a 1:N or N:N relationship metadata definition from the in-memory metadata store by schema name.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns the relationship metadata registered via <c>env.RegisterRelationship(...)</c> or <c>CreateOneToManyRequest</c>/<c>CreateManyToManyRequest</c>. Throws <c>FaultException</c> if not registered.</para>
    /// <para><strong>Configuration:</strong> None — returns whatever is in the in-memory metadata store.</para>
    /// </remarks>
    internal sealed class RetrieveRelationshipRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveRelationship", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var retrieveRequest = OrganizationRequestTypeAdapter.AsTyped<RetrieveRelationshipRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            if (string.IsNullOrEmpty(retrieveRequest.Name))
                throw DataverseFault.InvalidArgumentFault("Relationship name is required.");

            RelationshipMetadataBase? result = null;

            var otm = store.GetOneToManyRelationship(retrieveRequest.Name);
            if (otm != null)
            {
                var rel = new OneToManyRelationshipMetadata();
                rel.SchemaName = otm.SchemaName;
                rel.ReferencedEntity = otm.ReferencedEntity;
                rel.ReferencedAttribute = otm.ReferencedAttribute;
                rel.ReferencingEntity = otm.ReferencingEntity;
                rel.ReferencingAttribute = otm.ReferencingAttribute;
                result = rel;
            }

            if (result == null)
            {
                var mtm = store.GetManyToManyRelationship(retrieveRequest.Name);
                if (mtm != null)
                {
                    var rel = new ManyToManyRelationshipMetadata();
                    rel.SchemaName = mtm.SchemaName;
                    rel.Entity1LogicalName = mtm.Entity1LogicalName;
                    rel.Entity2LogicalName = mtm.Entity2LogicalName;
                    if (mtm.IntersectEntityName != null)
                        rel.IntersectEntityName = mtm.IntersectEntityName;
                    result = rel;
                }
            }

            if (result == null)
            {
                throw DataverseFault.Create(
                    DataverseFault.ObjectDoesNotExist,
                    $"Relationship '{retrieveRequest.Name}' does not exist.");
            }

            var response = new RetrieveRelationshipResponse();
            response.Results["RelationshipMetadata"] = result;
            return response;
        }
    }
}
