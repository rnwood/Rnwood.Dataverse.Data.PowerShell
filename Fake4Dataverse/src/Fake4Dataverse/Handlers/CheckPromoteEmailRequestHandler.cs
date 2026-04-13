using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CheckPromoteEmailRequest"/> by always indicating the email should be promoted.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Always returns <c>ShouldPromote = true</c> and <c>ReasonCode = 0</c>. Does not check mailbox configuration, email correlation, or promotion rules.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class CheckPromoteEmailRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CheckPromoteEmail", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var response = new CheckPromoteEmailResponse();
            response.Results["ShouldPromote"] = true;
            response.Results["ReasonCode"] = 0;
            return response;
        }
    }
}
