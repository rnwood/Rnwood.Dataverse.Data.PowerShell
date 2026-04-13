using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="GrantAccessRequest"/> by adding shared access rights on a record.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Stores the shared access rights via <c>SecurityManager.GrantAccess</c>; permissions are enforced in subsequent Retrieve/RetrieveMultiple calls when <see cref="FakeOrganizationServiceOptions.EnforceSecurityRoles"/> is enabled.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.EnforceSecurityRoles"/> — granting access has no visible effect on retrieval unless security enforcement is enabled.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class GrantAccessRequestHandler : IOrganizationRequestHandler
    {
        private readonly Security.SecurityManager _security;

        public GrantAccessRequestHandler(Security.SecurityManager security)
        {
            _security = security;
        }

        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "GrantAccess", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var grantRequest = OrganizationRequestTypeAdapter.AsTyped<GrantAccessRequest>(request);
            var target = grantRequest.Target;
            var principalAccess = grantRequest.PrincipalAccess;

            if (service is FakeOrganizationService fakeService)
            {
                _security.CheckPrivilege(fakeService.CallerId, target.LogicalName, Security.PrivilegeType.Share);
            }

            _security.GrantAccess(
                target.LogicalName,
                target.Id,
                principalAccess.Principal.Id,
                principalAccess.AccessMask);

            return new GrantAccessResponse();
        }
    }
}
