using System;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveMembersTeamRequest"/> by returning the team membership records for the specified team.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Queries <c>teammembership</c> records from the in-memory store for the specified team. Returns an <c>EntityCollection</c> of <c>systemuser</c> references. Does not return full user records.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RetrieveMembersTeamRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveMembersTeam", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var retrieveRequest = OrganizationRequestTypeAdapter.AsTyped<RetrieveMembersTeamRequest>(request);
            var teamId = retrieveRequest.EntityId;

            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("RetrieveMembersTeamRequestHandler requires FakeOrganizationService.");

            var allMemberships = fakeService.Environment.Store.GetAll("teammembership");
            var members = allMemberships
                .Where(e =>
                {
                    var teamRef = e.GetAttributeValue<EntityReference>("teamid");
                    return teamRef != null && teamRef.Id == teamId;
                })
                .Select(e =>
                {
                    var userRef = e.GetAttributeValue<EntityReference>("systemuserid");
                    var member = new Entity("systemuser", userRef?.Id ?? Guid.Empty);
                    member["systemuserid"] = userRef?.Id ?? Guid.Empty;
                    return member;
                })
                .ToList();

            var collection = new EntityCollection(members) { EntityName = "systemuser" };

            var response = new RetrieveMembersTeamResponse();
            response.Results["EntityCollection"] = collection;
            return response;
        }
    }
}
