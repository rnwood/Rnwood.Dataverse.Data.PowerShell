using System.Linq;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveAttributeRequest"/> by retrieving a specific attribute's metadata definition from the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns the <c>AttributeMetadata</c> registered for the entity/attribute combination. Throws <c>FaultException</c> if not registered (matching real Dataverse behavior), but only attributes explicitly registered are available.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.ValidateWithMetadata"/> — attribute metadata is only available when it has been registered; the handler throws if the attribute is not found.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class RetrieveAttributeRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveAttribute", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var retrieveRequest = OrganizationRequestTypeAdapter.AsTyped<RetrieveAttributeRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            // Return the full-fidelity SDK attribute when the entity has registered SDK metadata
            var sdkEntityMetadata = store.GetSdkEntityMetadata(retrieveRequest.EntityLogicalName);
            if (sdkEntityMetadata?.Attributes != null)
            {
                var sdkAttr = sdkEntityMetadata.Attributes.FirstOrDefault(a =>
                    string.Equals(a.LogicalName, retrieveRequest.LogicalName, System.StringComparison.OrdinalIgnoreCase));
                if (sdkAttr != null)
                {
                    var response = new RetrieveAttributeResponse();
                    response.Results["AttributeMetadata"] = sdkAttr;
                    return response;
                }
            }

            var entityInfo = store.GetEntityMetadataInfo(retrieveRequest.EntityLogicalName);
            if (entityInfo == null)
            {
                throw DataverseFault.Create(
                    DataverseFault.ObjectDoesNotExist,
                    $"Entity '{retrieveRequest.EntityLogicalName}' metadata does not exist.");
            }

            if (!entityInfo.Attributes.TryGetValue(retrieveRequest.LogicalName, out var attrInfo))
            {
                throw DataverseFault.Create(
                    DataverseFault.ObjectDoesNotExist,
                    $"Attribute '{retrieveRequest.LogicalName}' does not exist on entity '{retrieveRequest.EntityLogicalName}'.");
            }

            var sdkAttribute = SdkMetadataConverter.ToSdkAttributeMetadata(attrInfo);

            var response2 = new RetrieveAttributeResponse();
            response2.Results["AttributeMetadata"] = sdkAttribute;
            return response2;
        }
    }
}
