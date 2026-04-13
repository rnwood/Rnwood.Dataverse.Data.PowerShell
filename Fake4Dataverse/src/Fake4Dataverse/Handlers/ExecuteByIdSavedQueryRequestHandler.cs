using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="ExecuteByIdSavedQueryRequest"/> by executing the FetchXml stored on a <c>savedquery</c> record.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Reads the <c>fetchxml</c> column from the specified <c>savedquery</c> record, executes it through the fake FetchXml pipeline, and returns a simplified serialized XML result string.</para>
    /// <para><strong>Configuration:</strong> Any query-related environment settings (such as security filtering when enabled) apply because execution is delegated through <see cref="IOrganizationService.RetrieveMultiple"/>.</para>
    /// </remarks>
    internal sealed class ExecuteByIdSavedQueryRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "ExecuteByIdSavedQuery", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var executeRequest = OrganizationRequestTypeAdapter.AsTyped<ExecuteByIdSavedQueryRequest>(request);
            var query = service.Retrieve("savedquery", executeRequest.EntityId, new ColumnSet("fetchxml"));
            var fetchXml = query.GetAttributeValue<string>("fetchxml");

            if (string.IsNullOrWhiteSpace(fetchXml))
                throw new InvalidOperationException($"savedquery '{executeRequest.EntityId}' does not contain a fetchxml value.");

            var response = new ExecuteByIdSavedQueryResponse();
            response.Results["String"] = FetchExecutionHelper.ExecuteAndSerialize(service, fetchXml);
            return response;
        }
    }
}