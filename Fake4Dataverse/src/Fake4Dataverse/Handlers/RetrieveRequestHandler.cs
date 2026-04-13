using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveRequest"/> by delegating to <see cref="IOrganizationService.Retrieve"/>, supporting both ID-based and alternate-key-based retrieval.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Supports <c>ConcurrencyBehavior</c> headers, alternate key targeting via <c>EntityReference.KeyAttributes</c>, calculated field population, formatted values, and entity reference name population.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.EnforceSecurityRoles"/> — Read privilege is checked when enabled.</description></item>
    ///   <item><description><see cref="FakeOrganizationServiceOptions.ValidateWithMetadata"/> — alternate key resolution requires key metadata when enabled.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class RetrieveRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "Retrieve", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var retrieveRequest = OrganizationRequestTypeAdapter.AsTyped<RetrieveRequest>(request);
            var target = retrieveRequest.Target;
            Entity entity;

            if (target.Id == Guid.Empty && target.KeyAttributes != null && target.KeyAttributes.Count > 0
                && service is FakeOrganizationService fakeService)
            {
                entity = fakeService.RetrieveByAlternateKey(target.LogicalName, target.KeyAttributes, retrieveRequest.ColumnSet);
            }
            else
            {
                entity = service.Retrieve(target.LogicalName, target.Id, retrieveRequest.ColumnSet);
            }

            return new RetrieveResponse { Results = { ["Entity"] = entity } };
        }
    }
}
