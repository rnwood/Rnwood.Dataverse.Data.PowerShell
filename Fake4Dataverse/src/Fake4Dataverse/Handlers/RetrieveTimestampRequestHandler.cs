using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveTimestampRequest"/> by returning the current metadata store version timestamp.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns the internal <c>InMemoryMetadataStore</c> timestamp as the metadata version. This timestamp increments with every metadata change operation.</para>
    /// <para><strong>Configuration:</strong> None — reads directly from the metadata store's internal timestamp counter.</para>
    /// </remarks>
    internal sealed class RetrieveTimestampRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveTimestamp", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var timestamp = store.GetMetadataTimestamp().ToString();

            var response = new RetrieveTimestampResponse();
            response.Results["Timestamp"] = timestamp;
            return response;
        }
    }
}
