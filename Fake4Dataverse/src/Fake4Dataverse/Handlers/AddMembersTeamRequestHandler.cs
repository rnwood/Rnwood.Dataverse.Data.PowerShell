using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="AddMembersTeamRequest"/> by creating teammembership association records.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Creates <c>teammembership</c> association records; does not validate team type (owner vs. access) or enforce duplicate membership rules.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class AddMembersTeamRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "AddMembersTeam", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var addRequest = OrganizationRequestTypeAdapter.AsTyped<AddMembersTeamRequest>(request);
            var teamId = addRequest.TeamId;
            var memberIds = addRequest.MemberIds;

            foreach (var memberId in memberIds)
            {
                var membership = new Entity("teammembership")
                {
                    ["teamid"] = new EntityReference("team", teamId),
                    ["systemuserid"] = new EntityReference("systemuser", memberId)
                };
                service.Create(membership);
            }

            return new AddMembersTeamResponse();
        }
    }
}
