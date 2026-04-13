using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="ExecuteByIdUserQueryRequest"/> by executing the FetchXml stored on a <c>userquery</c> record.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Reads the <c>fetchxml</c> column from the specified <c>userquery</c> record, executes it through the fake FetchXml pipeline, and returns a simplified serialized XML result string.</para>
    /// <para><strong>Configuration:</strong> Any query-related environment settings (such as security filtering when enabled) apply because execution is delegated through <see cref="IOrganizationService.RetrieveMultiple"/>.</para>
    /// </remarks>
    internal sealed class ExecuteByIdUserQueryRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "ExecuteByIdUserQuery", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var executeRequest = OrganizationRequestTypeAdapter.AsTyped<ExecuteByIdUserQueryRequest>(request);
            var entityReference = executeRequest.EntityId ?? throw new InvalidOperationException("ExecuteByIdUserQueryRequest.EntityId is required.");
            var query = service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet("fetchxml"));
            var fetchXml = query.GetAttributeValue<string>("fetchxml");

            if (string.IsNullOrWhiteSpace(fetchXml))
                throw new InvalidOperationException($"userquery '{entityReference.Id}' does not contain a fetchxml value.");

            var response = new ExecuteByIdUserQueryResponse();
            response.Results["String"] = FetchExecutionHelper.ExecuteAndSerialize(service, fetchXml);
            return response;
        }
    }
}