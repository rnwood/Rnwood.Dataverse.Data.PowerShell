using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="UpdateEntityRequest"/> by updating an existing entity metadata definition in the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Replaces the stored <see cref="Microsoft.Xrm.Sdk.Metadata.EntityMetadata"/> with the updated definition. Does not perform schema migration on existing records.</para>
    /// <para><strong>Configuration:</strong> None — metadata update is unconditional.</para>
    /// </remarks>
    internal sealed class UpdateEntityRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "UpdateEntity", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var updateRequest = OrganizationRequestTypeAdapter.AsTyped<UpdateEntityRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var sdkEntity = updateRequest.Entity;
            if (sdkEntity == null || string.IsNullOrEmpty(sdkEntity.LogicalName))
                throw DataverseFault.InvalidArgumentFault("Entity metadata with a valid LogicalName is required.");

            var entityInfo = new EntityMetadataInfo(sdkEntity.LogicalName)
            {
                SchemaName = sdkEntity.SchemaName,
                PrimaryIdAttribute = sdkEntity.PrimaryIdAttribute,
                PrimaryNameAttribute = sdkEntity.PrimaryNameAttribute,
                ObjectTypeCode = sdkEntity.ObjectTypeCode
            };

            store.UpdateEntityMetadata(entityInfo);
            store.IncrementMetadataTimestamp();

            return new UpdateEntityResponse();
        }
    }
}
