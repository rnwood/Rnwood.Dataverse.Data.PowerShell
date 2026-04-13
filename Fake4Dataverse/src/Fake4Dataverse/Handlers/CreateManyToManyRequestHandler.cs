using System;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CreateManyToManyRequest"/> — registers an N:N relationship metadata definition between two entities.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Stores the relationship metadata; does not create an intersect entity table. The in-memory <c>Associate</c>/<c>Disassociate</c> operations use an <c>association_{schemaname}</c> internal entity.</para>
    /// <para><strong>Configuration:</strong> None — metadata registration is unconditional.</para>
    /// </remarks>
    internal sealed class CreateManyToManyRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CreateManyToMany", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var createRequest = OrganizationRequestTypeAdapter.AsTyped<CreateManyToManyRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var rel = createRequest.ManyToManyRelationship;
            if (rel == null || string.IsNullOrEmpty(rel.SchemaName))
                throw DataverseFault.InvalidArgumentFault("ManyToManyRelationship with a valid SchemaName is required.");

            var info = new ManyToManyRelationshipInfo(
                rel.SchemaName,
                rel.Entity1LogicalName ?? string.Empty,
                rel.Entity2LogicalName ?? string.Empty,
                rel.IntersectEntityName);

            store.CreateManyToManyRelationshipInternal(info);
            store.IncrementMetadataTimestamp();

            var response = new CreateManyToManyResponse();
            response.Results["ManyToManyRelationshipId"] = Guid.NewGuid();
            return response;
        }
    }
}
