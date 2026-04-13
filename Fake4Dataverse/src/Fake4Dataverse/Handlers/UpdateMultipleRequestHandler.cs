using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles the <c>UpdateMultipleRequest</c> by updating each entity in the <c>Targets</c> collection.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Iterates <c>Targets</c> and calls <c>service.Update(entity)</c> for each; pipeline hooks and all auto-set behaviors fire per record identical to individual <c>UpdateRequest</c> calls.</para>
    /// <para><strong>Configuration:</strong> All options apply per record, identical to <c>UpdateRequest</c> processing.</para>
    /// </remarks>
    internal sealed class UpdateMultipleRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "UpdateMultiple", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var targets = request.Parameters.ContainsKey("Targets")
                ? request.Parameters["Targets"] as EntityCollection
                : null;

            if (targets == null)
                throw DataverseFault.InvalidArgumentFault("UpdateMultiple requires a 'Targets' parameter of type EntityCollection.");

            foreach (var entity in targets.Entities)
            {
                service.Update(entity);
            }

            return new OrganizationResponse { ResponseName = "UpdateMultiple" };
        }
    }
}
