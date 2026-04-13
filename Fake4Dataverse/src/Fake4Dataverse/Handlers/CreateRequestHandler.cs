using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CreateRequest"/> by delegating to <see cref="IOrganizationService.Create"/>, which runs the full pipeline and auto-set behaviors.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Delegates entirely to the service layer; all configured behaviors (<see cref="FakeOrganizationServiceOptions.AutoSetTimestamps"/>, <see cref="FakeOrganizationServiceOptions.AutoSetOwner"/>, etc.) apply.</para>
    /// <para><strong>Configuration:</strong> All <see cref="FakeOrganizationServiceOptions"/> flags apply as they do for direct <c>service.Create(...)</c> calls.</para>
    /// </remarks>
    internal sealed class CreateRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "Create", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var createRequest = OrganizationRequestTypeAdapter.AsTyped<CreateRequest>(request);
            var id = service.Create(createRequest.Target);
            return new CreateResponse { Results = { ["id"] = id } };
        }
    }
}
