using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="UpdateAttributeRequest"/> by updating an existing attribute metadata definition on the specified entity in the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Replaces the stored <c>AttributeMetadata</c> with the updated definition. Does not validate that the change is backward-compatible with existing records.</para>
    /// <para><strong>Configuration:</strong> None — metadata update is unconditional.</para>
    /// </remarks>
    internal sealed class UpdateAttributeRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "UpdateAttribute", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var updateRequest = OrganizationRequestTypeAdapter.AsTyped<UpdateAttributeRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var entityName = updateRequest.Parameters.ContainsKey("EntityName") ? (string)updateRequest.Parameters["EntityName"] : null;
            if (string.IsNullOrEmpty(entityName))
                throw DataverseFault.InvalidArgumentFault("Entity logical name is required.");

            var sdkAttr = updateRequest.Parameters.ContainsKey("Attribute") ? (AttributeMetadata)updateRequest.Parameters["Attribute"] : null;
            if (sdkAttr == null || string.IsNullOrEmpty(sdkAttr.LogicalName))
                throw DataverseFault.InvalidArgumentFault("Attribute metadata with a valid LogicalName is required.");

            var attrType = sdkAttr.AttributeType ?? AttributeTypeCode.String;
            var attrInfo = new AttributeMetadataInfo(sdkAttr.LogicalName, attrType);

            if (sdkAttr.RequiredLevel?.Value != null)
                attrInfo.RequiredLevel = sdkAttr.RequiredLevel.Value;

            store.UpdateAttributeMetadata(entityName!, attrInfo);
            store.IncrementMetadataTimestamp();

            return new UpdateAttributeResponse();
        }
    }
}
