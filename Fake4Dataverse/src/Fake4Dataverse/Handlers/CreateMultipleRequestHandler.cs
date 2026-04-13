using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles the CreateMultiple request by creating each entity in the Targets collection.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Iterates <c>Targets</c> and calls <c>service.Create(entity)</c> for each; pipeline hooks and all auto-set behaviors fire per record identical to individual <c>CreateRequest</c> calls.</para>
    /// <para><strong>Configuration:</strong> All options apply per record, identical to <c>CreateRequest</c> processing.</para>
    /// </remarks>
    internal sealed class CreateMultipleRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CreateMultiple", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var targets = request.Parameters.ContainsKey("Targets")
                ? request.Parameters["Targets"] as EntityCollection
                : null;

            if (targets == null)
                throw DataverseFault.InvalidArgumentFault("CreateMultiple requires a 'Targets' parameter of type EntityCollection.");

            var ids = new Guid[targets.Entities.Count];
            for (int i = 0; i < targets.Entities.Count; i++)
            {
                ids[i] = service.Create(targets.Entities[i]);
            }

            var response = new OrganizationResponse { ResponseName = "CreateMultiple" };
            response.Results["Ids"] = ids;
            return response;
        }
    }
}
