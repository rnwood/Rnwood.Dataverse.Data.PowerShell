using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveAllManagedPropertiesRequest"/> by returning all managed properties (solution-layer customizability settings) in the organization.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Returns an empty collection. Managed properties are not modeled in the fake.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RetrieveAllManagedPropertiesRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveAllManagedProperties", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            // In the fake, return an empty collection of managed properties
            var response = new RetrieveAllManagedPropertiesResponse();
            response.Results["ManagedPropertyMetadata"] = new EntityMetadataCollection();
            return response;
        }
    }
}
