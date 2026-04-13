using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles DeleteFile requests by removing binary data from the entity attribute in the binary store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Removes the binary data associated with the specified file attribute from the binary store. If no matching binary exists, the request still succeeds.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class DeleteFileRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "DeleteFile", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            if (service is FakeOrganizationService fakeService)
            {
                var target = request.Parameters.ContainsKey("Target") ? (EntityReference)request["Target"] : null;
                var attributeName = request.Parameters.ContainsKey("FileAttributeName")
                    ? (string)request["FileAttributeName"]
                    : null;

                if (target != null && !string.IsNullOrEmpty(attributeName))
                {
                    fakeService.Environment.RemoveBinaryAttribute(
                        target.LogicalName, target.Id, attributeName!);
                }
            }

            return new OrganizationResponse { ResponseName = "DeleteFile" };
        }
    }
}
