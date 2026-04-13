using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="DeleteEntityRequest"/> — removes an entity metadata definition from the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Removes the entity schema; records of that entity type remain in the store. Real Dataverse deletes all records when the entity is deleted.</para>
    /// <para><strong>Configuration:</strong> None — metadata removal is unconditional.</para>
    /// </remarks>
    internal sealed class DeleteEntityRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "DeleteEntity", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var deleteRequest = OrganizationRequestTypeAdapter.AsTyped<DeleteEntityRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            if (string.IsNullOrEmpty(deleteRequest.LogicalName))
                throw DataverseFault.InvalidArgumentFault("Entity logical name is required.");

            store.DeleteEntityMetadata(deleteRequest.LogicalName);
            store.IncrementMetadataTimestamp();

            return new DeleteEntityResponse();
        }
    }
}
