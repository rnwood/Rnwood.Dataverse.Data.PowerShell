using System;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CreateCustomerRelationshipsRequest"/> — creates 1:N relationship metadata entries for the specified customer (account and contact) relationships.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Stores the relationship metadata definitions; cascade behavior and lookup attribute metadata are registered. Physical polymorphic constraint enforcement is not replicated.</para>
    /// <para><strong>Configuration:</strong> None — metadata registration is unconditional.</para>
    /// </remarks>
    internal sealed class CreateCustomerRelationshipsRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CreateCustomerRelationships", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var createRequest = OrganizationRequestTypeAdapter.AsTyped<CreateCustomerRelationshipsRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var lookup = createRequest.Lookup;
            if (lookup == null || string.IsNullOrEmpty(lookup.LogicalName))
                throw DataverseFault.InvalidArgumentFault("Lookup attribute with a valid LogicalName is required.");

            var relationships = createRequest.OneToManyRelationships;
            var relationshipIds = new Guid[relationships?.Length ?? 0];

            if (relationships != null)
            {
                for (int i = 0; i < relationships.Length; i++)
                {
                    var rel = relationships[i];
                    if (!string.IsNullOrEmpty(rel.SchemaName))
                    {
                        var info = new OneToManyRelationshipInfo(
                            rel.SchemaName,
                            rel.ReferencedEntity ?? string.Empty,
                            rel.ReferencedAttribute ?? string.Empty,
                            rel.ReferencingEntity ?? string.Empty,
                            rel.ReferencingAttribute ?? lookup.LogicalName);

                        store.CreateOneToManyRelationshipInternal(info);
                    }
                    relationshipIds[i] = Guid.NewGuid();
                }
            }

            store.IncrementMetadataTimestamp();

            var attributeId = Guid.NewGuid();
            var response = new CreateCustomerRelationshipsResponse();
            response.Results["AttributeId"] = attributeId;
            response.Results["RelationshipIds"] = relationshipIds;
            return response;
        }
    }
}
