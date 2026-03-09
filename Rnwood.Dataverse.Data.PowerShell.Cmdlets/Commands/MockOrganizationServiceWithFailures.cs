using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// A wrapper around IOrganizationService that allows injecting failures for testing retry logic.
    /// </summary>
    public class MockOrganizationServiceWithFailures : IOrganizationService
    {
        private readonly IOrganizationService _innerService;

        /// <summary>
        /// Initializes a new instance of the MockOrganizationServiceWithFailures class.
        /// </summary>
        /// <param name="innerService">The inner service to delegate to.</param>
        public MockOrganizationServiceWithFailures(IOrganizationService innerService)
        {
            _innerService = innerService;
        }

        /// <summary>
        /// List of functions that can generate exceptions for specific requests.
        /// Each function takes the request and returns an exception to throw, or null to not fail.
        /// </summary>
        public List<Func<OrganizationRequest, Exception>> RequestFailureGenerators { get; } = new List<Func<OrganizationRequest, Exception>>();

        /// <summary>
        /// Function to modify ExecuteMultipleResponse. Takes the request and original response, returns modified response.
        /// Can be used to inject faults into individual batch items or fail the whole batch.
        /// </summary>
        public Func<ExecuteMultipleRequest, ExecuteMultipleResponse, ExecuteMultipleResponse> ExecuteMultipleResponseModifier { get; set; }

        /// <summary>
        /// If true, the next ExecuteMultipleRequest will throw an exception.
        /// Resets to false after one failure.
        /// </summary>
        public bool FailNextExecuteMultiple { get; set; }

        /// <summary>
        /// List of indices in ExecuteMultipleRequest.Requests to fail with faults.
        /// </summary>
        public List<int> FailExecuteMultipleIndices { get; } = new List<int>();

        /// <summary>
        /// Number of times ExecuteMultipleRequest should fail before succeeding.
        /// </summary>
        public int FailExecuteMultipleTimes { get; set; }

        /// <summary>
        /// Executes a request, potentially failing or modifying based on configuration.
        /// </summary>
        public OrganizationResponse Execute(OrganizationRequest request)
        {
            // Check for request-specific failures
            foreach (var generator in RequestFailureGenerators)
            {
                var exception = generator(request);
                if (exception != null)
                {
                    throw exception;
                }
            }

            // Handle ExecuteMultiple specially
            if (request is ExecuteMultipleRequest executeMultipleRequest)
            {
                if (FailExecuteMultipleTimes > 0)
                {
                    FailExecuteMultipleTimes--;
                    throw new Exception("Simulated ExecuteMultiple failure");
                }

                var response = (ExecuteMultipleResponse)_innerService.Execute(request);

                // Inject faults for specified indices
                foreach (var index in FailExecuteMultipleIndices)
                {
                    if (index < response.Responses.Count)
                    {
                        response.Responses[index].Fault = new OrganizationServiceFault
                        {
                            ErrorCode = -2147220970, // Generic error
                            Message = "Simulated fault for testing"
                        };
                        response.Responses[index].Response = null;
                    }
                }

                if (ExecuteMultipleResponseModifier != null)
                {
                    response = ExecuteMultipleResponseModifier(executeMultipleRequest, response);
                }
                return response;
            }

            // Delegate to inner service
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