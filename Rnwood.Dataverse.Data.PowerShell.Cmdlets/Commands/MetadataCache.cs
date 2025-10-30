using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Global metadata cache shared across all cmdlets.
    /// Cache is automatically used when -UseMetadataCache parameter is specified on Get cmdlets.
    /// </summary>
    public static class MetadataCache
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, EntityMetadata>> _cache 
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, EntityMetadata>>(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<string, List<EntityMetadata>> _allEntitiesCache
            = new ConcurrentDictionary<string, List<EntityMetadata>>(StringComparer.OrdinalIgnoreCase);

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
        /// Creates a cache key that includes entity filters to ensure correct cached data is returned.
        /// </summary>
        private static string GetEntityCacheKey(string entityName, EntityFilters filters)
        {
            return $"{entityName}|{(int)filters}";
        }

        /// <summary>
        /// Creates a cache key for all entities that includes entity filters.
        /// </summary>
        private static string GetAllEntitiesCacheKey(string connectionKey, EntityFilters filters)
        {
            return $"{connectionKey}|{(int)filters}";
        }

        /// <summary>
        /// Tries to get entity metadata from the cache.
        /// </summary>
        public static bool TryGetEntityMetadata(string connectionKey, string entityName, EntityFilters filters, out EntityMetadata metadata)
        {
            metadata = null;
            var cacheKey = GetEntityCacheKey(entityName, filters);

            if (_cache.TryGetValue(connectionKey, out var entityCache))
            {
                return entityCache.TryGetValue(cacheKey, out metadata);
            }

            return false;
        }

        /// <summary>
        /// Adds entity metadata to the cache.
        /// </summary>
        public static void AddEntityMetadata(string connectionKey, string entityName, EntityFilters filters, EntityMetadata metadata)
        {
            var cacheKey = GetEntityCacheKey(entityName, filters);
            var entityCache = _cache.GetOrAdd(connectionKey, _ => new ConcurrentDictionary<string, EntityMetadata>(StringComparer.OrdinalIgnoreCase));
            entityCache[cacheKey] = metadata;
        }

        /// <summary>
        /// Tries to get all entities metadata from the cache.
        /// </summary>
        public static bool TryGetAllEntities(string connectionKey, EntityFilters filters, out List<EntityMetadata> allEntities)
        {
            var cacheKey = GetAllEntitiesCacheKey(connectionKey, filters);
            return _allEntitiesCache.TryGetValue(cacheKey, out allEntities);
        }

        /// <summary>
        /// Adds all entities metadata to the cache.
        /// </summary>
        public static void AddAllEntities(string connectionKey, EntityFilters filters, List<EntityMetadata> allEntities)
        {
            var cacheKey = GetAllEntitiesCacheKey(connectionKey, filters);
            _allEntitiesCache[cacheKey] = allEntities;
        }

        /// <summary>
        /// Invalidates cache for a specific entity (all filter variants).
        /// </summary>
        public static void InvalidateEntity(string connectionKey, string entityName)
        {
            if (_cache.TryGetValue(connectionKey, out var entityCache))
            {
                // Remove all cached versions of this entity (with different filters)
                var keysToRemove = new List<string>();
                foreach (var key in entityCache.Keys)
                {
                    if (key.StartsWith(entityName + "|", StringComparison.OrdinalIgnoreCase))
                    {
                        keysToRemove.Add(key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    entityCache.TryRemove(key, out _);
                }
            }

            // Also invalidate all entities caches since they may be stale
            var allEntitiesToRemove = new List<string>();
            foreach (var key in _allEntitiesCache.Keys)
            {
                if (key.StartsWith(connectionKey + "|", StringComparison.OrdinalIgnoreCase))
                {
                    allEntitiesToRemove.Add(key);
                }
            }

            foreach (var key in allEntitiesToRemove)
            {
                _allEntitiesCache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Clears all cached metadata for a specific connection.
        /// </summary>
        public static void ClearConnection(string connectionKey)
        {
            _cache.TryRemove(connectionKey, out _);
            
            // Remove all entities cache entries for this connection
            var keysToRemove = new List<string>();
            foreach (var key in _allEntitiesCache.Keys)
            {
                if (key.StartsWith(connectionKey + "|", StringComparison.OrdinalIgnoreCase))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _allEntitiesCache.TryRemove(key, out _);
            }
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
