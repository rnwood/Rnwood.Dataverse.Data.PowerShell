using System;
using System.Linq;
using System.Reflection;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveEntityKeyRequest"/> by retrieving an alternate key metadata definition from the in-memory metadata store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns the <see cref="Microsoft.Xrm.Sdk.Metadata.EntityKeyMetadata"/> registered for the entity/key name combination. Throws <c>FaultException</c> if not registered.</para>
    /// <para><strong>Configuration:</strong> None — returns whatever is in the in-memory metadata store.</para>
    /// </remarks>
    internal sealed class RetrieveEntityKeyRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveEntityKey", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var retrieveRequest = OrganizationRequestTypeAdapter.AsTyped<RetrieveEntityKeyRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            var entityName = retrieveRequest.Parameters.ContainsKey("EntityLogicalName") ? (string)retrieveRequest.Parameters["EntityLogicalName"] : null;
            var keyName = retrieveRequest.Parameters.ContainsKey("LogicalName") ? (string)retrieveRequest.Parameters["LogicalName"] : null;
            if (string.IsNullOrEmpty(entityName))
                throw DataverseFault.InvalidArgumentFault("Entity logical name is required.");
            if (string.IsNullOrEmpty(keyName))
                throw DataverseFault.InvalidArgumentFault("Key name is required.");

            var keyInfo = store.GetAlternateKey(entityName!, keyName!);
            if (keyInfo == null)
            {
                throw DataverseFault.Create(
                    DataverseFault.ObjectDoesNotExist,
                    $"Alternate key '{keyName}' does not exist on entity '{entityName}'.");
            }

            var sdkKey = new EntityKeyMetadata();
            sdkKey.LogicalName = keyInfo.Name;
            sdkKey.KeyAttributes = keyInfo.AttributeNames;

            var response = new RetrieveEntityKeyResponse();
            response.Results["EntityKeyMetadata"] = sdkKey;
            return response;
        }
    }
}
