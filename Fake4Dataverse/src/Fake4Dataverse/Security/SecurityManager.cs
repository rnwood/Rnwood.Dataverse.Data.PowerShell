using System;
using System.Collections.Generic;
using Microsoft.Crm.Sdk.Messages;

namespace Fake4Dataverse.Security
{
    /// <summary>
    /// Manages security roles, privilege enforcement, and record sharing for the fake Dataverse service.
    /// </summary>
    public sealed class SecurityManager
    {
        /// <summary>Dataverse error code for privilege not satisfied (0x80040220).</summary>
        public const int PrivilegeDepthNotSatisfied = unchecked((int)0x80040220);

        private readonly Dictionary<Guid, List<SecurityRole>> _userRoles = new Dictionary<Guid, List<SecurityRole>>();
        private readonly Dictionary<Guid, HashSet<Guid>> _teamMembers = new Dictionary<Guid, HashSet<Guid>>();
        private readonly Dictionary<Guid, List<SecurityRole>> _teamRoles = new Dictionary<Guid, List<SecurityRole>>();
        private readonly Dictionary<(string EntityName, Guid EntityId, Guid PrincipalId), AccessRights> _sharingTable =
            new Dictionary<(string, Guid, Guid), AccessRights>();

        /// <summary>
        /// Gets or sets whether security role enforcement is enabled.
        /// When <c>false</c> (default), all operations are permitted regardless of roles.
        /// </summary>
        public bool EnforceSecurityRoles { get; set; }

        /// <summary>
        /// Assigns a security role to a user.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="role">The security role to assign.</param>
        public void AssignRole(Guid userId, SecurityRole role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            if (!_userRoles.TryGetValue(userId, out var roles))
            {
                roles = new List<SecurityRole>();
                _userRoles[userId] = roles;
            }
            roles.Add(role);
        }

        /// <summary>
        /// Removes all security roles from a user.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        public void ClearRoles(Guid userId)
        {
            _userRoles.Remove(userId);
        }

        /// <summary>
        /// Checks whether a user has the specified privilege on an entity.
        /// Throws a <see cref="System.ServiceModel.FaultException{T}"/> if enforcement is enabled and the privilege is not satisfied.
        /// </summary>
        /// <param name="userId">The user performing the operation.</param>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <param name="privilege">The required privilege type.</param>
        public void CheckPrivilege(Guid userId, string entityName, PrivilegeType privilege)
        {
            if (!EnforceSecurityRoles) return;

            if (HasPrivilegeViaRolesOrTeams(userId, entityName, privilege))
                return;

            throw DataverseFault.Create(PrivilegeDepthNotSatisfied,
                $"Principal user (Id={userId}) is missing {privilege} privilege for entity '{entityName}'.");
        }

        /// <summary>
        /// Checks whether a user has the specified privilege on a specific record,
        /// taking into account sharing and ownership.
        /// </summary>
        /// <param name="userId">The user performing the operation.</param>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <param name="entityId">The record ID.</param>
        /// <param name="privilege">The required privilege type.</param>
        /// <param name="ownerId">The record's owner ID, or <c>null</c> if unknown.</param>
        public void CheckRecordPrivilege(Guid userId, string entityName, Guid entityId, PrivilegeType privilege, Guid? ownerId)
        {
            if (!EnforceSecurityRoles) return;

            // Owner always has access to their own records
            if (ownerId.HasValue && ownerId.Value == userId)
                return;

            // Check role-based privileges considering depth.
            // User-depth only allows access to own records (already checked above).
            // BusinessUnit and Organization depth grant access to non-owned records.
            var maxDepth = GetMaxDepthForPrivilege(userId, entityName, privilege);
            if (maxDepth >= PrivilegeDepth.BusinessUnit)
                return;

            // Check sharing table
            if (_sharingTable.TryGetValue((entityName, entityId, userId), out var sharedRights))
            {
                var requiredRight = MapPrivilegeToAccessRights(privilege);
                if (requiredRight != AccessRights.None && (sharedRights & requiredRight) == requiredRight)
                    return;
            }

            throw DataverseFault.Create(PrivilegeDepthNotSatisfied,
                $"Principal user (Id={userId}) is missing {privilege} privilege for entity '{entityName}' with Id={entityId}.");
        }

        /// <summary>
        /// Grants access rights on a record to a principal.
        /// </summary>
        /// <param name="entityName">The entity logical name.</param>
        /// <param name="entityId">The record ID.</param>
        /// <param name="principalId">The principal (user or team) ID.</param>
        /// <param name="rights">The access rights to grant.</param>
        public void GrantAccess(string entityName, Guid entityId, Guid principalId, AccessRights rights)
        {
            var key = (entityName, entityId, principalId);
            if (_sharingTable.TryGetValue(key, out var existing))
                _sharingTable[key] = existing | rights;
            else
                _sharingTable[key] = rights;
        }

