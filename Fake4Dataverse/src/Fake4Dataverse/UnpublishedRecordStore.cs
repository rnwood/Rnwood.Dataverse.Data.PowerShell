using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse
{
    /// <summary>
    /// Manages unpublished (draft) copies of solution-aware entity records.
    /// Solution-aware entities have a <c>componentstate</c> column; created and updated records
    /// are staged here until published via <c>PublishXmlRequest</c> or <c>PublishAllXmlRequest</c>.
    /// </summary>
    internal sealed class UnpublishedRecordStore : IDisposable
    {
        private readonly InMemoryEntityStore _unpublishedStore = new InMemoryEntityStore();
        private readonly HashSet<string> _solutionAwareEntities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the underlying store for unpublished records.
        /// Used by <c>RetrieveUnpublishedRequest</c> and <c>RetrieveUnpublishedMultipleRequest</c> handlers.
        /// </summary>
        internal InMemoryEntityStore Store => _unpublishedStore;

        /// <summary>
        /// Registers an entity type as solution-aware. Created and updated records of this type
        /// will be staged in the unpublished store until published.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        public void RegisterSolutionAwareEntity(string entityName)
        {
            if (string.IsNullOrEmpty(entityName))
                throw new ArgumentException("Entity name is required.", nameof(entityName));
            _solutionAwareEntities.Add(entityName);
        }

        /// <summary>
        /// Returns <c>true</c> if the specified entity type has been registered as solution-aware.
        /// </summary>
        public bool IsSolutionAware(string entityName)
        {
            return _solutionAwareEntities.Contains(entityName);
        }

        /// <summary>
        /// Gets the set of registered solution-aware entity names.
        /// </summary>
        internal IReadOnlyCollection<string> SolutionAwareEntities => _solutionAwareEntities;

        /// <summary>
        /// Stages a newly created entity in the unpublished store with <c>componentstate = 1</c> (Unpublished).
        /// </summary>
        internal Guid CreateUnpublished(Entity entity)
        {
            var clone = InMemoryEntityStore.CloneEntity(entity);
            clone["componentstate"] = new OptionSetValue(1); // Unpublished
            return _unpublishedStore.Create(clone);
        }

        /// <summary>
        /// Stages an update to an unpublished record. If the record does not yet exist in the
        /// unpublished store, a copy of the published record is placed there first, then updated.
        /// </summary>
        internal void UpdateUnpublished(Entity entity, InMemoryEntityStore publishedStore)
        {
            if (!_unpublishedStore.Exists(entity.LogicalName, entity.Id))
            {
                // Copy published record to unpublished so we have a base to merge into
                var published = publishedStore.Retrieve(entity.LogicalName, entity.Id, new ColumnSet(true));
                published["componentstate"] = new OptionSetValue(1); // Unpublished
                _unpublishedStore.Create(published);
            }

            var updateClone = InMemoryEntityStore.CloneEntity(entity);
            updateClone["componentstate"] = new OptionSetValue(1);
            _unpublishedStore.Update(updateClone);
        }

        /// <summary>
        /// Removes a record from the unpublished store. Called during delete of solution-aware entities.
        /// Silently succeeds if the record does not exist in the unpublished store.
        /// </summary>
        internal void DeleteUnpublished(string entityName, Guid id)
        {
            if (_unpublishedStore.Exists(entityName, id))
                _unpublishedStore.Delete(entityName, id);
        }

        /// <summary>
        /// Publishes all unpublished records of the specified entity type by moving them to the published store.
        /// Sets <c>componentstate = 0</c> (Published) on each record.
        /// </summary>
        internal void Publish(string entityName, InMemoryEntityStore publishedStore)
        {
            var unpublished = _unpublishedStore.GetAll(entityName);
            foreach (var record in unpublished)
            {
                var clone = InMemoryEntityStore.CloneEntity(record);
                clone["componentstate"] = new OptionSetValue(0); // Published

                if (publishedStore.Exists(entityName, clone.Id))
                {
                    // Merge all attributes from unpublished into published
                    publishedStore.Update(clone);
                }
                else
                {
                    publishedStore.Create(clone);
                }

                _unpublishedStore.Delete(entityName, clone.Id);
            }
        }

        /// <summary>
        /// Publishes a single record by ID, moving it from unpublished to published store.
        /// Sets <c>componentstate = 0</c> (Published).
        /// </summary>
        internal void PublishRecord(string entityName, Guid id, InMemoryEntityStore publishedStore)
        {
            if (!_unpublishedStore.Exists(entityName, id))
                return;

            var record = _unpublishedStore.Retrieve(entityName, id, new ColumnSet(true));
            var clone = InMemoryEntityStore.CloneEntity(record);
            clone["componentstate"] = new OptionSetValue(0); // Published

            if (publishedStore.Exists(entityName, clone.Id))
            {
                publishedStore.Update(clone);
            }
            else
            {
                publishedStore.Create(clone);
            }

            _unpublishedStore.Delete(entityName, id);
        }

        /// <summary>
        /// Publishes all unpublished records across all solution-aware entity types.
        /// </summary>
        internal void PublishAll(InMemoryEntityStore publishedStore)
        {
            foreach (var entityName in _solutionAwareEntities)
            {
                Publish(entityName, publishedStore);
            }
        }

        /// <summary>
        /// Clears all data from the unpublished store.
        /// </summary>
        internal void Clear()
        {
            _unpublishedStore.Clear();
        }

        /// <summary>
        /// Takes a snapshot of the unpublished store and solution-aware entity registrations.
        /// </summary>
        internal (Dictionary<string, Dictionary<Guid, Entity>> StoreData, HashSet<string> Entities) TakeSnapshot()
        {
            return (_unpublishedStore.TakeSnapshot(), new HashSet<string>(_solutionAwareEntities, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Restores the unpublished store and solution-aware entity registrations from a snapshot.
        /// </summary>
        internal void RestoreSnapshot(Dictionary<string, Dictionary<Guid, Entity>> storeData, HashSet<string> entities)
        {
            _unpublishedStore.RestoreSnapshot(storeData);
            _solutionAwareEntities.Clear();
            foreach (var e in entities)
                _solutionAwareEntities.Add(e);
        }

        public void Dispose()
        {
            _unpublishedStore.Dispose();
        }
    }
}
