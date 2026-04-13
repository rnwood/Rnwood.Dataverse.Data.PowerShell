using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveEntityRequest"/> by retrieving entity metadata (schema definition) from the in-memory metadata store, including relationships when requested.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns the <see cref="Microsoft.Xrm.Sdk.Metadata.EntityMetadata"/> registered via <c>env.RegisterEntity(...)</c> or auto-discovered; includes 1:N and N:N relationships that have been registered. System metadata fields (e.g., <c>IsCustomizable</c>, managed properties) are not populated.</para>
    /// <para><strong>Configuration:</strong> None — returns whatever is in the in-memory metadata store.</para>
    /// </remarks>
    internal sealed class RetrieveEntityRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveEntity", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var retrieveRequest = OrganizationRequestTypeAdapter.AsTyped<RetrieveEntityRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            // Return the full-fidelity SDK object when registered (preserves option-set labels etc.)
            var sdkMetadata = store.GetSdkEntityMetadata(retrieveRequest.LogicalName);
            if (sdkMetadata != null)
            {
                var response = new RetrieveEntityResponse();
                response.Results["EntityMetadata"] = sdkMetadata;
                return response;
            }

            var entityInfo = store.GetEntityMetadataInfo(retrieveRequest.LogicalName);
            if (entityInfo == null)
            {
                throw DataverseFault.Create(
                    DataverseFault.ObjectDoesNotExist,
                    $"Entity '{retrieveRequest.LogicalName}' metadata does not exist.");
            }

            var oneToMany = store.GetOneToManyRelationships(retrieveRequest.LogicalName);
            var manyToMany = store.GetManyToManyRelationships(retrieveRequest.LogicalName);
            var sdkEntity = SdkMetadataConverter.ToSdkEntityMetadata(entityInfo, oneToMany, manyToMany);

            var response2 = new RetrieveEntityResponse();
            response2.Results["EntityMetadata"] = sdkEntity;
            return response2;
        }
    }
}
