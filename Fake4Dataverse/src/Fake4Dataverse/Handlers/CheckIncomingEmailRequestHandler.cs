using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CheckIncomingEmailRequest"/> by always indicating the email should be delivered.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Always returns <c>ShouldDeliver = true</c> and <c>ReasonCode = 0</c>. Does not check mailbox configuration, tracking tokens, or duplicate detection rules.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class CheckIncomingEmailRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CheckIncomingEmail", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var response = new CheckIncomingEmailResponse();
            response.Results["ShouldDeliver"] = true;
            response.Results["ReasonCode"] = 0;
            return response;
        }
    }
}
