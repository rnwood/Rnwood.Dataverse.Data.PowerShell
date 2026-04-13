using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles CloseQuote requests by creating a quote close activity and closing the quote.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Creates a <c>quoteclose</c> activity and sets the quote <c>statecode</c>/<c>statuscode</c> to closed (state=3).</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> — state and status are applied when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class CloseQuoteRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CloseQuote", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var quoteClose = (Entity)request["QuoteClose"];
            var status = ((OptionSetValue)request["Status"]).Value;

            var quoteRef = quoteClose.GetAttributeValue<EntityReference>("quoteid");
            if (quoteRef == null)
                throw new InvalidOperationException("QuoteClose must contain a quoteid reference.");

            service.Create(quoteClose);

            var update = new Entity("quote", quoteRef.Id);
            update["statecode"] = new OptionSetValue(2); // Closed
            update["statuscode"] = new OptionSetValue(status);
            service.Update(update);

            return new OrganizationResponse { ResponseName = "CloseQuote" };
        }
    }
}
