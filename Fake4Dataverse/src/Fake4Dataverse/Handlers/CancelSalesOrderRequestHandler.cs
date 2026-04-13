using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CancelSalesOrderRequest"/> by creating an order close activity and marking the sales order as canceled.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Creates an <c>orderclose</c> activity record and sets the sales order to <c>statecode=4</c> (Canceled) with the specified <c>statuscode</c>.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> — state/status fields are applied when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class CancelSalesOrderRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CancelSalesOrder", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var orderClose = (Entity)request["OrderClose"];
            var status = ((OptionSetValue)request["Status"]).Value;

            var orderRef = orderClose.GetAttributeValue<EntityReference>("salesorderid");
            if (orderRef == null)
                throw new InvalidOperationException("OrderClose must contain a salesorderid reference.");

            service.Create(orderClose);

            var update = new Entity("salesorder", orderRef.Id);
            update["statecode"] = new OptionSetValue(4); // Canceled
            update["statuscode"] = new OptionSetValue(status);
            service.Update(update);

            return new CancelSalesOrderResponse();
        }
    }
}