        /// <summary>
        /// Modifies (replaces) access rights on a record for a principal.
        /// </summary>
        /// <param name="entityName">The entity logical name.</param>
        /// <param name="entityId">The record ID.</param>
        /// <param name="principalId">The principal (user or team) ID.</param>
        /// <param name="rights">The new access rights.</param>
        public void ModifyAccess(string entityName, Guid entityId, Guid principalId, AccessRights rights)
        {
            _sharingTable[(entityName, entityId, principalId)] = rights;
        }

        /// <summary>
        /// Revokes all access rights on a record for a principal.
        /// </summary>
        /// <param name="entityName">The entity logical name.</param>
        /// <param name="entityId">The record ID.</param>
        /// <param name="principalId">The principal (user or team) ID.</param>
        public void RevokeAccess(string entityName, Guid entityId, Guid principalId)
        {
            _sharingTable.Remove((entityName, entityId, principalId));
        }

        /// <summary>
        /// Retrieves the effective access rights a principal has on a record,
        /// combining ownership-based and shared access.
        /// </summary>
        /// <param name="entityName">The entity logical name.</param>
        /// <param name="entityId">The record ID.</param>
        /// <param name="principalId">The principal ID.</param>
        /// <param name="ownerId">The record owner's ID, or <c>null</c> if unknown.</param>
        /// <returns>The combined <see cref="AccessRights"/>.</returns>
        public AccessRights RetrievePrincipalAccess(string entityName, Guid entityId, Guid principalId, Guid? ownerId)
        {
            var rights = AccessRights.None;

            // Owner gets all rights
            if (ownerId.HasValue && ownerId.Value == principalId)
            {
                rights = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.DeleteAccess
                         | AccessRights.AppendAccess | AccessRights.AppendToAccess
                         | AccessRights.ShareAccess | AccessRights.AssignAccess;
            }

            // Add shared rights
            if (_sharingTable.TryGetValue((entityName, entityId, principalId), out var shared))
            {
                rights |= shared;
            }

            // Add role-based rights (user roles + team roles)
            foreach (PrivilegeType pt in Enum.GetValues(typeof(PrivilegeType)))
            {
                if (HasPrivilegeViaRolesOrTeams(principalId, entityName, pt))
                    rights |= MapPrivilegeToAccessRights(pt);
            }

            return rights;
        }

        /// <summary>
        /// Adds a user to a team.
        /// </summary>
        /// <param name="teamId">The team's ID.</param>
        /// <param name="userId">The user's ID.</param>
        public void AddTeamMember(Guid teamId, Guid userId)
        {
            if (!_teamMembers.TryGetValue(teamId, out var members))
            {
                members = new HashSet<Guid>();
                _teamMembers[teamId] = members;
            }
            members.Add(userId);
        }

        /// <summary>
        /// Removes a user from a team.
        /// </summary>
        /// <param name="teamId">The team's ID.</param>
        /// <param name="userId">The user's ID.</param>
        public void RemoveTeamMember(Guid teamId, Guid userId)
        {
            if (_teamMembers.TryGetValue(teamId, out var members))
                members.Remove(userId);
        }

        /// <summary>
        /// Assigns a security role to a team. Members of the team inherit the role's privileges.
        /// </summary>
        /// <param name="teamId">The team's ID.</param>
        /// <param name="role">The security role to assign.</param>
        public void AssignTeamRole(Guid teamId, SecurityRole role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            if (!_teamRoles.TryGetValue(teamId, out var roles))
            {
                roles = new List<SecurityRole>();
                _teamRoles[teamId] = roles;
            }
            roles.Add(role);
        }

        /// <summary>
        /// Grants access rights on a record to all current members of a team.
        /// </summary>
        /// <param name="entityName">The entity logical name.</param>
        /// <param name="entityId">The record ID.</param>
        /// <param name="teamId">The team ID.</param>
        /// <param name="rights">The access rights to grant.</param>
        public void GrantTeamAccess(string entityName, Guid entityId, Guid teamId, AccessRights rights)
        {
            if (_teamMembers.TryGetValue(teamId, out var members))
            {
                foreach (var memberId in members)
                {
                    GrantAccess(entityName, entityId, memberId, rights);
                }
            }
        }

        /// <summary>
        /// Clears all sharing records from the sharing table.
        /// </summary>
        internal void ClearSharing()
        {
            _sharingTable.Clear();
        }

