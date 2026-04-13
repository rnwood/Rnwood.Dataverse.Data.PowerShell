using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse
{
    /// <summary>
    /// Maintains equality-based indexes on entity attributes to accelerate filtered queries.
    /// Thread-safe: all mutating and lookup operations are guarded by an internal lock.
    /// </summary>
    internal sealed class AttributeIndex
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<(string EntityName, string AttributeName), Dictionary<object, HashSet<Guid>>> _indexes
            = new Dictionary<(string, string), Dictionary<object, HashSet<Guid>>>(TupleComparer.Instance);

        /// <summary>
        /// Registers an attribute for indexing. Does not retroactively index existing entities;
        /// use <see cref="IndexAttribute"/> for that.
        /// </summary>
        public void AddIndex(string entityName, string attributeName)
        {
            if (entityName == null) throw new ArgumentNullException(nameof(entityName));
            if (attributeName == null) throw new ArgumentNullException(nameof(attributeName));

            lock (_syncRoot)
            {
                var key = (entityName, attributeName);
                if (!_indexes.ContainsKey(key))
                    _indexes[key] = new Dictionary<object, HashSet<Guid>>(AttributeValueComparer.Instance);
            }
        }

        /// <summary>
        /// Indexes a single attribute value for an entity. Used for retroactive indexing
        /// when an index is added after entities already exist.
        /// </summary>
        public void IndexAttribute(string entityName, Guid id, string attributeName, object? value)
        {
            var normalized = Normalize(value);
            if (normalized == null) return;

            lock (_syncRoot)
            {
                var key = (entityName, attributeName);
                if (!_indexes.TryGetValue(key, out var index)) return;

                if (!index.TryGetValue(normalized, out var ids))
                {
                    ids = new HashSet<Guid>();
                    index[normalized] = ids;
                }
                ids.Add(id);
            }
        }

        /// <summary>
        /// Updates the index when an entity is created.
        /// </summary>
        public void OnCreate(Entity entity)
        {
            if (entity == null) return;
            var entityName = entity.LogicalName;
            if (entityName == null) return;

            lock (_syncRoot)
            {
                foreach (var attr in entity.Attributes)
                {
                    var key = (entityName, attr.Key);
                    if (!_indexes.TryGetValue(key, out var index)) continue;

                    var normalized = Normalize(attr.Value);
                    if (normalized == null) continue;

                    if (!index.TryGetValue(normalized, out var ids))
                    {
                        ids = new HashSet<Guid>();
                        index[normalized] = ids;
                    }
                    ids.Add(entity.Id);
                }
            }
        }

        /// <summary>
        /// Updates the index when an entity is updated. Must be called before applying
        /// the attribute changes so <paramref name="existingEntity"/> reflects old values.
        /// </summary>
        public void OnUpdate(Entity updateEntity, Entity existingEntity)
        {
            if (updateEntity == null || existingEntity == null) return;
            var entityName = updateEntity.LogicalName;
            if (entityName == null) return;

            lock (_syncRoot)
            {
                foreach (var attr in updateEntity.Attributes)
                {
                    var key = (entityName, attr.Key);
                    if (!_indexes.TryGetValue(key, out var index)) continue;

                    // Remove old value from index
                    if (existingEntity.Contains(attr.Key))
                    {
                        var oldNormalized = Normalize(existingEntity[attr.Key]);
                        if (oldNormalized != null && index.TryGetValue(oldNormalized, out var oldIds))
                        {
                            oldIds.Remove(updateEntity.Id);
                            if (oldIds.Count == 0) index.Remove(oldNormalized);
                        }
                    }

                    // Add new value to index
                    var newNormalized = Normalize(attr.Value);
                    if (newNormalized != null)
                    {
                        if (!index.TryGetValue(newNormalized, out var newIds))
                        {
                            newIds = new HashSet<Guid>();
                            index[newNormalized] = newIds;
                        }
                        newIds.Add(updateEntity.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the index when an entity is deleted.
        /// </summary>
        public void OnDelete(string entityName, Guid id, Entity existingEntity)
        {
            if (existingEntity == null || entityName == null) return;

            lock (_syncRoot)
            {
                foreach (var attr in existingEntity.Attributes)
                {
                    var key = (entityName, attr.Key);
                    if (!_indexes.TryGetValue(key, out var index)) continue;

                    var normalized = Normalize(attr.Value);
                    if (normalized == null) continue;

                    if (index.TryGetValue(normalized, out var ids))
                    {
                        ids.Remove(id);
                        if (ids.Count == 0) index.Remove(normalized);
                    }
                }
            }
        }

        /// <summary>
        /// Looks up entity IDs matching an indexed attribute value.
        /// Returns <c>null</c> if the attribute is not indexed, allowing the caller to fall
        /// back to a full scan.
        /// </summary>
        public HashSet<Guid>? Lookup(string entityName, string attributeName, object? value)
        {
            var normalized = Normalize(value);

            lock (_syncRoot)
            {
                var key = (entityName, attributeName);
                if (!_indexes.TryGetValue(key, out var index)) return null;

                if (normalized == null) return new HashSet<Guid>();
                return index.TryGetValue(normalized, out var ids) ? new HashSet<Guid>(ids) : new HashSet<Guid>();
            }
        }

        /// <summary>
        /// Clears all indexed data but preserves index definitions.
        /// </summary>
        public void Clear()
        {
            lock (_syncRoot)
            {
                foreach (var index in _indexes.Values)
                    index.Clear();
            }
        }

        private static object? Normalize(object? value)
        {
            if (value is EntityReference er) return er.Id;
            if (value is OptionSetValue osv) return osv.Value;
            if (value is Money m) return m.Value;
            return value;
        }

        private sealed class TupleComparer : IEqualityComparer<(string, string)>
        {
            public static readonly TupleComparer Instance = new TupleComparer();

            public bool Equals((string, string) x, (string, string) y)
                => StringComparer.OrdinalIgnoreCase.Equals(x.Item1, y.Item1)
                && StringComparer.OrdinalIgnoreCase.Equals(x.Item2, y.Item2);

            public int GetHashCode((string, string) obj)
            {
                unchecked
                {
                    return (StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item1) * 397)
                         ^ StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item2);
                }
            }
        }

        private sealed class AttributeValueComparer : IEqualityComparer<object>
        {
            public static readonly AttributeValueComparer Instance = new AttributeValueComparer();

            public new bool Equals(object? x, object? y)
            {
                if (x is string xs && y is string ys)
                    return string.Equals(xs, ys, StringComparison.OrdinalIgnoreCase);

                return object.Equals(x, y);
            }

            public int GetHashCode(object obj)
            {
                if (obj is string s)
                    return StringComparer.OrdinalIgnoreCase.GetHashCode(s);

                return obj.GetHashCode();
            }
        }
    }
}
