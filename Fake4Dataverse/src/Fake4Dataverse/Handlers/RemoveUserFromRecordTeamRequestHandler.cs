using System;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>RemoveUserFromRecordTeamRequest</c> by removing the <c>teammembership</c> record for an access team.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Removes the <c>teammembership</c> association record that links the user to the team. Does not validate access team template rules or cascade shared access removal.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RemoveUserFromRecordTeamRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RemoveUserFromRecordTeam", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var teamId = (Guid)request["TeamId"];
            var systemUserId = (Guid)request["SystemUserId"];

            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("RemoveUserFromRecordTeamRequestHandler requires FakeOrganizationService.");

            var allMemberships = fakeService.Environment.Store.GetAll("teammembership");
            var match = allMemberships.FirstOrDefault(e =>
            {
                var team = e.GetAttributeValue<EntityReference>("teamid");
                var user = e.GetAttributeValue<EntityReference>("systemuserid");
                return team != null && team.Id == teamId && user != null && user.Id == systemUserId;
            });

            if (match != null)
                service.Delete("teammembership", match.Id);

            var response = new OrganizationResponse { ResponseName = "RemoveUserFromRecordTeam" };
            response.Results["AccessTeamId"] = teamId;
            return response;
        }
    }
}
