using System;
using System.Collections.Generic;

namespace Fake4Dataverse.Security
{
    /// <summary>
    /// Represents a Dataverse security role with entity-level privilege definitions.
    /// </summary>
    public sealed class SecurityRole
    {
        private readonly Dictionary<(string EntityName, PrivilegeType Privilege), PrivilegeDepth> _privileges =
            new Dictionary<(string, PrivilegeType), PrivilegeDepth>(new EntityPrivilegeComparer());

        /// <summary>
        /// Gets the name of this security role.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a new security role with the specified name.
        /// </summary>
        /// <param name="name">The role name.</param>
        public SecurityRole(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Adds or updates a privilege for an entity on this role.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <param name="privilege">The privilege type.</param>
        /// <param name="depth">The privilege depth.</param>
        /// <returns>This role instance for fluent chaining.</returns>
        public SecurityRole AddPrivilege(string entityName, PrivilegeType privilege, PrivilegeDepth depth)
        {
            if (string.IsNullOrEmpty(entityName))
                throw new ArgumentException("Entity name is required.", nameof(entityName));

            _privileges[(entityName, privilege)] = depth;
            return this;
        }

        /// <summary>
        /// Gets the privilege depth for a specific entity and privilege type.
        /// Returns <see cref="PrivilegeDepth.None"/> if no privilege is defined.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <param name="privilege">The privilege type.</param>
        /// <returns>The privilege depth.</returns>
        public PrivilegeDepth GetDepth(string entityName, PrivilegeType privilege)
        {
            return _privileges.TryGetValue((entityName, privilege), out var depth) ? depth : PrivilegeDepth.None;
        }

        private sealed class EntityPrivilegeComparer : IEqualityComparer<(string EntityName, PrivilegeType Privilege)>
        {
            public bool Equals((string EntityName, PrivilegeType Privilege) x, (string EntityName, PrivilegeType Privilege) y)
            {
                return string.Equals(x.EntityName, y.EntityName, StringComparison.OrdinalIgnoreCase)
                    && x.Privilege == y.Privilege;
            }

            public int GetHashCode((string EntityName, PrivilegeType Privilege) obj)
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.EntityName) ^ obj.Privilege.GetHashCode();
            }
        }
    }
}