        /// <summary>
        /// Returns whether the specified user can access a record for the given privilege,
        /// considering role depth, ownership, and sharing.
        /// </summary>
        internal bool CanAccessRecord(Guid userId, string entityName, Guid entityId, PrivilegeType privilege, Guid? ownerId)
        {
            if (!EnforceSecurityRoles) return true;

            // Owner always has access
            if (ownerId.HasValue && ownerId.Value == userId)
                return true;

            // Organization or BusinessUnit depth grants access to non-owned records
            var maxDepth = GetMaxDepthForPrivilege(userId, entityName, privilege);
            if (maxDepth >= PrivilegeDepth.BusinessUnit)
                return true;

            // Check sharing table
            if (_sharingTable.TryGetValue((entityName, entityId, userId), out var sharedRights))
            {
                var requiredRight = MapPrivilegeToAccessRights(privilege);
                if (requiredRight != AccessRights.None && (sharedRights & requiredRight) == requiredRight)
                    return true;
            }

            return false;
        }

        private bool HasPrivilegeViaRolesOrTeams(Guid userId, string entityName, PrivilegeType privilege)
        {
            return GetMaxDepthForPrivilege(userId, entityName, privilege) != PrivilegeDepth.None;
        }

        /// <summary>
        /// Returns the maximum privilege depth a user has for a given entity and privilege type,
        /// considering both direct roles and team roles.
        /// </summary>
        internal PrivilegeDepth GetMaxDepthForPrivilege(Guid userId, string entityName, PrivilegeType privilege)
        {
            var maxDepth = PrivilegeDepth.None;

            // Check user's own roles
            if (_userRoles.TryGetValue(userId, out var roles))
            {
                foreach (var role in roles)
                {
                    var depth = role.GetDepth(entityName, privilege);
                    if (depth > maxDepth) maxDepth = depth;
                }
            }

            // Check roles assigned to any teams the user belongs to
            foreach (var team in _teamMembers)
            {
                if (team.Value.Contains(userId) && _teamRoles.TryGetValue(team.Key, out var teamRoles))
                {
                    foreach (var role in teamRoles)
                    {
                        var depth = role.GetDepth(entityName, privilege);
                        if (depth > maxDepth) maxDepth = depth;
                    }
                }
            }

            return maxDepth;
        }

        /// <summary>
        /// Returns all principals and their shared access rights for a specific record.
        /// </summary>
        /// <param name="entityName">The entity logical name.</param>
        /// <param name="entityId">The record ID.</param>
        /// <returns>A list of (PrincipalId, AccessRights) tuples.</returns>
        internal List<(Guid PrincipalId, AccessRights Rights)> GetSharedPrincipalsAndAccess(string entityName, Guid entityId)
        {
            var result = new List<(Guid, AccessRights)>();
            foreach (var kvp in _sharingTable)
            {
                if (string.Equals(kvp.Key.EntityName, entityName, StringComparison.OrdinalIgnoreCase) && kvp.Key.EntityId == entityId)
                    result.Add((kvp.Key.PrincipalId, kvp.Value));
            }
            return result;
        }

        /// <summary>
        /// Returns all roles assigned to a user, including roles inherited from team membership.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <returns>A list of security roles.</returns>
        internal IReadOnlyList<SecurityRole> GetEffectiveRoles(Guid userId)
        {
            var roles = new List<SecurityRole>();
            if (_userRoles.TryGetValue(userId, out var directRoles))
                roles.AddRange(directRoles);

            foreach (var team in _teamMembers)
            {
                if (team.Value.Contains(userId) && _teamRoles.TryGetValue(team.Key, out var teamRoles))
                    roles.AddRange(teamRoles);
            }
            return roles;
        }

        /// <summary>
        /// Returns the set of team member user IDs for a given team.
        /// </summary>
        /// <param name="teamId">The team's ID.</param>
        /// <returns>An enumerable of user IDs, or empty if the team has no members.</returns>
        internal IEnumerable<Guid> GetTeamMembers(Guid teamId)
        {
            if (_teamMembers.TryGetValue(teamId, out var members))
                return members;
            return Array.Empty<Guid>();
        }

        private static AccessRights MapPrivilegeToAccessRights(PrivilegeType privilege)
        {
            return privilege switch
            {
                PrivilegeType.Read => AccessRights.ReadAccess,
                PrivilegeType.Write => AccessRights.WriteAccess,
                PrivilegeType.Delete => AccessRights.DeleteAccess,
                PrivilegeType.Append => AccessRights.AppendAccess,
                PrivilegeType.AppendTo => AccessRights.AppendToAccess,
                PrivilegeType.Share => AccessRights.ShareAccess,
                PrivilegeType.Assign => AccessRights.AssignAccess,
                PrivilegeType.Create => AccessRights.CreateAccess,
                _ => AccessRights.None
            };
        }
    }
}
