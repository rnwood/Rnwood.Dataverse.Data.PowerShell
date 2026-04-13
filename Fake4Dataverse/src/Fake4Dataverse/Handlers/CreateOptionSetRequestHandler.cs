using System;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CreateOptionSetRequest"/> — registers a global (shared) option set metadata definition in the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Stores the <see cref="OptionSetMetadataBase"/> for retrieval; does not enforce option value uniqueness across the organization.</para>
    /// <para><strong>Configuration:</strong> None — metadata registration is unconditional.</para>
    /// </remarks>
    internal sealed class CreateOptionSetRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CreateOptionSet", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var createRequest = OrganizationRequestTypeAdapter.AsTyped<CreateOptionSetRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var sdkOptionSet = createRequest.OptionSet;
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

            store.CreateGlobalOptionSet(info);
            store.IncrementMetadataTimestamp();

            var optionSetId = Guid.NewGuid();
            var response = new CreateOptionSetResponse();
            response.Results["OptionSetId"] = optionSetId;
            return response;
        }
    }
}
