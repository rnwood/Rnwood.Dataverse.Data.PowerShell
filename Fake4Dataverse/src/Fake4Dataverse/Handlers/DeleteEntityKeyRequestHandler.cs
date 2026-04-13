using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="DeleteEntityKeyRequest"/> — removes an alternate key metadata definition from the specified entity.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Removes the key metadata; existing records with values matching the key pattern are unaffected.</para>
    /// <para><strong>Configuration:</strong> None — metadata removal is unconditional.</para>
    /// </remarks>
    internal sealed class DeleteEntityKeyRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "DeleteEntityKey", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var deleteRequest = OrganizationRequestTypeAdapter.AsTyped<DeleteEntityKeyRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            if (string.IsNullOrEmpty(deleteRequest.EntityLogicalName))
                throw DataverseFault.InvalidArgumentFault("Entity logical name is required.");
            if (string.IsNullOrEmpty(deleteRequest.Name))
                throw DataverseFault.InvalidArgumentFault("Key name is required.");

            store.DeleteAlternateKey(deleteRequest.EntityLogicalName, deleteRequest.Name);
            store.IncrementMetadataTimestamp();

            return new DeleteEntityKeyResponse();
        }
    }
}
