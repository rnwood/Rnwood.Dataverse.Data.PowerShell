using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="DeleteMultipleRequest"/> by deleting each target through the standard delete request pipeline.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Delegates each target to <see cref="DeleteRequest"/>, preserving alternate-key resolution, cascade delete behavior, optimistic concurrency enforcement, and pipeline execution on every record.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.EnablePipeline"/> — when <see langword="true"/>, pipeline steps fire for each inner delete.</description></item>
    ///   <item><description><see cref="FakeOrganizationServiceOptions.EnforceSecurityRoles"/> — when <see langword="true"/>, the caller must have Delete privilege for every target.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class DeleteMultipleRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "DeleteMultiple", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var deleteRequest = OrganizationRequestTypeAdapter.AsTyped<DeleteMultipleRequest>(request);
            var targets = deleteRequest.Targets ?? throw DataverseFault.InvalidArgumentFault("DeleteMultipleRequest.Targets must not be null.");

            foreach (var target in targets)
            {
                service.Execute(new DeleteRequest { Target = target });
            }

            return new OrganizationResponse { ResponseName = "DeleteMultiple" };
        }
    }
}