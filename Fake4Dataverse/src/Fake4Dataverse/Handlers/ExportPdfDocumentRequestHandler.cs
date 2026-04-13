using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles ExportPdfDocument requests — exports an entity record as a PDF document.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Stub</para>
    /// <para>Returns an empty byte array. Real Dataverse renders the record using a Word template via Power Automate / document generation pipeline.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class ExportPdfDocumentRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "ExportPdfDocument", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var response = new OrganizationResponse { ResponseName = "ExportPdfDocument" };
            response["PdfFile"] = Array.Empty<byte>();
            return response;
        }
    }
}
