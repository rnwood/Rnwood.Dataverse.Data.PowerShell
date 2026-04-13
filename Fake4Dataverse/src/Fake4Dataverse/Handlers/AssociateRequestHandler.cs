using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="AssociateRequest"/> by delegating to <see cref="IOrganizationService.Associate(string,System.Guid,Relationship,EntityReferenceCollection)"/>.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Uses the existing association service path, including record existence checks, duplicate-association detection, relationship validation, and N:N association record creation.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.ValidateWithMetadata"/> — when <see langword="true"/>, relationship metadata is validated before associating records.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class AssociateRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "Associate", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var associateRequest = OrganizationRequestTypeAdapter.AsTyped<AssociateRequest>(request);
            var target = associateRequest.Target ?? throw DataverseFault.InvalidArgumentFault("AssociateRequest.Target is required.");

            service.Associate(target.LogicalName, target.Id, associateRequest.Relationship, associateRequest.RelatedEntities);
            return new AssociateResponse();
        }
    }
}