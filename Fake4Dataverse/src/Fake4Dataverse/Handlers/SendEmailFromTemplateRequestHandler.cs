using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>SendEmailFromTemplateRequest</c> by creating an email activity record pre-populated with the specified template and recipient information.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Creates an <c>email</c> activity from the template, performs token substitution against the regarding record, preserves attributes from <c>Target</c>, and marks the resulting email as sent.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetTimestamps"/>, <see cref="FakeOrganizationServiceOptions.AutoSetOwner"/> — apply to the created email entity.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class SendEmailFromTemplateRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "SendEmailFromTemplate", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var sendRequest = OrganizationRequestTypeAdapter.AsTyped<SendEmailFromTemplateRequest>(request);
            var email = TemplateEmailHelper.BuildEmailFromTemplate(
                service,
                sendRequest.TemplateId,
                sendRequest.RegardingType,
                sendRequest.RegardingId,
                sendRequest.Target,
                null,
                null,
                null);
            TemplateEmailHelper.MarkAsSent(email);
            var emailId = service.Create(email);

            var response = new SendEmailFromTemplateResponse();
            response["Id"] = emailId;
            return response;
        }
    }
}
