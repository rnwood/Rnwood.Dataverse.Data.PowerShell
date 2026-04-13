using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="InitializeFileBlocksDownloadRequest"/> by looking up binary data size for a file attribute.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Retrieves the binary data stored on the target entity's file attribute to determine the file size. Returns a continuation token for subsequent <c>DownloadBlock</c> calls. Real Dataverse streams from Azure Blob Storage; the fake reads directly from the in-memory store.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class InitializeFileBlocksDownloadRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "InitializeFileBlocksDownload", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var downloadRequest = OrganizationRequestTypeAdapter.AsTyped<InitializeFileBlocksDownloadRequest>(request);
            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("InitializeFileBlocksDownloadRequestHandler requires FakeOrganizationService.");

            var target = downloadRequest.Target;
            var attributeName = downloadRequest.FileAttributeName;

            var data = fakeService.Environment.GetBinaryAttribute(target.LogicalName, target.Id, attributeName);
            var token = Guid.NewGuid().ToString("N");

            var response = new InitializeFileBlocksDownloadResponse();
            response.Results["FileName"] = attributeName;
            response.Results["FileSizeInBytes"] = data?.Length ?? 0;
            response.Results["FileContinuationToken"] = token;
            return response;
        }
    }
}
