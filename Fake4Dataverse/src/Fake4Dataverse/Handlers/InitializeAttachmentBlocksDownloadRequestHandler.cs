using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="InitializeAttachmentBlocksDownloadRequest"/> by looking up binary data size for an attachment's body.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Retrieves the binary data stored on the activity mime attachment record's <c>body</c> attribute to determine the file size. Returns a continuation token for subsequent <c>DownloadBlock</c> calls. Real Dataverse streams from Azure Blob Storage; the fake reads directly from the in-memory store.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class InitializeAttachmentBlocksDownloadRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "InitializeAttachmentBlocksDownload", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var downloadRequest = OrganizationRequestTypeAdapter.AsTyped<InitializeAttachmentBlocksDownloadRequest>(request);
            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("InitializeAttachmentBlocksDownloadRequestHandler requires FakeOrganizationService.");

            var target = downloadRequest.Target;
            var data = fakeService.Environment.GetBinaryAttribute(target.LogicalName, target.Id, "body");
            var token = Guid.NewGuid().ToString("N");

            var response = new InitializeAttachmentBlocksDownloadResponse();
            response.Results["FileName"] = "body";
            response.Results["FileSizeInBytes"] = data?.Length ?? 0;
            response.Results["FileContinuationToken"] = token;
            return response;
        }
    }
}
