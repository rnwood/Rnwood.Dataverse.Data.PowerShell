using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="InitializeAnnotationBlocksDownloadRequest"/> by looking up binary data size for an annotation's document body.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Retrieves the binary data stored on the annotation record's <c>documentbody</c> attribute to determine the file size. Returns a continuation token for subsequent <c>DownloadBlock</c> calls. Real Dataverse streams from Azure Blob Storage; the fake reads directly from the in-memory store.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class InitializeAnnotationBlocksDownloadRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "InitializeAnnotationBlocksDownload", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var downloadRequest = OrganizationRequestTypeAdapter.AsTyped<InitializeAnnotationBlocksDownloadRequest>(request);
            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("InitializeAnnotationBlocksDownloadRequestHandler requires FakeOrganizationService.");

            var target = downloadRequest.Target;
            var data = fakeService.Environment.GetBinaryAttribute(target.LogicalName, target.Id, "documentbody");
            var token = Guid.NewGuid().ToString("N");

            var response = new InitializeAnnotationBlocksDownloadResponse();
            response.Results["FileName"] = "documentbody";
            response.Results["FileSizeInBytes"] = data?.Length ?? 0;
            response.Results["FileContinuationToken"] = token;
            return response;
        }
    }
}
