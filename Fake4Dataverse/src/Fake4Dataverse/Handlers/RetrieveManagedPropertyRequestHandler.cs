using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveManagedPropertyRequest"/> by retrieving a specific managed property (customizability setting) for a solution component.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Returns <see langword="null"/> / an empty response. Managed properties are not modeled in the fake.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RetrieveManagedPropertyRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveManagedProperty", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            // In the fake, return a stub response for managed property retrieval
            var response = new RetrieveManagedPropertyResponse();
            response.Results["ManagedPropertyMetadata"] = null;
            return response;
        }
    }
}
