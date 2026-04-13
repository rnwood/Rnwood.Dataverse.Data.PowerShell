using System.Linq;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="UpdateOptionValueRequest"/> by updating the label and/or description of an existing option value within a global option set.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Updates the option entry in the in-memory metadata store. Does not update option labels on existing records or attribute metadata that references the option set.</para>
    /// <para><strong>Configuration:</strong> None — metadata update is unconditional.</para>
    /// </remarks>
    internal sealed class UpdateOptionValueRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "UpdateOptionValue", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var updateRequest = OrganizationRequestTypeAdapter.AsTyped<UpdateOptionValueRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var value = updateRequest.Value;
            var newLabel = updateRequest.Label?.UserLocalizedLabel?.Label
                ?? (updateRequest.Label?.LocalizedLabels?.Count > 0
                    ? updateRequest.Label.LocalizedLabels[0].Label
                    : null);

            if (!string.IsNullOrEmpty(updateRequest.OptionSetName))
            {
                var optionSet = store.GetGlobalOptionSet(updateRequest.OptionSetName);
                if (optionSet != null)
                {
                    var opt = optionSet.Options.FirstOrDefault(o => o.Value == value);
                    if (opt != null && newLabel != null)
                    {
                        opt.Label = newLabel;
                    }
                }
            }

            store.IncrementMetadataTimestamp();

            return new UpdateOptionValueResponse();
        }
    }
}
