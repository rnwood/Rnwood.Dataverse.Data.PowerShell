using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>GetAutoNumberSeed</c> requests by returning the current auto-number seed for a given entity attribute.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Retrieves the seed value from an in-memory store. If no seed has been explicitly set, returns a default of 1000.</para>
    /// </remarks>
    internal sealed class GetAutoNumberSeedRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "GetAutoNumberSeed", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var entityName = (string)request["EntityName"];
            var attributeName = (string)request["AttributeName"];

            var seed = AutoNumberSeedStore.GetOrDefault(entityName, attributeName);

            var response = new OrganizationResponse { ResponseName = "GetAutoNumberSeed" };
            response.Results["AutoNumberSeedValue"] = seed;
            return response;
        }
    }
}
