using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="ExecuteAsyncRequest"/> by executing the inner request and returning a completed async job record.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>The inner request is executed immediately and represented by a completed <c>asyncoperation</c> record whose ID is returned via <c>AsyncJobId</c>.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class ExecuteAsyncRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "ExecuteAsync", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var asyncRequest = OrganizationRequestTypeAdapter.AsTyped<ExecuteAsyncRequest>(request);

            // Execute the inner request synchronously in the fake
            if (asyncRequest.Request != null)
            {
                service.Execute(asyncRequest.Request);
            }

            var asyncJobId = AsyncOperationHelper.CreateCompletedAsyncOperation(
                service,
                "Execute Async",
                asyncRequest.Request?.RequestName ?? "ExecuteAsync");

            var response = new ExecuteAsyncResponse();
            response.Results["AsyncJobId"] = asyncJobId;
            return response;
        }
    }
}
