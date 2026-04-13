using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>SendTemplateRequest</c> by creating an email from a template and marking it as sent.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Creates an <c>email</c> entity from the template, performs token substitution against the regarding record, materializes recipient and sender activity parties, and marks the email as sent.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> — state/status update applies when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class SendTemplateRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "SendTemplate", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var sendRequest = OrganizationRequestTypeAdapter.AsTyped<SendTemplateRequest>(request);
            var email = TemplateEmailHelper.BuildEmailFromTemplate(
                service,
                sendRequest.TemplateId,
                sendRequest.RegardingType,
                sendRequest.RegardingId,
                null,
                sendRequest.RecipientIds,
                sendRequest.RecipientType,
                sendRequest.Sender);

            if (sendRequest.DeliveryPriorityCode != null)
                email["deliveryprioritycode"] = sendRequest.DeliveryPriorityCode;

            TemplateEmailHelper.MarkAsSent(email);
            service.Create(email);

            return new SendTemplateResponse();
        }
    }
}
