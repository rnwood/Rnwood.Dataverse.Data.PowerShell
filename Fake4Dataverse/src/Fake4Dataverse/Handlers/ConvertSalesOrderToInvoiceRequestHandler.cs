using System;
using System.Collections.Generic;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="ConvertSalesOrderToInvoiceRequest"/> by creating a new invoice from a sales order.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Retrieves the sales order, copies eligible attributes to a new <c>invoice</c> entity, creates it via the pipeline, and sets the sales order to invoiced state (<c>statecode=4</c>). System fields are excluded from the copy.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> — state/status fields are applied when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class ConvertSalesOrderToInvoiceRequestHandler : IOrganizationRequestHandler
    {
        private static readonly HashSet<string> ExcludedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "id", "salesorderid", "statecode", "statuscode", "createdon", "modifiedon",
            "versionnumber", "createdby", "modifiedby", "ownerid",
            "owningbusinessunit", "owningteam", "owninguser", "logicalname"
        };

        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "ConvertSalesOrderToInvoice", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var typedRequest = OrganizationRequestTypeAdapter.AsTyped<ConvertSalesOrderToInvoiceRequest>(request);
            var salesOrderId = typedRequest.SalesOrderId;

            var columnSet = request.Parameters.ContainsKey("ColumnSet")
                ? (ColumnSet)request["ColumnSet"]
                : new ColumnSet(true);

            var salesOrder = service.Retrieve("salesorder", salesOrderId, columnSet);

            var invoice = new Entity("invoice", Guid.NewGuid());
            foreach (var attr in salesOrder.Attributes)
            {
                if (!ExcludedFields.Contains(attr.Key))
                    invoice[attr.Key] = attr.Value;
            }

            service.Create(invoice);

            var orderUpdate = new Entity("salesorder", salesOrderId);
            orderUpdate["statecode"] = new OptionSetValue(4); // Invoiced
            service.Update(orderUpdate);

            var response = new ConvertSalesOrderToInvoiceResponse();
            response.Results["Entity"] = invoice;
            return response;
        }
    }
}
