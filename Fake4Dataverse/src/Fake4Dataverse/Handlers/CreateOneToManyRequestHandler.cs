using System;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CreateOneToManyRequest"/> — registers a 1:N relationship metadata definition and its lookup attribute in the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Stores the <see cref="OneToManyRelationshipMetadata"/> and associated <see cref="LookupAttributeMetadata"/>; cascade behaviors are recorded and applied during Delete/Assign. Does not enforce physical foreign-key constraints.</para>
    /// <para><strong>Configuration:</strong> Registered cascade behaviors affect <c>Delete</c> and <c>Assign</c> operations when child relationships are registered.</para>
    /// </remarks>
    internal sealed class CreateOneToManyRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CreateOneToMany", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var createRequest = OrganizationRequestTypeAdapter.AsTyped<CreateOneToManyRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var rel = createRequest.OneToManyRelationship;
            if (rel == null || string.IsNullOrEmpty(rel.SchemaName))
                throw DataverseFault.InvalidArgumentFault("OneToManyRelationship with a valid SchemaName is required.");

            var info = new OneToManyRelationshipInfo(
                rel.SchemaName,
                rel.ReferencedEntity ?? string.Empty,
                rel.ReferencedAttribute ?? string.Empty,
                rel.ReferencingEntity ?? string.Empty,
                rel.ReferencingAttribute ?? string.Empty);

            store.CreateOneToManyRelationshipInternal(info);
            store.IncrementMetadataTimestamp();

            var response = new CreateOneToManyResponse();
            response.Results["RelationshipId"] = Guid.NewGuid();
            response.Results["AttributeId"] = Guid.NewGuid();
            return response;
        }
    }
}
