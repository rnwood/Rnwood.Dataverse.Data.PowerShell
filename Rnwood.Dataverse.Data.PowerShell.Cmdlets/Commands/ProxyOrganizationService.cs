using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Proxy wrapper around IOrganizationService that records method calls for testing.
    /// </summary>
    public class ProxyOrganizationService : IOrganizationService
    {
        private readonly IOrganizationService _innerService;
        private readonly List<OrganizationRequest> _executedRequests = new List<OrganizationRequest>();
        private readonly List<OrganizationResponse> _responses = new List<OrganizationResponse>();

        public ProxyOrganizationService(IOrganizationService innerService)
        {
            _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
        }

        /// <summary>
        /// Gets the list of requests that have been executed.
        /// </summary>
        public IReadOnlyList<OrganizationRequest> ExecutedRequests => _executedRequests.AsReadOnly();

        /// <summary>
        /// Gets the list of responses that have been returned.
        /// </summary>
        public IReadOnlyList<OrganizationResponse> Responses => _responses.AsReadOnly();

        /// <summary>
        /// Gets the last request that was executed.
        /// </summary>
        public OrganizationRequest LastRequest => _executedRequests.LastOrDefault();

        /// <summary>
        /// Gets the last response that was returned.
        /// </summary>
        public OrganizationResponse LastResponse => _responses.LastOrDefault();

        /// <summary>
        /// Clears the recorded requests and responses.
        /// </summary>
        public void ClearHistory()
        {
            _executedRequests.Clear();
            _responses.Clear();
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            _innerService.Associate(entityName, entityId, relationship, relatedEntities);
        }

        public Guid Create(Entity entity)
        {
            return _innerService.Create(entity);
        }

        public void Delete(string entityName, Guid id)
        {
            _innerService.Delete(entityName, id);
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            _innerService.Disassociate(entityName, entityId, relationship, relatedEntities);
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            _executedRequests.Add(request);
            var response = _innerService.Execute(request);
            _responses.Add(response);
            return response;
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            return _innerService.Retrieve(entityName, id, columnSet);
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            return _innerService.RetrieveMultiple(query);
        }

        public void Update(Entity entity)
        {
            _innerService.Update(entity);
        }
    }
}
