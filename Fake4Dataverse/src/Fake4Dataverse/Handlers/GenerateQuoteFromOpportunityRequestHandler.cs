using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="GenerateQuoteFromOpportunityRequest"/> by creating a new quote linked to an opportunity.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Creates a minimal <c>quote</c> entity with an <c>opportunityid</c> reference back to the source opportunity. The <c>ColumnSet</c> parameter is accepted but does not influence the returned entity shape in this implementation.</para>
    /// </remarks>
    internal sealed class GenerateQuoteFromOpportunityRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "GenerateQuoteFromOpportunity", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var opportunityId = (Guid)request["OpportunityId"];

            var quote = new Entity("quote", Guid.NewGuid());
            quote["opportunityid"] = new EntityReference("opportunity", opportunityId);

            service.Create(quote);

            var response = new GenerateQuoteFromOpportunityResponse();
            response.Results["Entity"] = quote;
            return response;
        }
    }
}
