using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="InitializeFileBlocksUploadRequest"/> by creating an in-memory upload session.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Creates an in-memory upload session keyed by a new <see cref="Guid"/> token; subsequent <c>UploadBlockRequest</c> calls append data blocks to the session.</para>
    /// <para><strong>Configuration:</strong> None — upload sessions are managed unconditionally.</para>
    /// </remarks>
    internal sealed class InitializeFileBlocksUploadRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "InitializeFileBlocksUpload", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var initRequest = OrganizationRequestTypeAdapter.AsTyped<InitializeFileBlocksUploadRequest>(request);
            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("InitializeFileBlocksUploadRequestHandler requires FakeOrganizationService.");

            var target = initRequest.Target;
            var attributeName = initRequest.FileAttributeName;

            var token = Guid.NewGuid().ToString("N");
            fakeService.Environment.CreateUploadSession(token, target.LogicalName, target.Id, attributeName);

            var response = new InitializeFileBlocksUploadResponse();
            response.Results["FileContinuationToken"] = token;
            return response;
        }
    }
}
