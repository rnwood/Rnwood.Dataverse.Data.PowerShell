using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="DeleteOptionSetRequest"/> — removes a global option set metadata definition from the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Removes the option set metadata; attribute metadata referencing the option set is not automatically updated.</para>
    /// <para><strong>Configuration:</strong> None — metadata removal is unconditional.</para>
    /// </remarks>
    internal sealed class DeleteOptionSetRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "DeleteOptionSet", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var deleteRequest = OrganizationRequestTypeAdapter.AsTyped<DeleteOptionSetRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            if (string.IsNullOrEmpty(deleteRequest.Name))
                throw DataverseFault.InvalidArgumentFault("Option set name is required.");

            store.DeleteGlobalOptionSet(deleteRequest.Name);
            store.IncrementMetadataTimestamp();

            return new DeleteOptionSetResponse();
        }
    }
}
