using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse.Metadata
{
    /// <summary>
    /// Fluent builder for configuring entity metadata within <see cref="InMemoryMetadataStore"/>.
    /// </summary>
    public sealed class EntityMetadataBuilder
    {
        private readonly EntityMetadataInfo _entity;
        private readonly InMemoryMetadataStore _store;

        internal EntityMetadataBuilder(EntityMetadataInfo entity, InMemoryMetadataStore store)
        {
            _entity = entity;
            _store = store;
        }

        /// <summary>Sets the schema name for the entity.</summary>
        public EntityMetadataBuilder WithSchemaName(string schemaName)
        {
            _entity.SchemaName = schemaName;
            return this;
        }

        /// <summary>Sets the primary ID attribute for the entity.</summary>
        public EntityMetadataBuilder WithPrimaryIdAttribute(string attributeName)
        {
            _entity.PrimaryIdAttribute = attributeName;
            return this;
        }

        /// <summary>Sets the primary name attribute for the entity.</summary>
        public EntityMetadataBuilder WithPrimaryNameAttribute(string attributeName)
        {
            _entity.PrimaryNameAttribute = attributeName;
            return this;
        }

        /// <summary>Sets the object type code for the entity.</summary>
        public EntityMetadataBuilder WithObjectTypeCode(int code)
        {
            _entity.ObjectTypeCode = code;
            return this;
        }

        /// <summary>Adds a generic attribute with the specified type and required level.</summary>
        public EntityMetadataBuilder WithAttribute(
            string logicalName,
            AttributeTypeCode type,
            AttributeRequiredLevel requiredLevel = AttributeRequiredLevel.None)
        {
            _entity.Attributes[logicalName] = new AttributeMetadataInfo(logicalName, type)
            {
                RequiredLevel = requiredLevel
            };
            return this;
        }

        /// <summary>Adds a string attribute with optional max length constraint.</summary>
        public EntityMetadataBuilder WithStringAttribute(
            string logicalName,
            int? maxLength = null,
            AttributeRequiredLevel requiredLevel = AttributeRequiredLevel.None)
        {
            _entity.Attributes[logicalName] = new AttributeMetadataInfo(logicalName, AttributeTypeCode.String)
            {
                RequiredLevel = requiredLevel,
                MaxLength = maxLength
            };
            return this;
        }

        /// <summary>Adds an integer attribute with optional min/max constraints.</summary>
        public EntityMetadataBuilder WithIntegerAttribute(
            string logicalName,
            int? minValue = null,
            int? maxValue = null,
            AttributeRequiredLevel requiredLevel = AttributeRequiredLevel.None)
        {
            _entity.Attributes[logicalName] = new AttributeMetadataInfo(logicalName, AttributeTypeCode.Integer)
            {
                RequiredLevel = requiredLevel,
                MinValue = minValue,
                MaxValue = maxValue
            };
            return this;
        }

        /// <summary>Adds a decimal attribute with optional min/max constraints.</summary>
        public EntityMetadataBuilder WithDecimalAttribute(
            string logicalName,
            decimal? minValue = null,
            decimal? maxValue = null,
            AttributeRequiredLevel requiredLevel = AttributeRequiredLevel.None)
        {
            _entity.Attributes[logicalName] = new AttributeMetadataInfo(logicalName, AttributeTypeCode.Decimal)
            {
                RequiredLevel = requiredLevel,
                MinValue = minValue.HasValue ? (double)minValue.Value : (double?)null,
                MaxValue = maxValue.HasValue ? (double)maxValue.Value : (double?)null
            };
            return this;
        }

        /// <summary>Adds a double (floating point) attribute with optional min/max constraints.</summary>
        public EntityMetadataBuilder WithDoubleAttribute(
            string logicalName,
            double? minValue = null,
            double? maxValue = null,
            AttributeRequiredLevel requiredLevel = AttributeRequiredLevel.None)
        {
            _entity.Attributes[logicalName] = new AttributeMetadataInfo(logicalName, AttributeTypeCode.Double)
            {
                RequiredLevel = requiredLevel,
                MinValue = minValue,
                MaxValue = maxValue
            };
            return this;
        }

        /// <summary>Adds a money attribute with optional min/max constraints.</summary>
        public EntityMetadataBuilder WithMoneyAttribute(
            string logicalName,
            double? minValue = null,
            double? maxValue = null,
            AttributeRequiredLevel requiredLevel = AttributeRequiredLevel.None)
        {
            _entity.Attributes[logicalName] = new AttributeMetadataInfo(logicalName, AttributeTypeCode.Money)
            {
                RequiredLevel = requiredLevel,
                MinValue = minValue,
                MaxValue = maxValue
            };
            return this;
        }

        /// <summary>Adds a boolean attribute.</summary>
        public EntityMetadataBuilder WithBooleanAttribute(
            string logicalName,
            AttributeRequiredLevel requiredLevel = AttributeRequiredLevel.None)
        {
            _entity.Attributes[logicalName] = new AttributeMetadataInfo(logicalName, AttributeTypeCode.Boolean)
            {
                RequiredLevel = requiredLevel
            };
            return this;
        }

        /// <summary>Adds a DateTime attribute.</summary>
        public EntityMetadataBuilder WithDateTimeAttribute(
            string logicalName,
            AttributeRequiredLevel requiredLevel = AttributeRequiredLevel.None)
        {
            _entity.Attributes[logicalName] = new AttributeMetadataInfo(logicalName, AttributeTypeCode.DateTime)
            {
                RequiredLevel = requiredLevel
            };
            return this;
        }

        /// <summary>Adds a picklist (option set) attribute with optional valid values constraint.</summary>
        public EntityMetadataBuilder WithOptionSetAttribute(
            string logicalName,
            int[]? validValues = null,
            AttributeRequiredLevel requiredLevel = AttributeRequiredLevel.None)
        {
            var info = new AttributeMetadataInfo(logicalName, AttributeTypeCode.Picklist)
            {
                RequiredLevel = requiredLevel
            };
            if (validValues != null)
                info.ValidOptionSetValues = new HashSet<int>(validValues);
            _entity.Attributes[logicalName] = info;
            return this;
        }

        /// <summary>Adds a lookup attribute with optional target entity type constraint.</summary>
        public EntityMetadataBuilder WithLookupAttribute(
            string logicalName,
            string[]? targetEntityTypes = null,
            AttributeRequiredLevel requiredLevel = AttributeRequiredLevel.None)
        {
            var info = new AttributeMetadataInfo(logicalName, AttributeTypeCode.Lookup)
            {
                RequiredLevel = requiredLevel
            };
            if (targetEntityTypes != null)
                info.ValidTargetEntityTypes = new HashSet<string>(targetEntityTypes, StringComparer.OrdinalIgnoreCase);
            _entity.Attributes[logicalName] = info;
            return this;
        }

        /// <summary>
        /// Defines a one-to-many (1:N) relationship involving this entity.
        /// </summary>
        public EntityMetadataBuilder WithOneToManyRelationship(
            string schemaName,
            string referencedEntity,
            string referencedAttribute,
            string referencingEntity,
            string referencingAttribute)
        {
            _store.AddOneToManyRelationship(schemaName, referencedEntity, referencedAttribute, referencingEntity, referencingAttribute);
            return this;
        }

        /// <summary>
        /// Defines a many-to-many (N:N) relationship involving this entity.
        /// </summary>
        public EntityMetadataBuilder WithManyToManyRelationship(
            string schemaName,
            string entity1LogicalName,
            string entity2LogicalName,
            string? intersectEntityName = null)
        {
            _store.AddManyToManyRelationship(schemaName, entity1LogicalName, entity2LogicalName, intersectEntityName);
            return this;
        }

        /// <summary>
        /// Defines an alternate key for the entity.
        /// </summary>
        /// <param name="name">The key name.</param>
        /// <param name="attributeNames">The attribute logical names that comprise the key.</param>
        public EntityMetadataBuilder WithAlternateKey(string name, params string[] attributeNames)
        {
            _entity.AlternateKeys.Add(new AlternateKeyInfo(name, attributeNames));
            return this;
        }
    }
}
