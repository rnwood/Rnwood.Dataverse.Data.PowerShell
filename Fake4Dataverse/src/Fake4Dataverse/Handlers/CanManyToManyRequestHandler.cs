using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CanManyToManyRequest"/> — returns whether the specified entity can participate in a many-to-many relationship.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns <see langword="true"/> for any entity registered in the metadata store. Real Dataverse restricts certain system entities.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.ValidateWithMetadata"/> — when <see langword="false"/> (default), any entity name returns <see langword="true"/>.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class CanManyToManyRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CanManyToMany", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var canRequest = OrganizationRequestTypeAdapter.AsTyped<CanManyToManyRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            // In the fake, any registered entity can participate in N:N
            var entityInfo = store.GetEntityMetadataInfo(canRequest.EntityName);
            var canManyToMany = entityInfo != null;

            var response = new CanManyToManyResponse();
            response.Results["CanManyToMany"] = canManyToMany;
            return response;
        }
    }
}
