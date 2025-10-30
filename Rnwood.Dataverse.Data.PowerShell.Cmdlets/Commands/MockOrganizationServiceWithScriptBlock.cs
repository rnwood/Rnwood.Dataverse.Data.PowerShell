using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// A wrapper around IOrganizationService that allows intercepting requests with a PowerShell ScriptBlock.
    /// </summary>
    public class MockOrganizationServiceWithScriptBlock : IOrganizationService
    {
        private readonly IOrganizationService _innerService;
        private readonly ScriptBlock _requestInterceptor;

        /// <summary>
        /// Static counter for testing retry logic.
        /// </summary>
        public static int _testFailCount = 0;

        /// <summary>
        /// Initializes a new instance of the MockOrganizationServiceWithScriptBlock class.
        /// </summary>
        /// <param name="innerService">The inner service to delegate to.</param>
        /// <param name="requestInterceptor">The ScriptBlock to invoke for each request.</param>
        public MockOrganizationServiceWithScriptBlock(IOrganizationService innerService, ScriptBlock requestInterceptor)
        {
            _innerService = innerService;
            _requestInterceptor = requestInterceptor;
        }

        /// <summary>
        /// Executes a request, potentially intercepting it with the ScriptBlock.
        /// </summary>
        public OrganizationResponse Execute(OrganizationRequest request)
        {
            // Invoke the ScriptBlock with the request
            try
            {
                var result = _requestInterceptor.Invoke(request);
                if (result != null && result.Count > 0 && result[0].BaseObject is OrganizationResponse response)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                // If the ScriptBlock throws, rethrow it
                throw ex;
            }

            // If no response returned, delegate to inner service
            return _innerService.Execute(request);
        }

        // Delegate all other methods to inner service
        /// <inheritdoc />
        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) =>
            _innerService.Associate(entityName, entityId, relationship, relatedEntities);

        /// <inheritdoc />
        public Guid Create(Entity entity) => _innerService.Create(entity);

        /// <inheritdoc />
        public void Delete(string entityName, Guid id) => _innerService.Delete(entityName, id);

        /// <inheritdoc />
        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) =>
            _innerService.Disassociate(entityName, entityId, relationship, relatedEntities);

        /// <inheritdoc />
        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet) => _innerService.Retrieve(entityName, id, columnSet);

        /// <inheritdoc />
        public EntityCollection RetrieveMultiple(QueryBase query) => _innerService.RetrieveMultiple(query);

        /// <inheritdoc />
        public void Update(Entity entity) => _innerService.Update(entity);
    }
}
