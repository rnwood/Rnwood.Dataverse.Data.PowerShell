using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	internal class EntityMetadataFactory
	{
		private IOrganizationService Connection;

		public EntityMetadataFactory(IOrganizationService Connection)
		{
			this.Connection = Connection;
		}


		internal AttributeMetadata GetAttribute(string entityName, string columnName)
		{
			EntityMetadata entityMetadata = GetMetadata(entityName);

			return entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == columnName);
		}

		private readonly IDictionary<string, EntityMetadata> _entities = new Dictionary<string, EntityMetadata>();

		internal EntityMetadata GetMetadata(string entityName)
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