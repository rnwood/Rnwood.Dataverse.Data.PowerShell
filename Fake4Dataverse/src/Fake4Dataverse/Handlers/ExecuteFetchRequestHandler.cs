using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="ExecuteFetchRequest"/> by executing the supplied FetchXml against the in-memory store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Executes the FetchXml using the fake service query pipeline and returns a deterministic simplified XML result string. The serialized XML shape is intentionally lighter than real Dataverse <c>ExecuteFetch</c> output.</para>
    /// <para><strong>Configuration:</strong> Any query-related environment settings (such as security filtering when enabled) apply because execution is delegated through <see cref="IOrganizationService.RetrieveMultiple"/>.</para>
    /// </remarks>
    internal sealed class ExecuteFetchRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "ExecuteFetch", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var executeRequest = OrganizationRequestTypeAdapter.AsTyped<ExecuteFetchRequest>(request);
            var fetchXmlResult = FetchExecutionHelper.ExecuteAndSerialize(service, executeRequest.FetchXml);

            var response = new ExecuteFetchResponse();
            response.Results["FetchXmlResult"] = fetchXmlResult;
            return response;
        }
    }
}