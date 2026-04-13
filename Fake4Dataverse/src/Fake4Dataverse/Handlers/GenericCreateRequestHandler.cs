using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Fallback handler for untyped <see cref="OrganizationRequest"/> instances whose <see cref="OrganizationRequest.RequestName"/> is <c>"Create"</c> but which are not strongly typed as <see cref="CreateRequest"/>. Delegates to <see cref="IOrganizationService.Create"/>.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Delegates to the service layer; all configured behaviors apply.</para>
    /// <para><strong>Configuration:</strong> All <see cref="FakeOrganizationServiceOptions"/> flags apply as they do for <see cref="CreateRequest"/>.</para>
    /// </remarks>
    internal sealed class GenericCreateRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            request is not CreateRequest &&
            string.Equals(request.RequestName, "Create", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var target = (Entity)request["Target"];
            var id = service.Create(target);
            return new OrganizationResponse { Results = { ["id"] = id } };
        }
    }
}
