using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>ReviseQuoteRequest</c> by creating a new draft copy of an existing quote.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Creates a new draft quote record by copying all attributes from the original quote, then sets <c>statecode=0</c> / <c>statuscode=1</c> (Draft) on the new quote and marks the original as revised.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetTimestamps"/>, <see cref="FakeOrganizationServiceOptions.AutoSetOwner"/>, <see cref="FakeOrganizationServiceOptions.AutoSetVersionNumber"/> — apply to the newly created quote record.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class ReviseQuoteRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "ReviseQuote", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var quoteId = ((EntityReference)request["QuoteId"]).Id;
            var columnSet = request.Parameters.ContainsKey("ColumnSet") ? (ColumnSet)request["ColumnSet"] : new ColumnSet(true);

            // Retrieve original quote
            var original = service.Retrieve("quote", quoteId, columnSet);

            // Create a revised copy
            var revised = new Entity("quote");
            foreach (var attr in original.Attributes)
            {
                if (attr.Key != "quoteid" && attr.Key != "statecode" && attr.Key != "statuscode")
                    revised[attr.Key] = InMemoryEntityStore.CloneAttributeValue(attr.Value);
            }
            revised["statecode"] = new OptionSetValue(0); // Draft
            revised["statuscode"] = new OptionSetValue(1); // Draft
            var revisedId = service.Create(revised);

            var response = new OrganizationResponse { ResponseName = "ReviseQuote" };
            response["Entity"] = service.Retrieve("quote", revisedId, new ColumnSet(true));
            return response;
        }
    }
}
