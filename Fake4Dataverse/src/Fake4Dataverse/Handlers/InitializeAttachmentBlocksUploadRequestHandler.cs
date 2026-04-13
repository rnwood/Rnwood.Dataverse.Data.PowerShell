using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="InitializeAttachmentBlocksUploadRequest"/> by creating an in-memory upload session for activity mime attachment records.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Creates an in-memory upload session targeting the <c>body</c> attribute of the activity mime attachment record. Subsequent <c>UploadBlock</c> calls append data blocks to the session.</para>
    /// <para><strong>Configuration:</strong> None — upload sessions are managed unconditionally.</para>
    /// </remarks>
    internal sealed class InitializeAttachmentBlocksUploadRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "InitializeAttachmentBlocksUpload", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var initRequest = OrganizationRequestTypeAdapter.AsTyped<InitializeAttachmentBlocksUploadRequest>(request);
            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("InitializeAttachmentBlocksUploadRequestHandler requires FakeOrganizationService.");

            var target = initRequest.Target;
            var token = Guid.NewGuid().ToString("N");
            fakeService.Environment.CreateUploadSession(token, target.LogicalName, target.Id, "body");

            var response = new InitializeAttachmentBlocksUploadResponse();
            response.Results["FileContinuationToken"] = token;
            return response;
        }
    }
}
