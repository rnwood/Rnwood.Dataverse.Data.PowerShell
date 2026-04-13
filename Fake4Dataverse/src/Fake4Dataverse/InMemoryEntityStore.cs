using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse
{
    /// <summary>
    /// Thread-safe in-memory store for Dataverse entities.
    /// Uses <see cref="ReaderWriterLockSlim"/> for the outer entity-type mapping and
    /// <see cref="ConcurrentDictionary{TKey,TValue}"/> for per-entity-type tables,
    /// allowing concurrent reads and writes to different entity types.
    /// </summary>
    internal sealed class InMemoryEntityStore : IDisposable
    {
        private readonly Dictionary<string, ConcurrentDictionary<Guid, Entity>> _store = new Dictionary<string, ConcurrentDictionary<Guid, Entity>>(StringComparer.OrdinalIgnoreCase);
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly AsyncLocal<TransactionCopyOnWriteState?> _activeTransaction = new AsyncLocal<TransactionCopyOnWriteState?>();

        internal AttributeIndex? Index { get; set; }

        /// <summary>
        /// Gets or sets the active copy-on-write transaction context.
        /// When set, mutations are staged in a transaction-local buffer and only applied on commit.
        /// </summary>
        internal TransactionCopyOnWriteState? ActiveTransaction
        {
            get => _activeTransaction.Value;
            set => _activeTransaction.Value = value;
        }

        internal bool HasActiveTransaction => _activeTransaction.Value != null;

        public Guid Create(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.LogicalName)) throw new ArgumentException("Entity logical name must be set.", nameof(entity));

            var id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;

            entity.Id = id;

            var clone = CloneEntity(entity);
            clone.Id = id;
            clone[entity.LogicalName + "id"] = id;

            var transaction = _activeTransaction.Value;
            if (transaction != null)
            {
                if (TryGetEffectiveEntity(entity.LogicalName, id, transaction, out var existing) && existing != null)
                    throw DataverseFault.DuplicateId(entity.LogicalName, id);

                transaction.StageUpsert(clone, clone.Attributes.Keys);
                return id;
            }

            var table = GetOrCreateTable(entity.LogicalName);
            if (!table.TryAdd(id, clone))
                throw DataverseFault.DuplicateId(entity.LogicalName, id);

            Index?.OnCreate(clone);
            return id;
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet? columnSet)
        {
            if (string.IsNullOrEmpty(entityName)) throw new ArgumentException("Entity name is required.", nameof(entityName));

            var transaction = _activeTransaction.Value;
            if (transaction != null)
            {
                if (TryGetEffectiveEntity(entityName, id, transaction, out var stagedEntity) && stagedEntity != null)
                    return ProjectEntity(stagedEntity, columnSet);

                throw DataverseFault.EntityNotFound(entityName, id);
            }

            _lock.EnterReadLock();
            try
            {
                if (!_store.TryGetValue(entityName, out var table) || !table.TryGetValue(id, out var entity))
                    throw DataverseFault.EntityNotFound(entityName, id);

                return ProjectEntity(entity, columnSet);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IReadOnlyList<Entity> GetAll(string entityName)
        {
            var transaction = _activeTransaction.Value;
            if (transaction != null)
                return BuildEffectiveTable(entityName, transaction).Values.Select(CloneEntity).ToList();

            _lock.EnterReadLock();
            try
            {
                if (!_store.TryGetValue(entityName, out var table))
                    return Array.Empty<Entity>();

                return table.Values.Select(CloneEntity).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Update(Entity entity)
        {
            Update(entity, null);
        }

        public void Update(Entity entity, long? expectedVersion)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.LogicalName)) throw new ArgumentException("Entity logical name must be set.", nameof(entity));
            if (entity.Id == Guid.Empty) throw new ArgumentException("Entity id must be set for update.", nameof(entity));

            var transaction = _activeTransaction.Value;
            if (transaction != null)
            {
                if (!TryGetEffectiveEntity(entity.LogicalName, entity.Id, transaction, out var existing) || existing == null)
                    throw DataverseFault.EntityNotFound(entity.LogicalName, entity.Id);

                if (expectedVersion.HasValue)
                {
                    var storedVersion = existing.Contains("versionnumber") ? (long)existing["versionnumber"] : 0L;
                    if (storedVersion != expectedVersion.Value)
                        throw DataverseFault.ConcurrencyVersionMismatchFault(entity.LogicalName, entity.Id);
                }

                foreach (var attr in entity.Attributes)
                {
                    if (attr.Value == null)
                    {
                        existing.Attributes.Remove(attr.Key);
                    }
                    else
                    {
                        existing[attr.Key] = CloneAttributeValue(attr.Value);
                    }
                }

                var touchedAttributes = entity.Attributes.Keys;
                var clearedAttributes = entity.Attributes.Where(a => a.Value == null).Select(a => a.Key);
                transaction.StageUpsert(existing, touchedAttributes, clearedAttributes);
                return;
            }

            _lock.EnterWriteLock();
            try
            {
                if (!_store.TryGetValue(entity.LogicalName, out var table) || !table.TryGetValue(entity.Id, out var existing))
                    throw DataverseFault.EntityNotFound(entity.LogicalName, entity.Id);

                if (expectedVersion.HasValue)
                {
                    var storedVersion = existing.Contains("versionnumber") ? (long)existing["versionnumber"] : 0L;
                    if (storedVersion != expectedVersion.Value)
                        throw DataverseFault.ConcurrencyVersionMismatchFault(entity.LogicalName, entity.Id);
                }

                Index?.OnUpdate(entity, existing);

                foreach (var attr in entity.Attributes)
                {
                    if (attr.Value == null)
                    {
                        // In Dataverse, updating an attribute to null removes it from the record
                        existing.Attributes.Remove(attr.Key);
                    }
                    else
                    {
                        existing[attr.Key] = CloneAttributeValue(attr.Value);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Delete(string entityName, Guid id)
        {
            Delete(entityName, id, null);
        }

        public void Delete(string entityName, Guid id, long? expectedVersion)
        {
            if (string.IsNullOrEmpty(entityName)) throw new ArgumentException("Entity name is required.", nameof(entityName));

            var transaction = _activeTransaction.Value;
            if (transaction != null)
            {
                if (!TryGetEffectiveEntity(entityName, id, transaction, out var existing) || existing == null)
                    throw DataverseFault.EntityNotFound(entityName, id);

                if (expectedVersion.HasValue)
                {
                    var storedVersion = existing.Contains("versionnumber") ? (long)existing["versionnumber"] : 0L;
                    if (storedVersion != expectedVersion.Value)
                        throw DataverseFault.ConcurrencyVersionMismatchFault(entityName, id);
                }

                transaction.StageDelete(entityName, id);
                return;
            }

            _lock.EnterReadLock();
            try
            {
                if (!_store.TryGetValue(entityName, out var table))
                    throw DataverseFault.EntityNotFound(entityName, id);

                if (expectedVersion.HasValue)
                {
                    if (!table.TryGetValue(id, out var existing))
                        throw DataverseFault.EntityNotFound(entityName, id);

                    var storedVersion = existing.Contains("versionnumber") ? (long)existing["versionnumber"] : 0L;
                    if (storedVersion != expectedVersion.Value)
                        throw DataverseFault.ConcurrencyVersionMismatchFault(entityName, id);
                }

                if (!table.TryRemove(id, out var removed))
                    throw DataverseFault.EntityNotFound(entityName, id);

                Index?.OnDelete(entityName, id, removed);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Atomically finds and removes association records matching the specified criteria.
        /// </summary>
        public void RemoveAssociations(string associationEntity, string sourceEntityName, Guid sourceId, EntityReferenceCollection targets)
        {
            var transaction = _activeTransaction.Value;
            if (transaction != null)
            {
                var effectiveAssociations = BuildEffectiveTable(associationEntity, transaction);
                foreach (var target in targets)
                {
                    Guid? toRemove = null;
                    foreach (var kvp in effectiveAssociations)
                    {
                        var source = kvp.Value.GetAttributeValue<EntityReference>("sourceid");
                        var t = kvp.Value.GetAttributeValue<EntityReference>("targetid");
                        if (source != null && source.Id == sourceId && source.LogicalName == sourceEntityName
                            && t != null && t.Id == target.Id && t.LogicalName == target.LogicalName)
                        {
                            toRemove = kvp.Key;
                            break;
                        }
                    }

                    if (toRemove.HasValue)
                    {
                        transaction.StageDelete(associationEntity, toRemove.Value);
                        effectiveAssociations.Remove(toRemove.Value);
                    }
                }

                return;
            }

            _lock.EnterReadLock();
            try
            {
                if (!_store.TryGetValue(associationEntity, out var table))
                    return;

                foreach (var target in targets)
                {
                    Guid? toRemove = null;
                    foreach (var kvp in table)
                    {
                        var source = kvp.Value.GetAttributeValue<EntityReference>("sourceid");
                        var t = kvp.Value.GetAttributeValue<EntityReference>("targetid");
                        if (source != null && source.Id == sourceId && source.LogicalName == sourceEntityName
                            && t != null && t.Id == target.Id && t.LogicalName == target.LogicalName)
                        {
                            toRemove = kvp.Key;
                            break;
                        }
                    }
                    if (toRemove.HasValue)
                    {
                        if (table.TryRemove(toRemove.Value, out var removed))
                        {
                            Index?.OnDelete(associationEntity, toRemove.Value, removed);
                        }
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Exists(string entityName, Guid id)
        {
            var transaction = _activeTransaction.Value;
            if (transaction != null)
                return TryGetEffectiveEntity(entityName, id, transaction, out var existing) && existing != null;

            _lock.EnterReadLock();
            try
            {
                return _store.TryGetValue(entityName, out var table) && table.ContainsKey(id);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _store.Clear();
                Index?.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            _lock.Dispose();
        }

        internal void CommitTransaction(TransactionCopyOnWriteState transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            _lock.EnterWriteLock();
            try
            {
                foreach (var entityChanges in transaction.GetAllEntityChanges())
                {
                    var entityName = entityChanges.Key;
                    var changes = entityChanges.Value;

                    if (!_store.TryGetValue(entityName, out var table))
                    {
                        bool hasUpserts = changes.Values.Any(v => v != null);
                        if (!hasUpserts)
                            continue;

                        table = new ConcurrentDictionary<Guid, Entity>();
                        _store[entityName] = table;
                    }

                    foreach (var change in changes)
                    {
                        var id = change.Key;
                        var stagedEntity = change.Value.Entity;

                        if (stagedEntity == null)
                        {
                            if (table.TryRemove(id, out var removed))
                                Index?.OnDelete(entityName, id, removed);
                            continue;
                        }

                        var clone = CloneEntity(stagedEntity);
                        if (table.TryGetValue(id, out var existing))
                        {
                            var merged = CloneEntity(existing);

                            foreach (var touchedAttribute in change.Value.TouchedAttributes)
                            {
                                if (change.Value.ClearedAttributes.Contains(touchedAttribute))
                                {
                                    merged.Attributes.Remove(touchedAttribute);
                                    continue;
                                }

                                if (clone.Contains(touchedAttribute))
                                    merged[touchedAttribute] = CloneAttributeValue(clone[touchedAttribute]);
                            }

                            var indexUpdate = new Entity(entityName, id);
                            foreach (var touchedAttribute in change.Value.TouchedAttributes)
                            {
                                if (change.Value.ClearedAttributes.Contains(touchedAttribute))
                                {
                                    indexUpdate[touchedAttribute] = null;
                                    continue;
                                }

                                if (merged.Contains(touchedAttribute))
                                    indexUpdate[touchedAttribute] = CloneAttributeValue(merged[touchedAttribute]);
                            }

                            Index?.OnUpdate(indexUpdate, existing);
                            table[id] = merged;
                        }
                        else
                        {
                            Index?.OnCreate(clone);
                            table[id] = clone;
                        }
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Retrieves an entity by alternate key attributes.
        /// </summary>
        public Entity RetrieveByAlternateKey(string entityName, KeyAttributeCollection keyAttributes, ColumnSet? columnSet, InMemoryMetadataStore metadataStore)
        {
            var id = FindByAlternateKey(entityName, keyAttributes, metadataStore);
            return Retrieve(entityName, id, columnSet);
        }

        /// <summary>
        /// Finds the ID of an entity matching the given alternate key attributes.
        /// </summary>
        public Guid FindByAlternateKey(string entityName, KeyAttributeCollection keyAttributes, InMemoryMetadataStore metadataStore)
        {
            if (keyAttributes == null || keyAttributes.Count == 0)
                throw DataverseFault.InvalidArgumentFault("KeyAttributes must be provided for alternate key lookup.");

            var transaction = _activeTransaction.Value;
            if (transaction != null)
            {
                var effective = BuildEffectiveTable(entityName, transaction);
                foreach (var entity in effective.Values)
                {
                    bool match = true;
                    foreach (var keyAttr in keyAttributes)
                    {
                        if (!entity.Contains(keyAttr.Key) || !Equals(entity[keyAttr.Key], keyAttr.Value))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match) return entity.Id;
                }

                throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                    $"Entity '{entityName}' with the specified alternate key values does not exist.");
            }

            _lock.EnterReadLock();
            try
            {
                if (!_store.TryGetValue(entityName, out var table))
                    throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                        $"Entity '{entityName}' with the specified alternate key values does not exist.");

                foreach (var entity in table.Values)
                {
                    bool match = true;
                    foreach (var keyAttr in keyAttributes)
                    {
                        if (!entity.Contains(keyAttr.Key) || !Equals(entity[keyAttr.Key], keyAttr.Value))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match) return entity.Id;
                }

                throw DataverseFault.Create(DataverseFault.ObjectDoesNotExist,
                    $"Entity '{entityName}' with the specified alternate key values does not exist.");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        internal static object? CloneAttributeValue(object? value)
        {
            if (value == null)
                return null;

            switch (value)
            {
                case Entity entity:
                    return CloneEntity(entity);

                case EntityReference entityReference:
                    var entityReferenceClone = new EntityReference(entityReference.LogicalName, entityReference.Id)
                    {
                        Name = entityReference.Name,
                        RowVersion = entityReference.RowVersion
                    };

                    foreach (var keyAttribute in entityReference.KeyAttributes)
                        entityReferenceClone.KeyAttributes[keyAttribute.Key] = CloneAttributeValue(keyAttribute.Value);

                    return entityReferenceClone;

                case Money money:
                    return new Money(money.Value);

                case OptionSetValue optionSetValue:
                    return new OptionSetValue(optionSetValue.Value);

                case OptionSetValueCollection optionSetValues:
                    var optionSetValuesClone = new OptionSetValueCollection();
                    foreach (var option in optionSetValues)
                        optionSetValuesClone.Add(new OptionSetValue(option.Value));
                    return optionSetValuesClone;

                case EntityReferenceCollection entityReferences:
                    var entityReferenceCollectionClone = new EntityReferenceCollection();
                    foreach (var entityReferenceItem in entityReferences)
                        entityReferenceCollectionClone.Add((EntityReference)CloneAttributeValue(entityReferenceItem)!);
                    return entityReferenceCollectionClone;

                case EntityCollection entityCollection:
                    var entityCollectionClone = new EntityCollection();
                    foreach (var entityItem in entityCollection.Entities)
                        entityCollectionClone.Entities.Add(CloneEntity(entityItem));
                    entityCollectionClone.EntityName = entityCollection.EntityName;
                    entityCollectionClone.MoreRecords = entityCollection.MoreRecords;
                    entityCollectionClone.PagingCookie = entityCollection.PagingCookie;
                    entityCollectionClone.TotalRecordCount = entityCollection.TotalRecordCount;
                    return entityCollectionClone;

                case BooleanManagedProperty booleanManagedProperty:
                    return new BooleanManagedProperty(booleanManagedProperty.Value);

                case AliasedValue aliasedValue:
                    return new AliasedValue(
                        aliasedValue.EntityLogicalName,
                        aliasedValue.AttributeLogicalName,
                        CloneAttributeValue(aliasedValue.Value));

                case KeyAttributeCollection keyAttributes:
                    var keyAttributesClone = new KeyAttributeCollection();
                    foreach (var keyAttribute in keyAttributes)
                        keyAttributesClone[keyAttribute.Key] = CloneAttributeValue(keyAttribute.Value);
                    return keyAttributesClone;

                case byte[] bytes:
                    return (byte[])bytes.Clone();

                case Label label:
                    var labelClone = new Label();
                    if (label.UserLocalizedLabel != null)
                        labelClone.UserLocalizedLabel = new LocalizedLabel(label.UserLocalizedLabel.Label, label.UserLocalizedLabel.LanguageCode);
                    foreach (var ll in label.LocalizedLabels)
                        labelClone.LocalizedLabels.Add(new LocalizedLabel(ll.Label, ll.LanguageCode));
                    return labelClone;

                case LocalizedLabel localizedLabel:
                    return new LocalizedLabel(localizedLabel.Label, localizedLabel.LanguageCode);

                case Array array when array.Rank == 1:
                    var elementType = array.GetType().GetElementType() ?? typeof(object);
                    var arrayClone = Array.CreateInstance(elementType, array.Length);
                    for (var i = 0; i < array.Length; i++)
                        arrayClone.SetValue(CloneAttributeValue(array.GetValue(i)), i);
                    return arrayClone;

                default:
                    return value;
            }
        }

        internal static Entity CloneEntity(Entity source)
        {
            var clone = new Entity(source.LogicalName, source.Id);
            foreach (var attr in source.Attributes)
            {
                clone[attr.Key] = CloneAttributeValue(attr.Value);
            }
            foreach (var fv in source.FormattedValues)
            {
                clone.FormattedValues[fv.Key] = fv.Value;
            }
            if (source.KeyAttributes != null && source.KeyAttributes.Count > 0)
            {
                foreach (var ka in source.KeyAttributes)
                {
                    clone.KeyAttributes[ka.Key] = CloneAttributeValue(ka.Value);
                }
            }
            return clone;
        }

        internal Dictionary<string, Dictionary<Guid, Entity>> TakeSnapshot()
        {
            _lock.EnterReadLock();
            try
            {
                var snapshot = new Dictionary<string, Dictionary<Guid, Entity>>(StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in _store)
                {
                    var table = new Dictionary<Guid, Entity>();
                    foreach (var entry in kvp.Value)
                    {
                        table[entry.Key] = CloneEntity(entry.Value);
                    }
                    snapshot[kvp.Key] = table;
                }
                return snapshot;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        internal void RestoreSnapshot(Dictionary<string, Dictionary<Guid, Entity>> snapshot)
        {
            _lock.EnterWriteLock();
            try
            {
                _store.Clear();
                foreach (var kvp in snapshot)
                {
                    var table = new ConcurrentDictionary<Guid, Entity>();
                    foreach (var entry in kvp.Value)
                    {
                        table[entry.Key] = CloneEntity(entry.Value);
                    }
                    _store[kvp.Key] = table;
                }
                RebuildIndex();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Retrieves entities by their IDs, returning clones. IDs not found are silently skipped.
        /// </summary>
        internal IReadOnlyList<Entity> GetByIds(string entityName, HashSet<Guid> ids)
        {
            var transaction = _activeTransaction.Value;
            if (transaction != null)
            {
                var result = new List<Entity>(ids.Count);
                foreach (var id in ids)
                {
                    if (TryGetEffectiveEntity(entityName, id, transaction, out var entity) && entity != null)
                        result.Add(CloneEntity(entity));
                }

                return result;
            }

            _lock.EnterReadLock();
            try
            {
                if (!_store.TryGetValue(entityName, out var table))
                    return Array.Empty<Entity>();

                var result = new List<Entity>(ids.Count);
                foreach (var id in ids)
                {
                    if (table.TryGetValue(id, out var entity))
                        result.Add(CloneEntity(entity));
                }
                return result;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private bool TryGetEffectiveEntity(string entityName, Guid id, TransactionCopyOnWriteState transaction, out Entity? entity)
        {
            if (transaction.TryGetStagedEntity(entityName, id, out var stagedEntity))
            {
                entity = stagedEntity;
                return true;
            }

            _lock.EnterReadLock();
            try
            {
                if (_store.TryGetValue(entityName, out var table) && table.TryGetValue(id, out var existing))
                {
                    entity = CloneEntity(existing);
                    return true;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            entity = null;
            return false;
        }

        private Dictionary<Guid, Entity> BuildEffectiveTable(string entityName, TransactionCopyOnWriteState transaction)
        {
            Dictionary<Guid, Entity> effective;
            _lock.EnterReadLock();
            try
            {
                effective = new Dictionary<Guid, Entity>();
                if (_store.TryGetValue(entityName, out var table))
                {
                    foreach (var kvp in table)
                        effective[kvp.Key] = CloneEntity(kvp.Value);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            foreach (var staged in transaction.GetEntityChanges(entityName))
            {
                if (staged.Value.Entity == null)
                    effective.Remove(staged.Key);
                else
                    effective[staged.Key] = CloneEntity(staged.Value.Entity);
            }

            return effective;
        }

        private ConcurrentDictionary<Guid, Entity> GetOrCreateTable(string entityName)
        {
            _lock.EnterReadLock();
            try
            {
                if (_store.TryGetValue(entityName, out var table))
                    return table;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            _lock.EnterWriteLock();
            try
            {
                if (!_store.TryGetValue(entityName, out var table))
                {
                    table = new ConcurrentDictionary<Guid, Entity>();
                    _store[entityName] = table;
                }
                return table;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void RebuildIndex()
        {
            if (Index == null) return;
            Index.Clear();
            foreach (var table in _store.Values)
                foreach (var entity in table.Values)
                    Index.OnCreate(entity);
        }

        private static Entity ProjectEntity(Entity source, ColumnSet? columnSet)
        {
            if (columnSet == null || columnSet.AllColumns)
                return CloneEntity(source);

            var projected = new Entity(source.LogicalName, source.Id);
            foreach (var col in columnSet.Columns)
            {
                if (source.Contains(col))
                    projected[col] = CloneAttributeValue(source[col]);
            }
            foreach (var fv in source.FormattedValues)
            {
                if (columnSet.Columns.Contains(fv.Key))
                    projected.FormattedValues[fv.Key] = fv.Value;
            }
            return projected;
        }
    }
}
