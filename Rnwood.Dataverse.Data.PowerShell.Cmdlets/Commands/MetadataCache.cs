using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Global metadata cache shared across all cmdlets.
    /// </summary>
    public static class MetadataCache
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, EntityMetadata>> _cache 
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, EntityMetadata>>(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<string, List<EntityMetadata>> _allEntitiesCache
            = new ConcurrentDictionary<string, List<EntityMetadata>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets whether caching is enabled globally.
        /// </summary>
        public static bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Gets the cache key for a connection.
        /// </summary>
        private static string GetConnectionKey(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return "default";
            }
            return connectionString;
        }

        /// <summary>
        /// Tries to get entity metadata from the cache.
        /// </summary>
        public static bool TryGetEntityMetadata(string connectionKey, string entityName, out EntityMetadata metadata)
        {
            metadata = null;
            if (!IsEnabled)
            {
                return false;
            }

            if (_cache.TryGetValue(connectionKey, out var entityCache))
            {
                return entityCache.TryGetValue(entityName, out metadata);
            }

            return false;
        }

        /// <summary>
        /// Adds entity metadata to the cache.
        /// </summary>
        public static void AddEntityMetadata(string connectionKey, string entityName, EntityMetadata metadata)
        {
            if (!IsEnabled)
            {
                return;
            }

            var entityCache = _cache.GetOrAdd(connectionKey, _ => new ConcurrentDictionary<string, EntityMetadata>(StringComparer.OrdinalIgnoreCase));
            entityCache[entityName] = metadata;
        }

        /// <summary>
        /// Tries to get all entities metadata from the cache.
        /// </summary>
        public static bool TryGetAllEntities(string connectionKey, out List<EntityMetadata> allEntities)
        {
            allEntities = null;
            if (!IsEnabled)
            {
                return false;
            }

            return _allEntitiesCache.TryGetValue(connectionKey, out allEntities);
        }

        /// <summary>
        /// Adds all entities metadata to the cache.
        /// </summary>
        public static void AddAllEntities(string connectionKey, List<EntityMetadata> allEntities)
        {
            if (!IsEnabled)
            {
                return;
            }

            _allEntitiesCache[connectionKey] = allEntities;
        }

        /// <summary>
        /// Invalidates cache for a specific entity.
        /// </summary>
        public static void InvalidateEntity(string connectionKey, string entityName)
        {
            if (_cache.TryGetValue(connectionKey, out var entityCache))
            {
                entityCache.TryRemove(entityName, out _);
            }

            // Also invalidate the all entities cache since it may be stale
            _allEntitiesCache.TryRemove(connectionKey, out _);
        }

        /// <summary>
        /// Clears all cached metadata for a specific connection.
        /// </summary>
        public static void ClearConnection(string connectionKey)
        {
            _cache.TryRemove(connectionKey, out _);
            _allEntitiesCache.TryRemove(connectionKey, out _);
        }

        /// <summary>
        /// Clears all cached metadata.
        /// </summary>
        public static void ClearAll()
        {
            _cache.Clear();
            _allEntitiesCache.Clear();
        }

        /// <summary>
        /// Gets the cache key for a connection object.
        /// </summary>
        public static string GetConnectionKey(Microsoft.PowerPlatform.Dataverse.Client.ServiceClient connection)
        {
            if (connection == null)
            {
                return "default";
            }

            // Use the connection string or org URL as the cache key
            return connection.ConnectedOrgUniqueName ?? connection.ConnectedOrgUriActual?.ToString() ?? "default";
        }
    }
}
