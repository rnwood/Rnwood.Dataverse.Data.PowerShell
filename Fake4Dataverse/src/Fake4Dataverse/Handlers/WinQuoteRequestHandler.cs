using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>WinQuoteRequest</c> by creating a <c>quoteclose</c> activity and setting the quote to Won state.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Creates a <c>quoteclose</c> activity record, then sets the quote record's <c>statecode</c> to 3 (Won) and <c>statuscode</c> from the request. Mirrors the pattern used by <c>WinOpportunityRequestHandler</c>.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> — state/status update applies when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class WinQuoteRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "WinQuote", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var quoteClose = (Entity)request["QuoteClose"];
            var status = ((OptionSetValue)request["Status"]).Value;

            var quoteRef = quoteClose.GetAttributeValue<EntityReference>("quoteid");
            if (quoteRef == null)
                throw new InvalidOperationException("QuoteClose must contain a quoteid reference.");

            service.Create(quoteClose);

            var update = new Entity("quote", quoteRef.Id);
            update["statecode"] = new OptionSetValue(3); // Won
            update["statuscode"] = new OptionSetValue(status);
            service.Update(update);

            return new OrganizationResponse { ResponseName = "WinQuote" };
        }
    }
}
