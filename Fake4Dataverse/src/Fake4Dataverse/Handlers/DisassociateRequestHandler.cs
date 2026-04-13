using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="DisassociateRequest"/> by delegating to <see cref="IOrganizationService.Disassociate(string,System.Guid,Relationship,EntityReferenceCollection)"/>.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Uses the existing disassociation service path, including metadata validation when enabled and selective removal of only the requested association rows.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.ValidateWithMetadata"/> — when <see langword="true"/>, relationship metadata is validated before disassociating records.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class DisassociateRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "Disassociate", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var disassociateRequest = OrganizationRequestTypeAdapter.AsTyped<DisassociateRequest>(request);
            var target = disassociateRequest.Target ?? throw DataverseFault.InvalidArgumentFault("DisassociateRequest.Target is required.");

            service.Disassociate(target.LogicalName, target.Id, disassociateRequest.Relationship, disassociateRequest.RelatedEntities);
            return new DisassociateResponse();
        }
    }
}