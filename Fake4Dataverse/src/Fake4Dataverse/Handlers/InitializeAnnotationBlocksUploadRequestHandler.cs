using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="InitializeAnnotationBlocksUploadRequest"/> by creating an in-memory upload session for annotation records.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Creates an in-memory upload session targeting the <c>documentbody</c> attribute of the annotation record. Subsequent <c>UploadBlock</c> calls append data blocks to the session.</para>
    /// <para><strong>Configuration:</strong> None — upload sessions are managed unconditionally.</para>
    /// </remarks>
    internal sealed class InitializeAnnotationBlocksUploadRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "InitializeAnnotationBlocksUpload", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var initRequest = OrganizationRequestTypeAdapter.AsTyped<InitializeAnnotationBlocksUploadRequest>(request);
            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("InitializeAnnotationBlocksUploadRequestHandler requires FakeOrganizationService.");

            var target = initRequest.Target;
            var token = Guid.NewGuid().ToString("N");
            fakeService.Environment.CreateUploadSession(token, target.LogicalName, target.Id, "documentbody");

            var response = new InitializeAnnotationBlocksUploadResponse();
            response.Results["FileContinuationToken"] = token;
            return response;
        }
    }
}
