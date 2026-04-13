using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="UploadBlockRequest"/> by appending data to an in-memory upload session.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Appends the raw byte block to the in-memory upload session identified by the <c>FileContinuationToken</c>. Subsequent <c>CommitFileBlocksUploadRequest</c> assembles and stores the complete file.</para>
    /// <para><strong>Configuration:</strong> None — block upload is managed unconditionally.</para>
    /// </remarks>
    internal sealed class UploadBlockRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "UploadBlock", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var uploadRequest = OrganizationRequestTypeAdapter.AsTyped<UploadBlockRequest>(request);
            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("UploadBlockRequestHandler requires FakeOrganizationService.");

            var token = uploadRequest.FileContinuationToken;
            var blockData = uploadRequest.BlockData;

            fakeService.Environment.AppendUploadBlock(token, blockData);

            return new UploadBlockResponse();
        }
    }
}
