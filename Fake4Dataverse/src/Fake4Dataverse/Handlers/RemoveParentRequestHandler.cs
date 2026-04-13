using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RemoveParentRequest"/> by clearing the parent entity reference from the target record.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Clears the <c>parentid</c> attribute on the target record, removing the parent–child relationship.
    /// If the target entity uses a different parent attribute name (e.g. <c>parentcustomerid</c>), set it in the request parameters.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RemoveParentRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RemoveParent", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var removeRequest = OrganizationRequestTypeAdapter.AsTyped<RemoveParentRequest>(request);
            var target = removeRequest.Target;

            var update = new Entity(target.LogicalName, target.Id)
            {
                ["parentid"] = null
            };
            service.Update(update);

            return new OrganizationResponse { ResponseName = "RemoveParent" };
        }
    }
}
