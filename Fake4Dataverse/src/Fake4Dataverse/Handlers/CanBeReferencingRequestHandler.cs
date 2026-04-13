using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CanBeReferencingRequest"/> — returns whether the specified entity can be the referencing (child) entity in a 1:N relationship.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Same as <c>CanBeReferenced</c>; returns <see langword="true"/> for any entity registered in the metadata store.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.ValidateWithMetadata"/> — when <see langword="false"/> (default), any entity name returns <see langword="true"/>.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class CanBeReferencingRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CanBeReferencing", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var canRequest = OrganizationRequestTypeAdapter.AsTyped<CanBeReferencingRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            // In the fake, any registered entity can be a referencing entity
            var entityInfo = store.GetEntityMetadataInfo(canRequest.EntityName);
            var canBeReferencing = entityInfo != null;

            var response = new CanBeReferencingResponse();
            response.Results["CanBeReferencing"] = canBeReferencing;
            return response;
        }
    }
}
