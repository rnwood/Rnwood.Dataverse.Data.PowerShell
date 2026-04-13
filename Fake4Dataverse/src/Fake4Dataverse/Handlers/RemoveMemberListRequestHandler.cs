using System;
using System.Linq;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RemoveMemberListRequest"/> by removing listmember association records.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Removes the <c>listmember</c> association record matching the specified member. Does not validate list type or membership state before removal.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RemoveMemberListRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RemoveMemberList", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var removeRequest = OrganizationRequestTypeAdapter.AsTyped<RemoveMemberListRequest>(request);
            var listId = removeRequest.ListId;
            var entityId = removeRequest.EntityId;

            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("RemoveMemberListRequestHandler requires FakeOrganizationService.");

            var allMembers = fakeService.Environment.Store.GetAll("listmember");
            var match = allMembers.FirstOrDefault(e =>
            {
                var list = e.GetAttributeValue<EntityReference>("listid");
                return list != null && list.Id == listId && e.Id == entityId;
            });
            if (match != null)
                service.Delete("listmember", match.Id);

            return new RemoveMemberListResponse();
        }
    }
}
