using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>RetrieveUnpublishedMultipleRequest</c> by querying the unpublished store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Functional</para>
    /// <para>Queries unpublished (draft) records from solution-aware entity types. Supports
    /// <see cref="QueryExpression"/> and <see cref="Microsoft.Xrm.Sdk.Query.FetchExpression"/>.</para>
    /// <para><strong>Configuration:</strong> Requires solution-aware entity registration via
    /// <see cref="FakeDataverseEnvironment.RegisterSolutionAwareEntity"/>.</para>
    /// </remarks>
    internal sealed class RetrieveUnpublishedMultipleRequestHandler : IOrganizationRequestHandler
    {
        private readonly UnpublishedRecordStore _unpublishedStore;
        private readonly QueryExpressionEvaluator _queryEvaluator;
        private readonly FetchXmlEvaluator _fetchXmlEvaluator;

        internal RetrieveUnpublishedMultipleRequestHandler(
            UnpublishedRecordStore unpublishedStore,
            QueryExpressionEvaluator queryEvaluator,
            FetchXmlEvaluator fetchXmlEvaluator)
        {
            _unpublishedStore = unpublishedStore;
            _queryEvaluator = queryEvaluator;
            _fetchXmlEvaluator = fetchXmlEvaluator;
        }

        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveUnpublishedMultiple", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var query = (QueryBase)request.Parameters["Query"];
            EntityCollection result;

            if (query is QueryExpression qe)
            {
                result = _queryEvaluator.Evaluate(qe, _unpublishedStore.Store);
            }
            else if (query is FetchExpression fe)
            {
                result = _fetchXmlEvaluator.Evaluate(fe.Query, _unpublishedStore.Store);
            }
            else
            {
                throw new NotSupportedException($"Query type '{query.GetType().Name}' is not supported for RetrieveUnpublishedMultiple.");
            }

            var response = new RetrieveUnpublishedMultipleResponse();
            response.Results["EntityCollection"] = result;
            return response;
        }
    }
}
