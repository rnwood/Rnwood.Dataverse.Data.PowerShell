using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles BulkDelete requests by synchronously deleting all records matching the given queries.
    /// Returns a fake async job ID.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Executes the delete synchronously and immediately; real Dataverse runs bulk delete as a background async job. Returns a fake async job <see cref="EntityReference"/>.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class BulkDeleteRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "BulkDelete", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var querySet = (QueryExpression[])request["QuerySet"];

            // Execute deletes synchronously for testing
            foreach (var query in querySet)
            {
                query.ColumnSet = new ColumnSet(false); // Only need IDs
                var results = service.RetrieveMultiple(query);
                foreach (var entity in results.Entities)
                {
                    service.Delete(entity.LogicalName, entity.Id);
                }
            }

            // Return a fake job ID
            var jobId = Guid.NewGuid();
            var response = new OrganizationResponse { ResponseName = "BulkDelete" };
            response["JobId"] = jobId;
            return response;
        }
    }
}
