using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>SendFaxRequest</c> by marking the fax activity as sent.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Marks the fax activity as sent by setting <c>statecode=1</c> (Completed) and <c>statuscode=3</c> (Sent). Does not actually send any fax.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> — state/status update applies when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class SendFaxRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "SendFax", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var sendRequest = OrganizationRequestTypeAdapter.AsTyped<SendFaxRequest>(request);
            var faxId = sendRequest.FaxId;

            if (faxId != Guid.Empty)
            {
                var update = new Entity("fax", faxId)
                {
                    ["statecode"] = new OptionSetValue(1),   // Completed
                    ["statuscode"] = new OptionSetValue(3)    // Sent
                };
                service.Update(update);
            }

            return new SendFaxResponse();
        }
    }
}
