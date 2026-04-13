using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="ModifyAccessRequest"/> by replacing shared access rights on a record.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Replaces the principal's existing access rights with the specified <c>AccessMask</c> via <c>SecurityManager.ModifyAccess</c>. Takes effect on subsequent Retrieve/RetrieveMultiple when <see cref="FakeOrganizationServiceOptions.EnforceSecurityRoles"/> is enabled.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.EnforceSecurityRoles"/> — modified rights only affect retrieval when security enforcement is enabled.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class ModifyAccessRequestHandler : IOrganizationRequestHandler
    {
        private readonly Security.SecurityManager _security;

        public ModifyAccessRequestHandler(Security.SecurityManager security)
        {
            _security = security;
        }

        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "ModifyAccess", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var modifyRequest = OrganizationRequestTypeAdapter.AsTyped<ModifyAccessRequest>(request);
            var target = modifyRequest.Target;
            var principalAccess = modifyRequest.PrincipalAccess;

            _security.ModifyAccess(
                target.LogicalName,
                target.Id,
                principalAccess.Principal.Id,
                principalAccess.AccessMask);

            return new ModifyAccessResponse();
        }
    }
}
