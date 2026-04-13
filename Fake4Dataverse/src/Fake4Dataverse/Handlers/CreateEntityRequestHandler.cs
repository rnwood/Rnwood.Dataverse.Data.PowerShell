using System;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CreateEntityRequest"/> — registers a new entity metadata definition and its primary name attribute in the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Stores <see cref="EntityMetadata"/> and creates the primary attribute metadata; does not replicate physical table creation or enforce platform naming restrictions.</para>
    /// <para><strong>Configuration:</strong> None — metadata registration is unconditional.</para>
    /// </remarks>
    internal sealed class CreateEntityRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CreateEntity", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var createRequest = OrganizationRequestTypeAdapter.AsTyped<CreateEntityRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var sdkEntity = createRequest.Entity;
            if (sdkEntity == null || string.IsNullOrEmpty(sdkEntity.LogicalName))
                throw DataverseFault.InvalidArgumentFault("Entity metadata with a valid LogicalName is required.");

            var entityInfo = new EntityMetadataInfo(sdkEntity.LogicalName)
            {
                SchemaName = sdkEntity.SchemaName,
                PrimaryIdAttribute = sdkEntity.PrimaryIdAttribute,
                PrimaryNameAttribute = sdkEntity.PrimaryNameAttribute,
                ObjectTypeCode = sdkEntity.ObjectTypeCode
            };

            store.CreateEntityMetadata(entityInfo);
            store.IncrementMetadataTimestamp();

            var entityId = Guid.NewGuid();
            var response = new CreateEntityResponse();
            response.Results["EntityId"] = entityId;
            response.Results["AttributeId"] = Guid.NewGuid();
            return response;
        }
    }
}
