using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="AssignRequest"/> by updating the ownerid on the target entity.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Updates <c>ownerid</c> and triggers cascade-assign of child records exactly as real Dataverse does when <see cref="FakeOrganizationServiceOptions.AutoSetOwner"/> is enabled.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetOwner"/> — when <see langword="true"/> (default), cascaded child record ownership follows the parent.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class AssignRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "Assign", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var assignRequest = OrganizationRequestTypeAdapter.AsTyped<AssignRequest>(request);
            var target = assignRequest.Target;
            var update = new Entity(target.LogicalName, target.Id)
            {
                ["ownerid"] = assignRequest.Assignee
            };
            service.Update(update);
            return new AssignResponse();
        }
    }
}
