using System;

namespace Fake4Dataverse.Metadata
{
    /// <summary>
    /// Specifies how operations cascade from parent to child records.
    /// </summary>
    public enum CascadeType
    {
        /// <summary>Do nothing to child records.</summary>
        NoCascade = 0,
        /// <summary>Apply the operation to all child records.</summary>
        Cascade = 1,
        /// <summary>Apply the operation only to active child records.</summary>
        Active = 2,
        /// <summary>Apply the operation only to child records owned by the same user.</summary>
        UserOwned = 3,
        /// <summary>Clear the foreign key (set to null) on child records.</summary>
        RemoveLink = 4,
        /// <summary>Prevent the operation if child records exist.</summary>
        Restrict = 5,
    }

    /// <summary>
    /// Defines cascade behavior for a one-to-many relationship.
    /// </summary>
    public sealed class CascadeConfiguration
    {
        /// <summary>Cascade behavior on delete of parent record.</summary>
        public CascadeType Delete { get; set; }
        /// <summary>Cascade behavior on assign (owner change) of parent record.</summary>
        public CascadeType Assign { get; set; }
        /// <summary>Cascade behavior on reparent (change of lookup value).</summary>
        public CascadeType Reparent { get; set; }
        /// <summary>Cascade behavior on share of parent record.</summary>
        public CascadeType Share { get; set; }
        /// <summary>Cascade behavior on unshare of parent record.</summary>
        public CascadeType Unshare { get; set; }
    }

    internal sealed class OneToManyRelationshipInfo
    {
        public string SchemaName { get; }
        public string ReferencedEntity { get; }
        public string ReferencedAttribute { get; }
        public string ReferencingEntity { get; }
        public string ReferencingAttribute { get; }
        public CascadeConfiguration Cascade { get; }

        public OneToManyRelationshipInfo(
            string schemaName,
            string referencedEntity,
            string referencedAttribute,
            string referencingEntity,
            string referencingAttribute,
            CascadeConfiguration? cascade = null)
        {
            SchemaName = schemaName ?? throw new ArgumentNullException(nameof(schemaName));
            ReferencedEntity = referencedEntity ?? throw new ArgumentNullException(nameof(referencedEntity));
            ReferencedAttribute = referencedAttribute ?? throw new ArgumentNullException(nameof(referencedAttribute));
            ReferencingEntity = referencingEntity ?? throw new ArgumentNullException(nameof(referencingEntity));
            ReferencingAttribute = referencingAttribute ?? throw new ArgumentNullException(nameof(referencingAttribute));
            Cascade = cascade ?? new CascadeConfiguration();
        }
    }

    internal sealed class ManyToManyRelationshipInfo
    {
        public string SchemaName { get; }
        public string Entity1LogicalName { get; }
        public string Entity2LogicalName { get; }
        public string? IntersectEntityName { get; }

        public ManyToManyRelationshipInfo(
            string schemaName,
            string entity1LogicalName,
            string entity2LogicalName,
            string? intersectEntityName = null)
        {
            SchemaName = schemaName ?? throw new ArgumentNullException(nameof(schemaName));
            Entity1LogicalName = entity1LogicalName ?? throw new ArgumentNullException(nameof(entity1LogicalName));
            Entity2LogicalName = entity2LogicalName ?? throw new ArgumentNullException(nameof(entity2LogicalName));
            IntersectEntityName = intersectEntityName;
        }
    }
}
