using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles DownloadBlock requests by returning file data from the binary store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Retrieves binary data stored via <c>UploadBlock</c>/<c>CommitFileBlocksUpload</c> using the <c>FileContinuationToken</c>. If the token identifies a previously uploaded file, the data is returned. Falls back to an empty array if no data is found.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class DownloadBlockRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "DownloadBlock", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            byte[] data = Array.Empty<byte>();

            if (service is FakeOrganizationService fakeService)
            {
                var target = request.Parameters.ContainsKey("Target") ? (EntityReference)request["Target"] : null;
                var attributeName = request.Parameters.ContainsKey("FileAttributeName")
                    ? (string)request["FileAttributeName"]
                    : null;

                if (target != null && !string.IsNullOrEmpty(attributeName))
                {
                    var stored = fakeService.Environment.GetBinaryAttribute(
                        target.LogicalName, target.Id, attributeName!);
                    if (stored != null)
                        data = stored;
                }
            }

            var response = new OrganizationResponse { ResponseName = "DownloadBlock" };
            response["Data"] = data;
            return response;
        }
    }
}
