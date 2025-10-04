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

		/// <summary>
		/// Initializes a new instance of the EntityMetadataFactory class.
		/// </summary>
		/// <param name="Connection">The organization service connection to use for metadata retrieval.</param>
		public EntityMetadataFactory(IOrganizationService Connection)
		{
			this.Connection = Connection;
		}


		/// <summary>
		/// Gets the attribute metadata for the specified entity and column.
		/// </summary>
		/// <param name="entityName">The logical name of the entity.</param>
		/// <param name="columnName">The logical name of the column.</param>
		/// <returns>The attribute metadata, or null if not found.</returns>
		public AttributeMetadata GetAttribute(string entityName, string columnName)
		{
			EntityMetadata entityMetadata = GetMetadata(entityName);

			return entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == columnName);
		}

		private readonly IDictionary<string, EntityMetadata> _entities = new Dictionary<string, EntityMetadata>();

		/// <summary>
		/// Gets the entity metadata for the specified entity.
		/// </summary>
		/// <param name="entityName">The logical name of the entity.</param>
		/// <returns>The entity metadata.</returns>
		public EntityMetadata GetMetadata(string entityName)
		{
			EntityMetadata result;

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
			}
			return result;
		}
	}
}