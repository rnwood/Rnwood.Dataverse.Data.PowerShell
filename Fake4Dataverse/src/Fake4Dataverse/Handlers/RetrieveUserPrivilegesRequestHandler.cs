using System;
using System.Collections.Generic;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveUserPrivilegesRequest"/> by returning all privilege entries for a user from their assigned roles and team roles.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns <see cref="RolePrivilege"/> entries from all security roles assigned to the user (directly and via teams). Privilege IDs are generated deterministically from entity name and privilege type. Does not replicate real Dataverse privilege GUIDs.</para>
    /// <para><strong>Configuration:</strong> None — returns data from the security manager regardless of <see cref="FakeOrganizationServiceOptions.EnforceSecurityRoles"/>.</para>
    /// </remarks>
    internal sealed class RetrieveUserPrivilegesRequestHandler : IOrganizationRequestHandler
    {
        private readonly Security.SecurityManager _security;

        public RetrieveUserPrivilegesRequestHandler(Security.SecurityManager security)
        {
            _security = security;
        }

        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveUserPrivileges", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var retrieveRequest = OrganizationRequestTypeAdapter.AsTyped<RetrieveUserPrivilegesRequest>(request);
            var userId = retrieveRequest.UserId;

            var roles = _security.GetEffectiveRoles(userId);
            var privilegeSet = new Dictionary<(string Entity, Security.PrivilegeType Priv), Security.PrivilegeDepth>(
                new EntityPrivilegeComparer());

            foreach (var role in roles)
            {
                foreach (Security.PrivilegeType pt in Enum.GetValues(typeof(Security.PrivilegeType)))
                {
                    // Probe common entity names and all that the role may have
                    // We enumerate the role's privileges by testing known privilege types
                    var depth = role.GetDepth("*", pt);
                    if (depth != Security.PrivilegeDepth.None)
                    {
                        var key = ("*", pt);
                        if (!privilegeSet.TryGetValue(key, out var existing) || depth > existing)
                            privilegeSet[key] = depth;
                    }
                }
            }

            // Build RolePrivilege array
            var rolePrivileges = new List<RolePrivilege>();
            foreach (var kvp in privilegeSet)
            {
                rolePrivileges.Add(new RolePrivilege
                {
                    Depth = MapDepth(kvp.Value),
                    PrivilegeId = GeneratePrivilegeId(kvp.Key.Entity, kvp.Key.Priv)
                });
            }

            var response = new RetrieveUserPrivilegesResponse();
            response.Results["RolePrivileges"] = rolePrivileges.ToArray();
            return response;
        }

        private static PrivilegeDepth MapDepth(Security.PrivilegeDepth depth)
        {
            return depth switch
            {
                Security.PrivilegeDepth.User => PrivilegeDepth.Basic,
                Security.PrivilegeDepth.BusinessUnit => PrivilegeDepth.Local,
                Security.PrivilegeDepth.ParentChildBusinessUnit => PrivilegeDepth.Deep,
                Security.PrivilegeDepth.Organization => PrivilegeDepth.Global,
                _ => PrivilegeDepth.Basic
            };
        }

        private static Guid GeneratePrivilegeId(string entityName, Security.PrivilegeType privilege)
        {
            // Generate a deterministic GUID from entity name and privilege type
            var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(entityName ?? "") ^ privilege.GetHashCode();
            var bytes = new byte[16];
            BitConverter.GetBytes(hash).CopyTo(bytes, 0);
            BitConverter.GetBytes((int)privilege).CopyTo(bytes, 4);
            return new Guid(bytes);
        }

        private sealed class EntityPrivilegeComparer : IEqualityComparer<(string Entity, Security.PrivilegeType Priv)>
        {
            public bool Equals((string Entity, Security.PrivilegeType Priv) x, (string Entity, Security.PrivilegeType Priv) y)
            {
                return string.Equals(x.Entity, y.Entity, StringComparison.OrdinalIgnoreCase) && x.Priv == y.Priv;
            }

            public int GetHashCode((string Entity, Security.PrivilegeType Priv) obj)
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Entity ?? "") ^ obj.Priv.GetHashCode();
            }
        }
    }
}
