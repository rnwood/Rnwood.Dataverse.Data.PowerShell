using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CanBeReferencedRequest"/> — returns whether the specified entity can be referenced (be the primary entity) in a 1:N relationship.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns <see langword="true"/> for any entity registered in the metadata store; returns <see langword="false"/> only if the entity has no metadata registered. Real Dataverse has per-entity restrictions.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.ValidateWithMetadata"/> — when <see langword="false"/> (default), any entity name returns <see langword="true"/>.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class CanBeReferencedRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CanBeReferenced", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var canRequest = OrganizationRequestTypeAdapter.AsTyped<CanBeReferencedRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            // In the fake, any registered entity can be a referenced entity
            var entityInfo = store.GetEntityMetadataInfo(canRequest.EntityName);
            var canBeReferenced = entityInfo != null;

            var response = new CanBeReferencedResponse();
            response.Results["CanBeReferenced"] = canBeReferenced;
            return response;
        }
    }
}
