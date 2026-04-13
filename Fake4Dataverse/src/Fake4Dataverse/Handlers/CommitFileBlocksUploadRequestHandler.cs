using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CommitFileBlocksUploadRequest"/> by assembling uploaded blocks
    /// and storing them as binary attribute data.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Assembles uploaded byte blocks and stores the result as a byte array attribute on the target record. Real Dataverse stores binary data in Azure Blob Storage; the fake stores it directly in the entity attribute.</para>
    /// <para><strong>Configuration:</strong> None — file upload sessions are managed unconditionally via <see cref="FakeDataverseEnvironment"/>.</para>
    /// </remarks>
    internal sealed class CommitFileBlocksUploadRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CommitFileBlocksUpload", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var commitRequest = OrganizationRequestTypeAdapter.AsTyped<CommitFileBlocksUploadRequest>(request);
            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("CommitFileBlocksUploadRequestHandler requires FakeOrganizationService.");

            var token = commitRequest.FileContinuationToken;
            var fileName = commitRequest.FileName;

            fakeService.Environment.CommitUploadSession(token, fileName);

            var response = new CommitFileBlocksUploadResponse();
            response.Results["FileSizeInBytes"] = fakeService.Environment.GetCommittedFileSize(token);
            return response;
        }
    }
}
