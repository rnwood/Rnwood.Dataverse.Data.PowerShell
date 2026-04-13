using System;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RemoveMembersTeamRequest"/> by removing teammembership association records.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Removes all <c>teammembership</c> association records for the specified member IDs. Does not validate that the members are currently in the team.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RemoveMembersTeamRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RemoveMembersTeam", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var removeRequest = OrganizationRequestTypeAdapter.AsTyped<RemoveMembersTeamRequest>(request);
            var teamId = removeRequest.TeamId;
            var memberIds = removeRequest.MemberIds;

            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("RemoveMembersTeamRequestHandler requires FakeOrganizationService.");

            var allMemberships = fakeService.Environment.Store.GetAll("teammembership");
            foreach (var memberId in memberIds)
            {
                var match = allMemberships.FirstOrDefault(e =>
                {
                    var team = e.GetAttributeValue<EntityReference>("teamid");
                    var user = e.GetAttributeValue<EntityReference>("systemuserid");
                    return team != null && team.Id == teamId && user != null && user.Id == memberId;
                });
                if (match != null)
                    service.Delete("teammembership", match.Id);
            }

            return new RemoveMembersTeamResponse();
        }
    }
}
