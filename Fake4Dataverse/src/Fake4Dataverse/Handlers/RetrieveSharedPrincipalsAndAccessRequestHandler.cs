using System;
using System.Collections.Generic;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveSharedPrincipalsAndAccessRequest"/> by querying the sharing table for all principals with shared access to a record.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Returns all principals (users and teams) who have been explicitly granted shared access to the target record. Does not include implied access from ownership or role-based privileges.</para>
    /// <para><strong>Configuration:</strong> None — always returns data from the sharing table regardless of <see cref="FakeOrganizationServiceOptions.EnforceSecurityRoles"/>.</para>
    /// </remarks>
    internal sealed class RetrieveSharedPrincipalsAndAccessRequestHandler : IOrganizationRequestHandler
    {
        private readonly Security.SecurityManager _security;

        public RetrieveSharedPrincipalsAndAccessRequestHandler(Security.SecurityManager security)
        {
            _security = security;
        }

        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveSharedPrincipalsAndAccess", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var retrieveRequest = OrganizationRequestTypeAdapter.AsTyped<RetrieveSharedPrincipalsAndAccessRequest>(request);
            var target = retrieveRequest.Target;

            var shared = _security.GetSharedPrincipalsAndAccess(target.LogicalName, target.Id);

            var principalAccesses = new List<PrincipalAccess>();
            foreach (var (principalId, rights) in shared)
            {
                principalAccesses.Add(new PrincipalAccess
                {
                    Principal = new EntityReference("systemuser", principalId),
                    AccessMask = rights
                });
            }

            var response = new RetrieveSharedPrincipalsAndAccessResponse();
            response.Results["PrincipalAccesses"] = principalAccesses.ToArray();
            return response;
        }
    }
}
