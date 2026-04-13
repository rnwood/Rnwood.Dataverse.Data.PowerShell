using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>AddUserToRecordTeamRequest</c> by creating a <c>teammembership</c> record for an access team.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Creates a <c>teammembership</c> association record linking the user to the team. Does not validate that the team is an access team or enforce access team template rules.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class AddUserToRecordTeamRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "AddUserToRecordTeam", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {

            var teamId = (Guid)request["TeamId"];
            var systemUserId = (Guid)request["SystemUserId"];

            var membership = new Entity("teammembership")
            {
                ["teamid"] = new EntityReference("team", teamId),
                ["systemuserid"] = new EntityReference("systemuser", systemUserId)
            };
            service.Create(membership);

            var response = new OrganizationResponse { ResponseName = "AddUserToRecordTeam" };
            response.Results["AccessTeamId"] = teamId;
            return response;
        }
    }
}
