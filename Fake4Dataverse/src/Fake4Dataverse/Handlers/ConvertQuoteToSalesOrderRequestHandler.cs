using System;
using System.Collections.Generic;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="ConvertQuoteToSalesOrderRequest"/> by creating a new sales order from a quote.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Retrieves the quote, copies eligible attributes to a new <c>salesorder</c> entity, creates it via the pipeline, and closes the quote (<c>statecode=2</c>). System fields are excluded from the copy.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> — state/status fields are applied when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class ConvertQuoteToSalesOrderRequestHandler : IOrganizationRequestHandler
    {
        private static readonly HashSet<string> ExcludedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "id", "quoteid", "statecode", "statuscode", "createdon", "modifiedon",
            "versionnumber", "createdby", "modifiedby", "ownerid",
            "owningbusinessunit", "owningteam", "owninguser", "logicalname"
        };

        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "ConvertQuoteToSalesOrder", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var typedRequest = OrganizationRequestTypeAdapter.AsTyped<ConvertQuoteToSalesOrderRequest>(request);
            var quoteId = typedRequest.QuoteId;

            var columnSet = request.Parameters.ContainsKey("ColumnSet")
                ? (ColumnSet)request["ColumnSet"]
                : new ColumnSet(true);

            var quote = service.Retrieve("quote", quoteId, columnSet);

            var salesOrder = new Entity("salesorder", Guid.NewGuid());
            foreach (var attr in quote.Attributes)
            {
                if (!ExcludedFields.Contains(attr.Key))
                    salesOrder[attr.Key] = attr.Value;
            }

            service.Create(salesOrder);

            var quoteUpdate = new Entity("quote", quoteId);
            quoteUpdate["statecode"] = new OptionSetValue(2); // Closed
            service.Update(quoteUpdate);

            var response = new ConvertQuoteToSalesOrderResponse();
            response.Results["Entity"] = salesOrder;
            return response;
        }
    }
}
