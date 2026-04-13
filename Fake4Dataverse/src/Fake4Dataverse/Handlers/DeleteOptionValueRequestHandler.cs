using System.Linq;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="DeleteOptionValueRequest"/> — removes a specific option value from a global option set in the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Removes the option from the metadata; existing records with that option value set are unaffected.</para>
    /// <para><strong>Configuration:</strong> None — metadata update is unconditional.</para>
    /// </remarks>
    internal sealed class DeleteOptionValueRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "DeleteOptionValue", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var deleteRequest = OrganizationRequestTypeAdapter.AsTyped<DeleteOptionValueRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var value = deleteRequest.Value;

            if (!string.IsNullOrEmpty(deleteRequest.OptionSetName))
            {
                var optionSet = store.GetGlobalOptionSet(deleteRequest.OptionSetName);
                if (optionSet != null)
                {
                    optionSet.Options.RemoveAll(o => o.Value == value);
                }
            }

            store.IncrementMetadataTimestamp();

            return new DeleteOptionValueResponse();
        }
    }
}
