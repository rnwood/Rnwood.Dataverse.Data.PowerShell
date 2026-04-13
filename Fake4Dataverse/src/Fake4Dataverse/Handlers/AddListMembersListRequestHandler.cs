using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="AddListMembersListRequest"/> by creating listmember association records.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Creates <c>listmember</c> association records in the in-memory store; does not replicate list type validation or member de-duplication.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class AddListMembersListRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "AddListMembersList", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var addRequest = OrganizationRequestTypeAdapter.AsTyped<AddListMembersListRequest>(request);
            var listId = addRequest.ListId;
            var memberIds = addRequest.MemberIds;

            foreach (var memberId in memberIds)
            {
                var membership = new Entity("listmember")
                {
                    ["listid"] = new EntityReference("list", listId),
                    ["entityid"] = memberId
                };
                service.Create(membership);
            }

            return new AddListMembersListResponse();
        }
    }
}
