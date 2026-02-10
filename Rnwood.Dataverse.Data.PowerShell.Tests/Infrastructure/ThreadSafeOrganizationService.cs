using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// Thread-safe wrapper around IOrganizationService that synchronizes access for parallel operations.
    /// This allows Invoke-DataverseParallel to use the same connection without Clone() support.
    /// </summary>
    internal class ThreadSafeOrganizationService : IOrganizationService
    {
        private readonly IOrganizationService _innerService;
        private readonly object _lock = new object();

        public ThreadSafeOrganizationService(IOrganizationService innerService)
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

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
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

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            lock (_lock)
            {
                return _innerService.RetrieveMultiple(query);
            }
        }
    }
}
