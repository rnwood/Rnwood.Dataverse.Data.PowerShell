using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CreateAsyncJobToRevokeInheritedAccessRequest"/> — creates a background job to revoke inherited access from child records.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Immediately returns a fake async job <see cref="EntityReference"/>; no access revocation is performed. Real Dataverse queues a background job to walk the record hierarchy.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class CreateAsyncJobToRevokeInheritedAccessRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CreateAsyncJobToRevokeInheritedAccess", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            // In the fake, this is a no-op that returns a job ID
            var response = new CreateAsyncJobToRevokeInheritedAccessResponse();
            response.Results["AsyncJobId"] = Guid.NewGuid();
            return response;
        }
    }
}
