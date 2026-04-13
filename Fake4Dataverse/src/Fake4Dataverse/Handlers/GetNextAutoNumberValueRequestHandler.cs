using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>GetNextAutoNumberValue</c> requests by atomically incrementing and returning the auto-number seed.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Increments the seed value atomically and returns the new value. If no seed has been set, increments from the default of 1000.</para>
    /// </remarks>
    internal sealed class GetNextAutoNumberValueRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "GetNextAutoNumberValue", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var entityName = (string)request["EntityName"];
            var attributeName = (string)request["AttributeName"];

            var nextValue = AutoNumberSeedStore.IncrementAndGet(entityName, attributeName);

            var response = new OrganizationResponse { ResponseName = "GetNextAutoNumberValue" };
            response.Results["AutoNumberValue"] = nextValue;
            return response;
        }
    }
}
