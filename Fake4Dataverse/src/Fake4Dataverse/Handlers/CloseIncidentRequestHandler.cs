using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles CloseIncident requests by creating an incident resolution activity and resolving the case.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Creates an <c>incidentresolution</c> activity, resolves the incident to the specified state/status, and sets <c>incidentid</c> lookup on the resolution.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> — state and status are applied when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class CloseIncidentRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CloseIncident", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var incidentResolution = (Entity)request["IncidentResolution"];
            var status = ((OptionSetValue)request["Status"]).Value;

            var incidentRef = incidentResolution.GetAttributeValue<EntityReference>("incidentid");
            if (incidentRef == null)
                throw new InvalidOperationException("IncidentResolution must contain an incidentid reference.");

            // Create the resolution activity
            service.Create(incidentResolution);

            // Close the incident
            var update = new Entity("incident", incidentRef.Id);
            update["statecode"] = new OptionSetValue(1); // Resolved
            update["statuscode"] = new OptionSetValue(status);
            service.Update(update);

            return new OrganizationResponse { ResponseName = "CloseIncident" };
        }
    }
}
