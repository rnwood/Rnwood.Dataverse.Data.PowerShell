using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="SetBusinessSystemUserRequest"/>.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Returns a structurally valid response with no side effects.</para>
    /// </remarks>
    internal sealed class SetBusinessSystemUserRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "SetBusinessSystemUser", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service) =>
            new SetBusinessSystemUserResponse();
    }
}