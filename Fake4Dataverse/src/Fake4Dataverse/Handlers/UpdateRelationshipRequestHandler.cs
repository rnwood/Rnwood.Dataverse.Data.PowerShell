using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="UpdateRelationshipRequest"/> by updating a 1:N or N:N relationship metadata definition in the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Replaces the stored relationship metadata. Cascade behavior changes take effect immediately for subsequent operations.</para>
    /// <para><strong>Configuration:</strong> None — metadata update is unconditional.</para>
    /// </remarks>
    internal sealed class UpdateRelationshipRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "UpdateRelationship", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var updateRequest = OrganizationRequestTypeAdapter.AsTyped<UpdateRelationshipRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var rel = updateRequest.Relationship;
            if (rel == null || string.IsNullOrEmpty(rel.SchemaName))
                throw DataverseFault.InvalidArgumentFault("Relationship metadata with a valid SchemaName is required.");

            if (rel is OneToManyRelationshipMetadata otm)
            {
                var info = new OneToManyRelationshipInfo(
                    otm.SchemaName,
                    otm.ReferencedEntity ?? string.Empty,
                    otm.ReferencedAttribute ?? string.Empty,
                    otm.ReferencingEntity ?? string.Empty,
                    otm.ReferencingAttribute ?? string.Empty);
                store.UpdateOneToManyRelationship(info);
            }
            else if (rel is ManyToManyRelationshipMetadata mtm)
            {
                var info = new ManyToManyRelationshipInfo(
                    mtm.SchemaName,
                    mtm.Entity1LogicalName ?? string.Empty,
                    mtm.Entity2LogicalName ?? string.Empty,
                    mtm.IntersectEntityName);
                store.UpdateManyToManyRelationship(info);
            }
            else
            {
                throw DataverseFault.InvalidArgumentFault("Unsupported relationship type.");
            }

            store.IncrementMetadataTimestamp();
            return new UpdateRelationshipResponse();
        }
    }
}
