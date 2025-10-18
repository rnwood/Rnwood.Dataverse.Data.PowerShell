using Microsoft.Xrm.Sdk;
using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Thread-safe wrapper for IOrganizationService.
	/// Used to wrap FakeXrmEasy services which are not thread-safe.
	/// </summary>
	internal class ThreadSafeOrganizationServiceProxy : IOrganizationService
	{
		private readonly IOrganizationService _innerService;
		private readonly object _lock = new object();

		public ThreadSafeOrganizationServiceProxy(IOrganizationService innerService)
		{
			_innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
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
				return _innerService.Retrieve(entityName, id, columnSet);
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
				return _innerService.Execute(request);
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
				return _innerService.RetrieveMultiple(query);
			}
		}
	}
}
