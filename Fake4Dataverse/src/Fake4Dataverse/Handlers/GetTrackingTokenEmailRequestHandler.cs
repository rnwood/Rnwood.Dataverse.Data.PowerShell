using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="GetTrackingTokenEmailRequest"/> by returning a generated tracking token string.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns a deterministic tracking token string based on the current subject prefix. The token format follows the Dataverse convention (<c>CRM:NNNNNNN</c>).</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class GetTrackingTokenEmailRequestHandler : IOrganizationRequestHandler
    {
        private static int _tokenCounter;

        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "GetTrackingTokenEmail", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var token = System.Threading.Interlocked.Increment(ref _tokenCounter);

            var response = new GetTrackingTokenEmailResponse();
            response.Results["TrackingToken"] = $"CRM:{token:D7}";
            return response;
        }
    }
}
