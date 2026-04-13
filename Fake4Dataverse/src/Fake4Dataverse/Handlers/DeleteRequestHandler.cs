using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="DeleteRequest"/> — deletes a record by ID or alternate key, with optional optimistic concurrency enforcement.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Supports <c>ConcurrencyBehavior.IfRowVersionMatches</c>, alternate-key targeting via <c>EntityReference.KeyAttributes</c>, and cascade delete behaviors registered via <c>env.RegisterRelationship(...)</c>.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.EnablePipeline"/> — when <see langword="true"/>, pre/post operation pipeline hooks fire around the delete.</description></item>
    ///   <item><description><see cref="FakeOrganizationServiceOptions.EnforceSecurityRoles"/> — when <see langword="true"/>, the caller must have Delete privilege.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class DeleteRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "Delete", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var deleteRequest = OrganizationRequestTypeAdapter.AsTyped<DeleteRequest>(request);
            var target = deleteRequest.Target;

            Guid id = target.Id;
            if (id == Guid.Empty && target.KeyAttributes != null && target.KeyAttributes.Count > 0
                && service is FakeOrganizationService fakeService1)
            {
                id = fakeService1.Environment.Store.FindByAlternateKey(target.LogicalName, target.KeyAttributes, fakeService1.Environment.MetadataStore);
            }

            if (deleteRequest.ConcurrencyBehavior == ConcurrencyBehavior.IfRowVersionMatches
                && service is FakeOrganizationService fakeService2)
            {
                if (string.IsNullOrEmpty(target.RowVersion))
                    throw DataverseFault.ConcurrencyVersionNotProvidedFault();

                if (!long.TryParse(target.RowVersion, out var expectedVersion))
                    throw DataverseFault.ConcurrencyVersionNotProvidedFault();

                fakeService2.DeleteWithConcurrencyCheck(target.LogicalName, id, expectedVersion);
            }
            else
            {
                service.Delete(target.LogicalName, id);
            }

            return new DeleteResponse();
        }
    }
}
