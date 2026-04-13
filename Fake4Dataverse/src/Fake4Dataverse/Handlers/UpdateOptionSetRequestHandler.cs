using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="UpdateOptionSetRequest"/> by updating a global option set metadata definition in the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Replaces the stored option set metadata. Does not validate backward-compatibility with existing records that reference the option set.</para>
    /// <para><strong>Configuration:</strong> None — metadata update is unconditional.</para>
    /// </remarks>
    internal sealed class UpdateOptionSetRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "UpdateOptionSet", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var updateRequest = OrganizationRequestTypeAdapter.AsTyped<UpdateOptionSetRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var sdkOptionSet = updateRequest.OptionSet;
            if (sdkOptionSet == null || string.IsNullOrEmpty(sdkOptionSet.Name))
                throw DataverseFault.InvalidArgumentFault("OptionSet with a valid Name is required.");

            var info = new GlobalOptionSetInfo(sdkOptionSet.Name)
            {
                IsGlobal = sdkOptionSet.IsGlobal ?? true
            };

            if (sdkOptionSet is OptionSetMetadata osm)
            {
                foreach (var opt in osm.Options)
                {
                    if (opt.Value.HasValue)
                        info.Options.Add(new OptionInfo(opt.Value.Value, opt.Label?.UserLocalizedLabel?.Label));
                }
            }

            store.UpdateGlobalOptionSet(info);
            store.IncrementMetadataTimestamp();

            return new UpdateOptionSetResponse();
        }
    }
}
