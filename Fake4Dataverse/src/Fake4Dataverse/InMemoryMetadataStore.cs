using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Fake4Dataverse
{
    /// <summary>
    /// Thread-safe in-memory store for entity and attribute metadata.
    /// Supports fluent configuration, validation, and auto-discovery.
    /// </summary>
    public sealed class InMemoryMetadataStore
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string, EntityMetadataInfo> _entities =
            new Dictionary<string, EntityMetadataInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, OneToManyRelationshipInfo> _oneToManyRelationships =
            new Dictionary<string, OneToManyRelationshipInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ManyToManyRelationshipInfo> _manyToManyRelationships =
            new Dictionary<string, ManyToManyRelationshipInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EntityMetadata> _sdkMetadataCache =
            new Dictionary<string, EntityMetadata>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets whether metadata is automatically inferred from entities on Create.
        /// When <c>true</c>, attribute types are discovered from the values in created entities.
        /// </summary>
        public bool AutoDiscoverMetadata { get; set; }

        /// <summary>
        /// Adds entity metadata and returns a fluent builder for further configuration.
        /// If metadata for the entity already exists, the existing metadata is returned for modification.
        /// </summary>
        public EntityMetadataBuilder AddEntity(string logicalName)
        {
            if (string.IsNullOrEmpty(logicalName))
                throw new ArgumentException("Entity logical name is required.", nameof(logicalName));

            lock (_lock)
            {
                if (!_entities.TryGetValue(logicalName, out var entity))
                {
                    entity = new EntityMetadataInfo(logicalName);
                    _entities[logicalName] = entity;
                }
                return new EntityMetadataBuilder(entity, this);
            }
        }

        /// <summary>
        /// Registers a one-to-many (1:N) relationship.
        /// </summary>
        public void AddOneToManyRelationship(
            string schemaName,
            string referencedEntity,
            string referencedAttribute,
            string referencingEntity,
            string referencingAttribute)
        {
            var info = new OneToManyRelationshipInfo(
                schemaName, referencedEntity, referencedAttribute, referencingEntity, referencingAttribute);
            lock (_lock)
            {
                _oneToManyRelationships[schemaName] = info;
            }
        }

        /// <summary>
        /// Registers a one-to-many (1:N) relationship with cascade configuration.
        /// </summary>
        public void AddOneToManyRelationship(
            string schemaName,
            string referencedEntity,
            string referencedAttribute,
            string referencingEntity,
            string referencingAttribute,
            Metadata.CascadeConfiguration cascade)
        {
            var info = new OneToManyRelationshipInfo(
                schemaName, referencedEntity, referencedAttribute, referencingEntity, referencingAttribute, cascade);
            lock (_lock)
            {
                _oneToManyRelationships[schemaName] = info;
            }
        }

        /// <summary>
        /// Registers a many-to-many (N:N) relationship.
        /// </summary>
        public void AddManyToManyRelationship(
            string schemaName,
            string entity1LogicalName,
            string entity2LogicalName,
            string? intersectEntityName = null)
        {
            var info = new ManyToManyRelationshipInfo(schemaName, entity1LogicalName, entity2LogicalName, intersectEntityName);
            lock (_lock)
            {
                _manyToManyRelationships[schemaName] = info;
            }
        }

        /// <summary>
        /// Loads entity metadata from a DataContract-serialized XML string produced by the Dataverse
        /// SDK (e.g. a <c>RetrieveEntityResponse.EntityMetadata</c> or
        /// <c>RetrieveAllEntitiesResponse.EntityMetadata</c> array serialized with
        /// <c>DataContractSerializer</c>). The root element must be <c>&lt;EntityMetadata&gt;</c>
        /// (single entity) or <c>&lt;ArrayOfEntityMetadata&gt;</c> (multiple entities).
        /// Existing metadata for an entity is merged; attributes are upserted by logical name.
        /// </summary>
        /// <param name="xml">DataContract XML containing serialized entity metadata.</param>
        public void LoadFromXml(string xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            var entities = EntityMetadataXmlLoader.ParseXml(xml);
            foreach (var em in entities)
                ImportSdkEntityMetadata(em);
        }

        /// <summary>
        /// Loads entity metadata from a DataContract-serialized XML file produced by the Dataverse SDK.
        /// The file must contain either a single <c>&lt;EntityMetadata&gt;</c> element or an
        /// <c>&lt;ArrayOfEntityMetadata&gt;</c> root element.
        /// Existing metadata for an entity is merged; attributes are upserted by logical name.
        /// </summary>
        /// <param name="filePath">Path to the XML file.</param>
        public void LoadFromXmlFile(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            LoadFromXml(File.ReadAllText(filePath));
        }

        /// <summary>
        /// Registers a full-fidelity SDK <see cref="EntityMetadata"/> object so that
        /// <c>RetrieveEntity</c>, <c>RetrieveAllEntities</c>, and <c>RetrieveAttribute</c>
        /// requests return it verbatim — preserving option-set labels, managed properties,
        /// and all typed attribute subclasses. Also imports the structural metadata into the
        /// internal store for query-engine and validation purposes.
        /// </summary>
        /// <param name="entityMetadata">The SDK entity metadata to register.</param>
        public void RegisterSdkEntityMetadata(EntityMetadata entityMetadata)
        {
            if (entityMetadata == null) throw new ArgumentNullException(nameof(entityMetadata));
            if (string.IsNullOrEmpty(entityMetadata.LogicalName))
                throw new ArgumentException("EntityMetadata.LogicalName is required.", nameof(entityMetadata));

            lock (_lock)
            {
                _sdkMetadataCache[entityMetadata.LogicalName] = entityMetadata;
            }

            // Also import into the structural store so the query engine has attribute types etc.
            ImportSdkEntityMetadata(entityMetadata);

            // Import alternate keys (not handled by ImportSdkEntityMetadata)
            if (entityMetadata.Keys != null)
            {
                var builder = AddEntity(entityMetadata.LogicalName);
                foreach (var key in entityMetadata.Keys)
                {
                    if (!string.IsNullOrEmpty(key.LogicalName) && key.KeyAttributes?.Length > 0)
                    {
                        builder.WithAlternateKey(key.LogicalName, key.KeyAttributes);
                    }
                }
            }
        }

        internal EntityMetadata? GetSdkEntityMetadata(string logicalName)
        {
            lock (_lock)
            {
                _sdkMetadataCache.TryGetValue(logicalName, out var metadata);
                return metadata;
            }
        }

        internal EntityMetadata[] GetAllSdkEntityMetadata()
        {
            lock (_lock)
            {
                return _sdkMetadataCache.Values.ToArray();
            }
        }

        private void ImportSdkEntityMetadata(EntityMetadata em)
        {
            if (em?.LogicalName == null) return;

            lock (_lock)
            {
                if (!_entities.TryGetValue(em.LogicalName, out var entity))
                {
                    entity = new EntityMetadataInfo(em.LogicalName);
                    _entities[em.LogicalName] = entity;
                }

                if (em.SchemaName != null) entity.SchemaName = em.SchemaName;
                if (em.PrimaryIdAttribute != null) entity.PrimaryIdAttribute = em.PrimaryIdAttribute;
                if (em.PrimaryNameAttribute != null) entity.PrimaryNameAttribute = em.PrimaryNameAttribute;
                if (em.ObjectTypeCode.HasValue) entity.ObjectTypeCode = em.ObjectTypeCode.Value;

                if (em.Attributes != null)
                {
                    foreach (var attr in em.Attributes)
                    {
                        if (attr?.LogicalName == null) continue;

                        var attrType = attr.AttributeType ?? InferAttributeTypeFromSdkType(attr);
                        var info = new AttributeMetadataInfo(attr.LogicalName, attrType);

                        if (attr.RequiredLevel?.Value != null)
                            info.RequiredLevel = attr.RequiredLevel.Value;

                        switch (attr)
                        {
                            case StringAttributeMetadata s when s.MaxLength.HasValue:
                                info.MaxLength = s.MaxLength.Value;
                                break;
                            case MemoAttributeMetadata m when m.MaxLength.HasValue:
                                info.MaxLength = m.MaxLength.Value;
                                break;
                            case IntegerAttributeMetadata i:
                                if (i.MinValue.HasValue) info.MinValue = i.MinValue.Value;
                                if (i.MaxValue.HasValue) info.MaxValue = i.MaxValue.Value;
                                break;
                            case DecimalAttributeMetadata d:
                                if (d.MinValue.HasValue) info.MinValue = (double)d.MinValue.Value;
                                if (d.MaxValue.HasValue) info.MaxValue = (double)d.MaxValue.Value;
                                break;
                            case DoubleAttributeMetadata db:
                                if (db.MinValue.HasValue) info.MinValue = db.MinValue.Value;
                                if (db.MaxValue.HasValue) info.MaxValue = db.MaxValue.Value;
                                break;
                            case MoneyAttributeMetadata mo:
                                if (mo.MinValue.HasValue) info.MinValue = mo.MinValue.Value;
                                if (mo.MaxValue.HasValue) info.MaxValue = mo.MaxValue.Value;
                                break;
                        }

                        if (attr is PicklistAttributeMetadata pl && pl.OptionSet?.Options != null)
                        {
                            info.ValidOptionSetValues = new HashSet<int>(
                                pl.OptionSet.Options
                                    .Where(o => o.Value.HasValue)
                                    .Select(o => o.Value!.Value));
                        }

                        if (attr is LookupAttributeMetadata lu && lu.Targets != null)
                        {
                            info.ValidTargetEntityTypes = new HashSet<string>(
                                lu.Targets, StringComparer.OrdinalIgnoreCase);
                        }

                        entity.Attributes[attr.LogicalName] = info;
                    }
                }

                if (em.OneToManyRelationships != null)
                {
                    foreach (var rel in em.OneToManyRelationships)
                    {
                        if (rel?.SchemaName == null) continue;
                        _oneToManyRelationships[rel.SchemaName] = new OneToManyRelationshipInfo(
                            rel.SchemaName,
                            rel.ReferencedEntity ?? string.Empty,
                            rel.ReferencedAttribute ?? string.Empty,
                            rel.ReferencingEntity ?? string.Empty,
                            rel.ReferencingAttribute ?? string.Empty);
                    }
                }

                if (em.ManyToManyRelationships != null)
                {
                    foreach (var rel in em.ManyToManyRelationships)
                    {
                        if (rel?.SchemaName == null) continue;
                        _manyToManyRelationships[rel.SchemaName] = new ManyToManyRelationshipInfo(
                            rel.SchemaName,
                            rel.Entity1LogicalName ?? string.Empty,
                            rel.Entity2LogicalName ?? string.Empty,
                            rel.IntersectEntityName);
                    }
                }
            }
        }

        private static AttributeTypeCode InferAttributeTypeFromSdkType(AttributeMetadata attr)
        {
            if (attr is StringAttributeMetadata) return AttributeTypeCode.String;
            if (attr is MemoAttributeMetadata) return AttributeTypeCode.Memo;
            if (attr is IntegerAttributeMetadata) return AttributeTypeCode.Integer;
            if (attr is DecimalAttributeMetadata) return AttributeTypeCode.Decimal;
            if (attr is DoubleAttributeMetadata) return AttributeTypeCode.Double;
            if (attr is MoneyAttributeMetadata) return AttributeTypeCode.Money;
            if (attr is PicklistAttributeMetadata) return AttributeTypeCode.Picklist;
            if (attr is LookupAttributeMetadata) return AttributeTypeCode.Lookup;
            if (attr is BooleanAttributeMetadata) return AttributeTypeCode.Boolean;
            if (attr is DateTimeAttributeMetadata) return AttributeTypeCode.DateTime;
            if (attr is BigIntAttributeMetadata) return AttributeTypeCode.BigInt;
            if (attr is UniqueIdentifierAttributeMetadata) return AttributeTypeCode.Uniqueidentifier;
            if (attr is StatusAttributeMetadata) return AttributeTypeCode.Status;
            if (attr is StateAttributeMetadata) return AttributeTypeCode.State;
            return AttributeTypeCode.String;
        }

        internal EntityMetadataInfo? GetEntityMetadataInfo(string logicalName)
        {
            lock (_lock)
            {
                _entities.TryGetValue(logicalName, out var entity);
                return entity;
            }
        }

        internal IReadOnlyList<EntityMetadataInfo> GetAllEntityMetadataInfo()
        {
            lock (_lock)
            {
                return _entities.Values.ToList();
            }
        }

        /// <summary>
        /// Gets all 1:N relationships where the specified entity is the parent (referenced entity).
        /// </summary>
        internal IReadOnlyList<OneToManyRelationshipInfo> GetChildRelationships(string parentEntity)
        {
            lock (_lock)
            {
                return _oneToManyRelationships.Values
                    .Where(r => string.Equals(r.ReferencedEntity, parentEntity, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        internal IReadOnlyList<OneToManyRelationshipInfo> GetOneToManyRelationships(string? entityLogicalName = null)
        {
            lock (_lock)
            {
                if (entityLogicalName == null)
                    return _oneToManyRelationships.Values.ToList();

                return _oneToManyRelationships.Values
                    .Where(r => string.Equals(r.ReferencedEntity, entityLogicalName, StringComparison.OrdinalIgnoreCase)
                             || string.Equals(r.ReferencingEntity, entityLogicalName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        internal IReadOnlyList<ManyToManyRelationshipInfo> GetManyToManyRelationships(string? entityLogicalName = null)
        {
            lock (_lock)
            {
                if (entityLogicalName == null)
                    return _manyToManyRelationships.Values.ToList();

                return _manyToManyRelationships.Values
                    .Where(r => string.Equals(r.Entity1LogicalName, entityLogicalName, StringComparison.OrdinalIgnoreCase)
                             || string.Equals(r.Entity2LogicalName, entityLogicalName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        internal void ValidateOnCreate(Entity entity)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(entity.LogicalName, out var entityMeta))
                    return;

                // Check required fields are present and non-null.
                foreach (var attrMeta in entityMeta.Attributes.Values)
                {
                    if (attrMeta.RequiredLevel == AttributeRequiredLevel.SystemRequired
                        || attrMeta.RequiredLevel == AttributeRequiredLevel.ApplicationRequired)
                    {
                        if (!entity.Contains(attrMeta.LogicalName) || entity[attrMeta.LogicalName] == null)
                        {
                            throw DataverseFault.Create(
                                DataverseFault.InvalidArgument,
                                $"Required attribute '{attrMeta.LogicalName}' is missing on create of '{entity.LogicalName}'.");
                        }
                    }
                }

                // Validate constraints on provided attributes.
                ValidateAttributeConstraints(entity, entityMeta);
            }
        }

        internal void ValidateOnUpdate(Entity entity)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(entity.LogicalName, out var entityMeta))
                    return;

                // On update, if a required field is being set to null, reject it.
                foreach (var attr in entity.Attributes)
                {
                    if (entityMeta.Attributes.TryGetValue(attr.Key, out var attrMeta))
                    {
                        if ((attrMeta.RequiredLevel == AttributeRequiredLevel.SystemRequired
                             || attrMeta.RequiredLevel == AttributeRequiredLevel.ApplicationRequired)
                            && attr.Value == null)
                        {
                            throw DataverseFault.Create(
                                DataverseFault.InvalidArgument,
                                $"Required attribute '{attrMeta.LogicalName}' cannot be set to null on update of '{entity.LogicalName}'.");
                        }
                    }
                }

                ValidateAttributeConstraints(entity, entityMeta);
            }
        }

        internal void AutoDiscover(Entity entity)
        {
            if (!AutoDiscoverMetadata) return;

            lock (_lock)
            {
                if (!_entities.TryGetValue(entity.LogicalName, out var entityMeta))
                {
                    entityMeta = new EntityMetadataInfo(entity.LogicalName);
                    _entities[entity.LogicalName] = entityMeta;
                }

                foreach (var attr in entity.Attributes)
                {
                    if (entityMeta.Attributes.ContainsKey(attr.Key)) continue;
                    if (attr.Value == null) continue;

                    var inferred = InferAttributeType(attr.Value);
                    if (inferred.HasValue)
                    {
                        entityMeta.Attributes[attr.Key] = new AttributeMetadataInfo(attr.Key, inferred.Value);
                    }
                }
            }
        }

        internal void ValidateRelationship(
            string entityName,
            Relationship relationship,
            EntityReferenceCollection relatedEntities)
        {
            lock (_lock)
            {
                // Check one-to-many relationships.
                if (_oneToManyRelationships.TryGetValue(relationship.SchemaName, out var otm))
                {
                    foreach (var related in relatedEntities)
                    {
                        bool valid =
                            (string.Equals(otm.ReferencedEntity, entityName, StringComparison.OrdinalIgnoreCase)
                             && string.Equals(otm.ReferencingEntity, related.LogicalName, StringComparison.OrdinalIgnoreCase))
                            ||
                            (string.Equals(otm.ReferencingEntity, entityName, StringComparison.OrdinalIgnoreCase)
                             && string.Equals(otm.ReferencedEntity, related.LogicalName, StringComparison.OrdinalIgnoreCase));

                        if (!valid)
                        {
                            throw DataverseFault.Create(
                                DataverseFault.InvalidArgument,
                                $"Entity '{related.LogicalName}' is not valid for relationship '{relationship.SchemaName}' with entity '{entityName}'.");
                        }
                    }
                    return;
                }

                // Check many-to-many relationships.
                if (_manyToManyRelationships.TryGetValue(relationship.SchemaName, out var mtm))
                {
                    foreach (var related in relatedEntities)
                    {
                        bool valid =
                            (string.Equals(mtm.Entity1LogicalName, entityName, StringComparison.OrdinalIgnoreCase)
                             && string.Equals(mtm.Entity2LogicalName, related.LogicalName, StringComparison.OrdinalIgnoreCase))
                            ||
                            (string.Equals(mtm.Entity2LogicalName, entityName, StringComparison.OrdinalIgnoreCase)
                             && string.Equals(mtm.Entity1LogicalName, related.LogicalName, StringComparison.OrdinalIgnoreCase));

                        if (!valid)
                        {
                            throw DataverseFault.Create(
                                DataverseFault.InvalidArgument,
                                $"Entity '{related.LogicalName}' is not valid for relationship '{relationship.SchemaName}' with entity '{entityName}'.");
                        }
                    }
                }

                // If the relationship schema name is not defined in metadata, skip validation.
            }
        }

        private static void ValidateAttributeConstraints(Entity entity, EntityMetadataInfo entityMeta)
        {
            foreach (var attr in entity.Attributes)
            {
                if (attr.Value == null) continue;
                if (!entityMeta.Attributes.TryGetValue(attr.Key, out var attrMeta)) continue;

                switch (attrMeta.AttributeType)
                {
                    case AttributeTypeCode.String:
                    case AttributeTypeCode.Memo:
                        ValidateStringLength(attr.Value, attrMeta, entity.LogicalName);
                        break;

                    case AttributeTypeCode.Integer:
                        ValidateNumericRange(Convert.ToDouble(attr.Value), attrMeta, entity.LogicalName);
                        break;

                    case AttributeTypeCode.Decimal:
                        if (attr.Value is decimal decVal)
                            ValidateNumericRange((double)decVal, attrMeta, entity.LogicalName);
                        break;

                    case AttributeTypeCode.Double:
                        if (attr.Value is double dblVal)
                            ValidateNumericRange(dblVal, attrMeta, entity.LogicalName);
                        break;

                    case AttributeTypeCode.Money:
                        if (attr.Value is Money moneyVal)
                            ValidateNumericRange((double)moneyVal.Value, attrMeta, entity.LogicalName);
                        break;

                    case AttributeTypeCode.Picklist:
                        ValidateOptionSetValue(attr.Value, attrMeta, entity.LogicalName);
                        break;

                    case AttributeTypeCode.Lookup:
                    case AttributeTypeCode.Customer:
                    case AttributeTypeCode.Owner:
                        ValidateEntityReferenceTarget(attr.Value, attrMeta, entity.LogicalName);
                        break;
                }
            }
        }

        private static void ValidateStringLength(object value, AttributeMetadataInfo attrMeta, string entityName)
        {
            if (!attrMeta.MaxLength.HasValue) return;
            if (value is string str && str.Length > attrMeta.MaxLength.Value)
            {
                throw DataverseFault.Create(
                    DataverseFault.InvalidArgument,
                    $"Attribute '{attrMeta.LogicalName}' on '{entityName}' exceeds maximum length of {attrMeta.MaxLength.Value}. Actual length: {str.Length}.");
            }
        }

        private static void ValidateNumericRange(double numericValue, AttributeMetadataInfo attrMeta, string entityName)
        {
            if (attrMeta.MinValue.HasValue && numericValue < attrMeta.MinValue.Value)
            {
                throw DataverseFault.Create(
                    DataverseFault.InvalidArgument,
                    $"Attribute '{attrMeta.LogicalName}' on '{entityName}' value {numericValue} is below minimum {attrMeta.MinValue.Value}.");
            }
            if (attrMeta.MaxValue.HasValue && numericValue > attrMeta.MaxValue.Value)
            {
                throw DataverseFault.Create(
                    DataverseFault.InvalidArgument,
                    $"Attribute '{attrMeta.LogicalName}' on '{entityName}' value {numericValue} is above maximum {attrMeta.MaxValue.Value}.");
            }
        }

        private static void ValidateOptionSetValue(object value, AttributeMetadataInfo attrMeta, string entityName)
        {
            if (attrMeta.ValidOptionSetValues == null) return;
            if (value is OptionSetValue osv && !attrMeta.ValidOptionSetValues.Contains(osv.Value))
            {
                throw DataverseFault.Create(
                    DataverseFault.InvalidArgument,
                    $"Attribute '{attrMeta.LogicalName}' on '{entityName}' has invalid option set value {osv.Value}.");
            }
        }

        private static void ValidateEntityReferenceTarget(object value, AttributeMetadataInfo attrMeta, string entityName)
        {
            if (attrMeta.ValidTargetEntityTypes == null) return;
            if (value is EntityReference er && !attrMeta.ValidTargetEntityTypes.Contains(er.LogicalName))
            {
                throw DataverseFault.Create(
                    DataverseFault.InvalidArgument,
                    $"Attribute '{attrMeta.LogicalName}' on '{entityName}' does not accept entity type '{er.LogicalName}'.");
            }
        }

        private static AttributeTypeCode? InferAttributeType(object value)
        {
            switch (value)
            {
                case string _: return AttributeTypeCode.String;
                case int _: return AttributeTypeCode.Integer;
                case decimal _: return AttributeTypeCode.Decimal;
                case double _: return AttributeTypeCode.Double;
                case float _: return AttributeTypeCode.Double;
                case bool _: return AttributeTypeCode.Boolean;
                case DateTime _: return AttributeTypeCode.DateTime;
                case Guid _: return AttributeTypeCode.Uniqueidentifier;
                case Money _: return AttributeTypeCode.Money;
                case OptionSetValue _: return AttributeTypeCode.Picklist;
                case EntityReference _: return AttributeTypeCode.Lookup;
                case EntityCollection _: return AttributeTypeCode.PartyList;
                default: return null;
            }
        }

        // ── Internal mutation methods for metadata request handlers ──

        internal void CreateEntityMetadata(EntityMetadataInfo entity)
        {
            lock (_lock)
            {
                if (_entities.ContainsKey(entity.LogicalName))
                    throw DataverseFault.Create(DataverseFault.DuplicateRecord,
                        $"Entity '{entity.LogicalName}' metadata already exists.");
                _entities[entity.LogicalName] = entity;
            }
        }

        internal void UpdateEntityMetadata(EntityMetadataInfo entity)
        {
            lock (_lock)
            {
                if (!_entities.ContainsKey(entity.LogicalName))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Entity '{entity.LogicalName}' metadata does not exist.");
                // Merge: update mutable properties but keep existing attributes/keys
                var existing = _entities[entity.LogicalName];
                if (entity.SchemaName != null) existing.SchemaName = entity.SchemaName;
                if (entity.PrimaryIdAttribute != null) existing.PrimaryIdAttribute = entity.PrimaryIdAttribute;
                if (entity.PrimaryNameAttribute != null) existing.PrimaryNameAttribute = entity.PrimaryNameAttribute;
                if (entity.ObjectTypeCode.HasValue) existing.ObjectTypeCode = entity.ObjectTypeCode;
            }
        }

        internal void DeleteEntityMetadata(string logicalName)
        {
            lock (_lock)
            {
                if (!_entities.Remove(logicalName))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Entity '{logicalName}' metadata does not exist.");
            }
        }

        internal void CreateAttributeMetadata(string entityLogicalName, AttributeMetadataInfo attribute)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(entityLogicalName, out var entity))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Entity '{entityLogicalName}' metadata does not exist.");
                if (entity.Attributes.ContainsKey(attribute.LogicalName))
                    throw DataverseFault.Create(DataverseFault.DuplicateRecord,
                        $"Attribute '{attribute.LogicalName}' already exists on entity '{entityLogicalName}'.");
                entity.Attributes[attribute.LogicalName] = attribute;
            }
        }

        internal void UpdateAttributeMetadata(string entityLogicalName, AttributeMetadataInfo attribute)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(entityLogicalName, out var entity))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Entity '{entityLogicalName}' metadata does not exist.");
                if (!entity.Attributes.ContainsKey(attribute.LogicalName))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Attribute '{attribute.LogicalName}' does not exist on entity '{entityLogicalName}'.");
                entity.Attributes[attribute.LogicalName] = attribute;
            }
        }

        internal void DeleteAttributeMetadata(string entityLogicalName, string attributeLogicalName)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(entityLogicalName, out var entity))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Entity '{entityLogicalName}' metadata does not exist.");
                if (!entity.Attributes.Remove(attributeLogicalName))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Attribute '{attributeLogicalName}' does not exist on entity '{entityLogicalName}'.");
            }
        }

        internal OneToManyRelationshipInfo? GetOneToManyRelationship(string schemaName)
        {
            lock (_lock)
            {
                _oneToManyRelationships.TryGetValue(schemaName, out var rel);
                return rel;
            }
        }

        internal ManyToManyRelationshipInfo? GetManyToManyRelationship(string schemaName)
        {
            lock (_lock)
            {
                _manyToManyRelationships.TryGetValue(schemaName, out var rel);
                return rel;
            }
        }

        internal void CreateOneToManyRelationshipInternal(OneToManyRelationshipInfo rel)
        {
            lock (_lock)
            {
                if (_oneToManyRelationships.ContainsKey(rel.SchemaName))
                    throw DataverseFault.Create(DataverseFault.DuplicateRecord,
                        $"Relationship '{rel.SchemaName}' already exists.");
                _oneToManyRelationships[rel.SchemaName] = rel;
            }
        }

        internal void CreateManyToManyRelationshipInternal(ManyToManyRelationshipInfo rel)
        {
            lock (_lock)
            {
                if (_manyToManyRelationships.ContainsKey(rel.SchemaName))
                    throw DataverseFault.Create(DataverseFault.DuplicateRecord,
                        $"Relationship '{rel.SchemaName}' already exists.");
                _manyToManyRelationships[rel.SchemaName] = rel;
            }
        }

        internal void UpdateOneToManyRelationship(OneToManyRelationshipInfo rel)
        {
            lock (_lock)
            {
                if (!_oneToManyRelationships.ContainsKey(rel.SchemaName))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Relationship '{rel.SchemaName}' does not exist.");
                _oneToManyRelationships[rel.SchemaName] = rel;
            }
        }

        internal void UpdateManyToManyRelationship(ManyToManyRelationshipInfo rel)
        {
            lock (_lock)
            {
                if (!_manyToManyRelationships.ContainsKey(rel.SchemaName))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Relationship '{rel.SchemaName}' does not exist.");
                _manyToManyRelationships[rel.SchemaName] = rel;
            }
        }

        internal void DeleteRelationshipInternal(string schemaName)
        {
            lock (_lock)
            {
                if (!_oneToManyRelationships.Remove(schemaName) && !_manyToManyRelationships.Remove(schemaName))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Relationship '{schemaName}' does not exist.");
            }
        }

        internal AlternateKeyInfo? GetAlternateKey(string entityLogicalName, string keyName)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(entityLogicalName, out var entity))
                    return null;
                return entity.AlternateKeys.Find(k => string.Equals(k.Name, keyName, StringComparison.OrdinalIgnoreCase));
            }
        }

        internal void CreateAlternateKey(string entityLogicalName, AlternateKeyInfo key)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(entityLogicalName, out var entity))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Entity '{entityLogicalName}' metadata does not exist.");
                if (entity.AlternateKeys.Exists(k => string.Equals(k.Name, key.Name, StringComparison.OrdinalIgnoreCase)))
                    throw DataverseFault.Create(DataverseFault.DuplicateRecord,
                        $"Alternate key '{key.Name}' already exists on entity '{entityLogicalName}'.");
                entity.AlternateKeys.Add(key);
            }
        }

        internal void DeleteAlternateKey(string entityLogicalName, string keyName)
        {
            lock (_lock)
            {
                if (!_entities.TryGetValue(entityLogicalName, out var entity))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Entity '{entityLogicalName}' metadata does not exist.");
                var removed = entity.AlternateKeys.RemoveAll(
                    k => string.Equals(k.Name, keyName, StringComparison.OrdinalIgnoreCase));
                if (removed == 0)
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Alternate key '{keyName}' does not exist on entity '{entityLogicalName}'.");
            }
        }

        private readonly Dictionary<string, GlobalOptionSetInfo> _globalOptionSets =
            new Dictionary<string, GlobalOptionSetInfo>(StringComparer.OrdinalIgnoreCase);

        internal void CreateGlobalOptionSet(GlobalOptionSetInfo optionSet)
        {
            lock (_lock)
            {
                if (_globalOptionSets.ContainsKey(optionSet.Name))
                    throw DataverseFault.Create(DataverseFault.DuplicateRecord,
                        $"Global option set '{optionSet.Name}' already exists.");
                _globalOptionSets[optionSet.Name] = optionSet;
            }
        }

        internal GlobalOptionSetInfo? GetGlobalOptionSet(string name)
        {
            lock (_lock)
            {
                _globalOptionSets.TryGetValue(name, out var os);
                return os;
            }
        }

        internal IReadOnlyList<GlobalOptionSetInfo> GetAllGlobalOptionSets()
        {
            lock (_lock)
            {
                return _globalOptionSets.Values.ToList();
            }
        }

        internal void UpdateGlobalOptionSet(GlobalOptionSetInfo optionSet)
        {
            lock (_lock)
            {
                if (!_globalOptionSets.ContainsKey(optionSet.Name))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Global option set '{optionSet.Name}' does not exist.");
                _globalOptionSets[optionSet.Name] = optionSet;
            }
        }

        internal void DeleteGlobalOptionSet(string name)
        {
            lock (_lock)
            {
                if (!_globalOptionSets.Remove(name))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Global option set '{name}' does not exist.");
            }
        }

        private long _metadataTimestamp = 1;

        internal long GetMetadataTimestamp()
        {
            lock (_lock)
            {
                return _metadataTimestamp;
            }
        }

        internal void IncrementMetadataTimestamp()
        {
            lock (_lock)
            {
                _metadataTimestamp++;
            }
        }
    }
}
