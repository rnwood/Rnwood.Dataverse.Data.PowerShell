using System.Linq;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveAllOptionSetsRequest"/> by returning all global (shared) option set metadata registered in the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns only global option sets registered via <c>env.RegisterGlobalOptionSet(...)</c> or via <c>CreateOptionSetRequest</c>. Real Dataverse also returns system option sets.</para>
    /// <para><strong>Configuration:</strong> None — returns whatever is in the in-memory metadata store.</para>
    /// </remarks>
    internal sealed class RetrieveAllOptionSetsRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveAllOptionSets", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var allOptionSets = store.GetAllGlobalOptionSets();
            var sdkOptionSets = allOptionSets
                .Select(os => (OptionSetMetadataBase)RetrieveOptionSetRequestHandler.ConvertToSdkOptionSet(os))
                .ToArray();

            var response = new RetrieveAllOptionSetsResponse();
            response.Results["OptionSetMetadata"] = sdkOptionSets;
            return response;
        }
    }
}
