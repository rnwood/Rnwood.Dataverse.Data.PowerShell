using System;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="InsertStatusValueRequest"/> — adds a new status reason (status code) option to an entity's <c>statuscode</c> attribute.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Acknowledges the request and returns the new status value; does not persist a new option to attribute metadata. State/status transitions registered via <c>env.RegisterStateTransition(...)</c> are unaffected.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class InsertStatusValueRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "InsertStatusValue", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var insertRequest = OrganizationRequestTypeAdapter.AsTyped<InsertStatusValueRequest>(request);
            var newValue = insertRequest.Value ?? new Random().Next(100000, 999999);

            var fakeService = (FakeOrganizationService)service;
            fakeService.Environment.MetadataStore.IncrementMetadataTimestamp();

            var response = new InsertStatusValueResponse();
            response.Results["NewOptionValue"] = newValue;
            return response;
        }
    }
}
