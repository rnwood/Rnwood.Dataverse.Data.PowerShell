using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse
{
    /// <summary>
    /// Holds transaction-local copy-on-write mutations.
    /// Each entry is either an upserted entity snapshot or a tombstone (<c>null</c>) for delete.
    /// </summary>
    internal sealed class TransactionCopyOnWriteState
    {
        private readonly Dictionary<string, Dictionary<Guid, StagedEntityChange>> _entityChanges =
            new Dictionary<string, Dictionary<Guid, StagedEntityChange>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Stages an entity upsert in the transaction.
        /// </summary>
        public void StageUpsert(Entity entity, IEnumerable<string> touchedAttributes, IEnumerable<string>? clearedAttributes = null)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.LogicalName)) throw new ArgumentException("Entity logical name must be set.", nameof(entity));
            if (touchedAttributes == null) throw new ArgumentNullException(nameof(touchedAttributes));

            var table = GetOrCreateEntityChanges(entity.LogicalName);
            if (!table.TryGetValue(entity.Id, out var change) || change.Entity == null)
            {
                change = new StagedEntityChange(InMemoryEntityStore.CloneEntity(entity));
                table[entity.Id] = change;
            }
            else
            {
                change.Entity = InMemoryEntityStore.CloneEntity(entity);
            }

            foreach (var touched in touchedAttributes)
            {
                if (!string.IsNullOrEmpty(touched))
                    change.TouchedAttributes.Add(touched);
            }

            if (clearedAttributes != null)
            {
                foreach (var cleared in clearedAttributes)
                {
                    if (!string.IsNullOrEmpty(cleared))
                        change.ClearedAttributes.Add(cleared);
                }
            }

            foreach (var touched in change.TouchedAttributes)
            {
                if (change.Entity != null && change.Entity.Contains(touched))
                    change.ClearedAttributes.Remove(touched);
            }
        }

        /// <summary>
        /// Stages a delete tombstone in the transaction.
        /// </summary>
        public void StageDelete(string entityName, Guid id)
        {
            if (string.IsNullOrEmpty(entityName)) throw new ArgumentException("Entity name is required.", nameof(entityName));
            if (id == Guid.Empty) throw new ArgumentException("Entity id is required.", nameof(id));

            var table = GetOrCreateEntityChanges(entityName);
            table[id] = new StagedEntityChange(null);
        }

        /// <summary>
        /// Gets staged changes for a specific entity type.
        /// </summary>
        public IReadOnlyDictionary<Guid, StagedEntityChange> GetEntityChanges(string entityName)
        {
            if (_entityChanges.TryGetValue(entityName, out var table))
                return table;

            return EmptyEntityChanges.Instance;
        }

        /// <summary>
        /// Gets all staged entity changes.
        /// </summary>
        public IReadOnlyDictionary<string, Dictionary<Guid, StagedEntityChange>> GetAllEntityChanges()
        {
            return _entityChanges;
        }

        /// <summary>
        /// Attempts to get a staged value for an entity id.
        /// </summary>
        /// <returns><c>true</c> when the transaction has an explicit staged entry (upsert or delete) for the entity.</returns>
        public bool TryGetStagedEntity(string entityName, Guid id, out Entity? entity)
        {
            if (_entityChanges.TryGetValue(entityName, out var table) && table.TryGetValue(id, out var staged))
            {
                entity = staged.Entity == null ? null : InMemoryEntityStore.CloneEntity(staged.Entity);
                return true;
            }

            entity = null;
            return false;
        }

        private Dictionary<Guid, StagedEntityChange> GetOrCreateEntityChanges(string entityName)
        {
            if (!_entityChanges.TryGetValue(entityName, out var table))
            {
                table = new Dictionary<Guid, StagedEntityChange>();
                _entityChanges[entityName] = table;
            }

            return table;
        }

        internal sealed class StagedEntityChange
        {
            public Entity? Entity { get; set; }

            public HashSet<string> TouchedAttributes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public HashSet<string> ClearedAttributes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public StagedEntityChange(Entity? entity)
            {
                Entity = entity;
            }
        }

        private sealed class EmptyEntityChanges : IReadOnlyDictionary<Guid, StagedEntityChange>
        {
            public static readonly EmptyEntityChanges Instance = new EmptyEntityChanges();

            public IEnumerable<Guid> Keys => Array.Empty<Guid>();

            public IEnumerable<StagedEntityChange> Values => Array.Empty<StagedEntityChange>();

            public int Count => 0;

            public StagedEntityChange this[Guid key] => throw new KeyNotFoundException();

            public bool ContainsKey(Guid key) => false;

            public IEnumerator<KeyValuePair<Guid, StagedEntityChange>> GetEnumerator()
            {
                return ((IEnumerable<KeyValuePair<Guid, StagedEntityChange>>)Array.Empty<KeyValuePair<Guid, StagedEntityChange>>()).GetEnumerator();
            }

            public bool TryGetValue(Guid key, out StagedEntityChange value)
            {
                value = null!;
                return false;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
