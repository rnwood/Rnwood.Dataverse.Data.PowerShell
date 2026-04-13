using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="DeleteAttributeRequest"/> — removes an attribute metadata definition from the specified entity in the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Removes the metadata entry; does not remove the attribute's data from existing records in the store or enforce dependency checks.</para>
    /// <para><strong>Configuration:</strong> None — metadata removal is unconditional.</para>
    /// </remarks>
    internal sealed class DeleteAttributeRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "DeleteAttribute", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var deleteRequest = OrganizationRequestTypeAdapter.AsTyped<DeleteAttributeRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            if (string.IsNullOrEmpty(deleteRequest.EntityLogicalName))
                throw DataverseFault.InvalidArgumentFault("Entity logical name is required.");
            if (string.IsNullOrEmpty(deleteRequest.LogicalName))
                throw DataverseFault.InvalidArgumentFault("Attribute logical name is required.");

            store.DeleteAttributeMetadata(deleteRequest.EntityLogicalName, deleteRequest.LogicalName);
            store.IncrementMetadataTimestamp();

            return new DeleteAttributeResponse();
        }
    }
}
