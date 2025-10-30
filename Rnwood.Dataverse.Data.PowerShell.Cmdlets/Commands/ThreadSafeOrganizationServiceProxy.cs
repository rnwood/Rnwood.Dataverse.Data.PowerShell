using Microsoft.Xrm.Sdk;
using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Thread-safe wrapper for IOrganizationService.
	/// Used to wrap FakeXrmEasy services which are not thread-safe.
	/// Also patches FakeXrmEasy limitations by ensuring primary key attributes are populated.
	/// </summary>
	internal class ThreadSafeOrganizationServiceProxy : IOrganizationService
	{
		private readonly IOrganizationService _innerService;
		private readonly object _lock = new object();

		public ThreadSafeOrganizationServiceProxy(IOrganizationService innerService)
		{
			_innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
		}

		/// <summary>
		/// Patches FakeXrmEasy limitation by ensuring the primary key attribute is set in the entity.
		/// FakeXrmEasy sometimes returns entities without the primary key attribute populated.
		/// </summary>
		private void EnsurePrimaryKeyAttribute(Entity entity)
		{
			if (entity == null) return;

			// Get the primary key attribute name (e.g., "contactid" for "contact")
			string primaryKeyName = entity.LogicalName + "id";

			// If the entity has an Id but the primary key attribute is missing, add it
			if (entity.Id != Guid.Empty && !entity.Contains(primaryKeyName))
			{
				entity[primaryKeyName] = entity.Id;
			}
		}

		public Guid Create(Entity entity)
		{
			lock (_lock)
			{
				return _innerService.Create(entity);
			}
		}

		public Entity Retrieve(string entityName, Guid id, Microsoft.Xrm.Sdk.Query.ColumnSet columnSet)
		{
			lock (_lock)
			{
				var result = _innerService.Retrieve(entityName, id, columnSet);
				EnsurePrimaryKeyAttribute(result);
				return result;
			}
		}

		public void Update(Entity entity)
		{
			lock (_lock)
			{
				_innerService.Update(entity);
			}
		}

		public void Delete(string entityName, Guid id)
		{
			lock (_lock)
			{
				_innerService.Delete(entityName, id);
			}
		}

		public OrganizationResponse Execute(OrganizationRequest request)
		{
			lock (_lock)
			{
				var response = _innerService.Execute(request);
				
				// Patch responses that return entities
				if (response != null && response.Results != null)
				{
					foreach (var key in response.Results.Keys)
					{
						if (response.Results[key] is Entity entity)
						{
							EnsurePrimaryKeyAttribute(entity);
						}
						else if (response.Results[key] is EntityCollection entityCollection)
						{
							foreach (var ent in entityCollection.Entities)
							{
								EnsurePrimaryKeyAttribute(ent);
							}
						}
					}
				}
				
				return response;
			}
		}

		public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
		{
			lock (_lock)
			{
				_innerService.Associate(entityName, entityId, relationship, relatedEntities);
			}
		}

		public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
		{
			lock (_lock)
			{
				_innerService.Disassociate(entityName, entityId, relationship, relatedEntities);
			}
		}

		public EntityCollection RetrieveMultiple(Microsoft.Xrm.Sdk.Query.QueryBase query)
		{
			lock (_lock)
			{
				var result = _innerService.RetrieveMultiple(query);
				
				// Patch all entities in the collection
				if (result != null && result.Entities != null)
				{
					foreach (var entity in result.Entities)
					{
						EnsurePrimaryKeyAttribute(entity);
					}
				}
				
				return result;
			}
		}
	}
}
