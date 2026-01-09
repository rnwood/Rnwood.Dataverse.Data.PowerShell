using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Factory for retrieving and caching entity metadata from Dataverse.
	/// </summary>
	public class EntityMetadataFactory
	{
		private IOrganizationService Connection;
		private readonly string ConnectionKey;
		private readonly bool UseSharedCache;

		/// <summary>
		/// Initializes a new instance of the EntityMetadataFactory class.
		/// </summary>
		/// <param name="Connection">The organization service connection to use for metadata retrieval.</param>
		/// <param name="useSharedCache">Whether to use the shared global cache.</param>
		public EntityMetadataFactory(IOrganizationService Connection, bool useSharedCache = false)
		{
			this.Connection = Connection;
			this.UseSharedCache = useSharedCache;

			// Get connection key for caching
			if (Connection is Microsoft.PowerPlatform.Dataverse.Client.ServiceClient serviceClient)
			{
				this.ConnectionKey = MetadataCache.GetConnectionKey(serviceClient);
			}
			else
			{
				this.ConnectionKey = "default";
			}
		}


		/// <summary>
		/// Gets the attribute metadata for the specified entity and column.
		/// </summary>
		/// <param name="entityName">The logical name of the entity.</param>
		/// <param name="columnName">The logical name of the column.</param>
		/// <returns>The attribute metadata, or null if not found.</returns>
		public AttributeMetadata GetAttribute(string entityName, string columnName)
		{
			EntityMetadata entityMetadata = GetLimitedMetadata(entityName);

			return entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == columnName);
		}

		private readonly IDictionary<string, EntityMetadata> _entities = new Dictionary<string, EntityMetadata>();

        /// <summary>
        /// Gets the entity metadata for the specified entity.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <returns>The entity metadata.</returns>
        public EntityMetadata GetLimitedMetadata(string entityName)
        {
            EntityMetadata result;
            bool retrieveAsIfPublished = false; // Use published metadata for CRUD operations


            // Try shared cache first if enabled
            if (UseSharedCache && MetadataCache.TryGetEntityMetadata(ConnectionKey, entityName, EntityFilters.Attributes|EntityFilters.Relationships, retrieveAsIfPublished, out result))
            {
                return result;
            }


            string cacheKey = $"{entityName}$limited";

            // Try local cache
            if (!_entities.TryGetValue(cacheKey, out result))
            {

                RetrieveEntityRequest request = new RetrieveEntityRequest()
                {
                    EntityFilters = EntityFilters.All,
                    RetrieveAsIfPublished = false,
                    LogicalName = entityName
                };

                RetrieveEntityResponse response = (RetrieveEntityResponse)this.Connection.Execute(request);
                result = response.EntityMetadata;
                _entities.Add(cacheKey, result);

                // Add to shared cache if enabled
                if (UseSharedCache)
                {
                    MetadataCache.AddEntityMetadata(ConnectionKey, entityName, EntityFilters.Attributes|EntityFilters.Relationships, retrieveAsIfPublished, result);
                }
            }
            return result;
        }


        /// <summary>
        /// Gets the entity metadata for the specified entity.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <returns>The entity metadata.</returns>
        public EntityMetadata GetMetadata(string entityName)
		{
			EntityMetadata result;
			bool retrieveAsIfPublished = false; // Use published metadata for CRUD operations

			// Try shared cache first if enabled
			if (UseSharedCache && MetadataCache.TryGetEntityMetadata(ConnectionKey, entityName, EntityFilters.All, retrieveAsIfPublished, out result))
			{
				return result;
			}

			// Try local cache
			if (!_entities.TryGetValue(entityName, out result))
			{

				RetrieveEntityRequest request = new RetrieveEntityRequest()
				{
					EntityFilters = EntityFilters.All,
					RetrieveAsIfPublished = false,
					LogicalName = entityName
				};

				RetrieveEntityResponse response = (RetrieveEntityResponse)this.Connection.Execute(request);
				result = response.EntityMetadata;
				_entities.Add(entityName, result);

				// Add to shared cache if enabled
				if (UseSharedCache)
				{
					MetadataCache.AddEntityMetadata(ConnectionKey, entityName, EntityFilters.All, retrieveAsIfPublished, result);
				}
			}
			return result;
		}

		private List<EntityMetadata> _allEntityMetadata;

		/// <summary>
		/// Retrieves (and caches) all EntityMetadata objects for the organization.
		/// </summary>
		/// <returns>Collection of EntityMetadata objects.</returns>
		public IEnumerable<EntityMetadata> GetAllEntityMetadata()
		{
			bool retrieveAsIfPublished = false; // Use published metadata for CRUD operations

			// Try shared cache first if enabled
			if (UseSharedCache && MetadataCache.TryGetAllEntities(ConnectionKey, EntityFilters.Entity, retrieveAsIfPublished, out var cachedEntities))
			{
				return cachedEntities;
			}

			if (_allEntityMetadata == null)
			{
				var request = new RetrieveAllEntitiesRequest()
				{
					EntityFilters = EntityFilters.Entity,
					RetrieveAsIfPublished = false
				};

				var response = (RetrieveAllEntitiesResponse)this.Connection.Execute(request);
				_allEntityMetadata = response.EntityMetadata.Where(em => !string.IsNullOrWhiteSpace(em.LogicalName)).OrderBy(em => em.LogicalName).ToList();

				// Add to shared cache if enabled
				if (UseSharedCache)
				{
					MetadataCache.AddAllEntities(ConnectionKey, EntityFilters.Entity, retrieveAsIfPublished, _allEntityMetadata);
				}
			}

			return _allEntityMetadata;
		}

		/// <summary>
		/// Retrieves (and caches) the logical names of all entities in the organization.
		/// </summary>
		/// <returns>Collection of logical entity names.</returns>
		public IEnumerable<string> GetAllEntityLogicalNames()
		{
			return GetAllEntityMetadata().Select(em => em.LogicalName);
		}
	}
}