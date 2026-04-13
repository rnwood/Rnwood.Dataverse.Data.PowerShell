using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="UpdateStateValueRequest"/> by updating the label of an existing state or status value in the entity's state/status attribute metadata.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Increments the metadata store's internal timestamp (making retrieval reflect a change) but does not update option set metadata or persist label changes. State/status transition rules registered via <c>env.RegisterStateTransition(...)</c> are unaffected.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class UpdateStateValueRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "UpdateStateValue", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            // UpdateStateValue updates the label/description of a State option value.
            // In the fake, this is a no-op that acknowledges the request.
            var fakeService = (FakeOrganizationService)service;
            fakeService.Environment.MetadataStore.IncrementMetadataTimestamp();

            return new UpdateStateValueResponse();
        }
    }
}
