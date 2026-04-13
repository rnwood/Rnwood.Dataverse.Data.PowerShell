using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="UpdateRequest"/> by updating a record by ID or alternate key, with optional optimistic concurrency enforcement.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Supports <c>ConcurrencyBehavior.IfRowVersionMatches</c>, alternate-key targeting via <c>EntityReference.KeyAttributes</c>, and cascade assign when <c>ownerid</c> is changed.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.EnablePipeline"/> — when <see langword="true"/>, pre/post operation pipeline hooks fire around the update.</description></item>
    ///   <item><description><see cref="FakeOrganizationServiceOptions.EnforceSecurityRoles"/> — when <see langword="true"/>, the caller must have Write privilege.</description></item>
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetTimestamps"/> — sets <c>modifiedon</c> when <see langword="true"/> (default).</description></item>
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetOwner"/> — sets <c>modifiedby</c> when <see langword="true"/> (default).</description></item>
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetVersionNumber"/> — increments <c>versionnumber</c> when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class UpdateRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "Update", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var updateRequest = OrganizationRequestTypeAdapter.AsTyped<UpdateRequest>(request);

            if (updateRequest.ConcurrencyBehavior == ConcurrencyBehavior.IfRowVersionMatches
                && service is FakeOrganizationService fakeService)
            {
                var target = updateRequest.Target;
                if (string.IsNullOrEmpty(target.RowVersion))
                    throw DataverseFault.ConcurrencyVersionNotProvidedFault();

                if (!long.TryParse(target.RowVersion, out var expectedVersion))
                    throw DataverseFault.ConcurrencyVersionNotProvidedFault();

                fakeService.UpdateWithConcurrencyCheck(target, expectedVersion);
            }
            else
            {
                service.Update(updateRequest.Target);
            }

            return new UpdateResponse();
        }
    }
}
