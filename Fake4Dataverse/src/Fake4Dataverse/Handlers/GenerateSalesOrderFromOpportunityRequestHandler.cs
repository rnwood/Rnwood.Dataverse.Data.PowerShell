using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="GenerateSalesOrderFromOpportunityRequest"/> by creating a new sales order linked to an opportunity.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Creates a minimal <c>salesorder</c> entity with an <c>opportunityid</c> reference back to the source opportunity. The <c>ColumnSet</c> parameter is accepted but does not influence the returned entity shape in this implementation.</para>
    /// </remarks>
    internal sealed class GenerateSalesOrderFromOpportunityRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "GenerateSalesOrderFromOpportunity", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var opportunityId = (Guid)request["OpportunityId"];

            var salesOrder = new Entity("salesorder", Guid.NewGuid());
            salesOrder["opportunityid"] = new EntityReference("opportunity", opportunityId);

            service.Create(salesOrder);

            var response = new GenerateSalesOrderFromOpportunityResponse();
            response.Results["Entity"] = salesOrder;
            return response;
        }
    }
}
