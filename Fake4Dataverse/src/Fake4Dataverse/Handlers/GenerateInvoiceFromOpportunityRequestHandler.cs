using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="GenerateInvoiceFromOpportunityRequest"/> by creating a new invoice linked to an opportunity.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Creates a minimal <c>invoice</c> entity with an <c>opportunityid</c> reference back to the source opportunity. The <c>ColumnSet</c> parameter is accepted but does not influence the returned entity shape in this implementation.</para>
    /// </remarks>
    internal sealed class GenerateInvoiceFromOpportunityRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "GenerateInvoiceFromOpportunity", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var opportunityId = (Guid)request["OpportunityId"];

            var invoice = new Entity("invoice", Guid.NewGuid());
            invoice["opportunityid"] = new EntityReference("opportunity", opportunityId);

            service.Create(invoice);

            var response = new GenerateInvoiceFromOpportunityResponse();
            response.Results["Entity"] = invoice;
            return response;
        }
    }
}
