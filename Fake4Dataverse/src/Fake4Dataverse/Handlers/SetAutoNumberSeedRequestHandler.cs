using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>SetAutoNumberSeed</c> requests by storing the auto-number seed for a given entity attribute.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Sets the seed value in an in-memory store. Subsequent <c>GetAutoNumberSeed</c> and <c>GetNextAutoNumberValue</c> requests will use the updated value.</para>
    /// </remarks>
    internal sealed class SetAutoNumberSeedRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "SetAutoNumberSeed", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var entityName = (string)request["EntityName"];
            var attributeName = (string)request["AttributeName"];
            var value = (long)request["Value"];

            AutoNumberSeedStore.Set(entityName, attributeName, value);

            return new OrganizationResponse { ResponseName = "SetAutoNumberSeed" };
        }
    }
}
