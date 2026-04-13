using System.Linq;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveAllEntitiesRequest"/> by returning metadata for all entities currently registered in the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns only entities explicitly registered via <c>env.RegisterEntity(...)</c> or auto-discovered. Real Dataverse returns hundreds of system and custom entities; the fake only knows entities that have been registered.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.ValidateWithMetadata"/> — entities are only available when metadata has been registered, but this handler returns whatever is in the store regardless of this option.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class RetrieveAllEntitiesRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveAllEntities", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var allEntities = store.GetAllEntityMetadataInfo();
            var sdkEntities = allEntities
                .Select(e =>
                {
                    // Return the full-fidelity SDK object when registered (preserves option-set labels etc.)
                    var cached = store.GetSdkEntityMetadata(e.LogicalName);
                    if (cached != null)
                        return cached;
                    return SdkMetadataConverter.ToSdkEntityMetadata(
                        e,
                        store.GetOneToManyRelationships(e.LogicalName),
                        store.GetManyToManyRelationships(e.LogicalName));
                })
                .ToArray();

            var response = new RetrieveAllEntitiesResponse();
            response.Results["EntityMetadata"] = sdkEntities;
            return response;
        }
    }
}
