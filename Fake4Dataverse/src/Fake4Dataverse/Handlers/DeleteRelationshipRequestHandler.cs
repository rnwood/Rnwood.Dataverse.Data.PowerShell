using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="DeleteRelationshipRequest"/> — removes a 1:N or N:N relationship metadata definition from the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Removes the relationship schema; association records in the store and cascade behaviors registered on child entities are not automatically cleaned up.</para>
    /// <para><strong>Configuration:</strong> None — metadata removal is unconditional.</para>
    /// </remarks>
    internal sealed class DeleteRelationshipRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "DeleteRelationship", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var deleteRequest = OrganizationRequestTypeAdapter.AsTyped<DeleteRelationshipRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            if (string.IsNullOrEmpty(deleteRequest.Name))
                throw DataverseFault.InvalidArgumentFault("Relationship name is required.");

            store.DeleteRelationshipInternal(deleteRequest.Name);
            store.IncrementMetadataTimestamp();

            return new DeleteRelationshipResponse();
        }
    }
}
