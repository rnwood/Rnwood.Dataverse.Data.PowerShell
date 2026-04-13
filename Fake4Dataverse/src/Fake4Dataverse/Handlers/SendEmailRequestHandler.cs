using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="SendEmailRequest"/> by marking the email entity as sent.
    /// The email entity is updated with <c>statecode = 1</c> (Completed) and <c>statuscode = 3</c> (Sent).
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Marks the email activity as sent by setting <c>statecode=1</c> (Completed) and <c>statuscode=3</c> (Sent). Does not actually send any email.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> — state/status update applies when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class SendEmailRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "SendEmail", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var sendRequest = OrganizationRequestTypeAdapter.AsTyped<SendEmailRequest>(request);
            var emailId = sendRequest.EmailId;

            // Mark the email entity as sent
            var update = new Entity("email", emailId)
            {
                ["statecode"] = new OptionSetValue(1),   // Completed
                ["statuscode"] = new OptionSetValue(3)    // Sent
            };
            service.Update(update);

            return new SendEmailResponse();
        }
    }
}
