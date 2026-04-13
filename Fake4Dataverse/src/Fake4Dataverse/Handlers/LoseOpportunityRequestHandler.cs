using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles LoseOpportunity requests by creating an opportunity close activity and marking the opportunity as lost.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Creates an <c>opportunityclose</c> activity record and sets the opportunity to <c>statecode=2</c> (Lost) with the specified <c>statuscode</c>.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> — state/status fields are applied when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class LoseOpportunityRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "LoseOpportunity", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var opportunityClose = (Entity)request["OpportunityClose"];
            var status = ((OptionSetValue)request["Status"]).Value;

            var oppRef = opportunityClose.GetAttributeValue<EntityReference>("opportunityid");
            if (oppRef == null)
                throw new InvalidOperationException("OpportunityClose must contain an opportunityid reference.");

            service.Create(opportunityClose);

            var update = new Entity("opportunity", oppRef.Id);
            update["statecode"] = new OptionSetValue(2); // Lost
            update["statuscode"] = new OptionSetValue(status);
            service.Update(update);

            return new OrganizationResponse { ResponseName = "LoseOpportunity" };
        }
    }
}
