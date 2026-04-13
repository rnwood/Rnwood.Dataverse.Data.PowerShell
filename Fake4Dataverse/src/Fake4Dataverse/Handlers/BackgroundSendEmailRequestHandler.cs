using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>BackgroundSendEmailRequest</c> by marking the email as sent and creating a completed async operation record.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Marks the email activity as sent (same as <c>SendEmailRequest</c>) and returns the ID of a created <c>asyncoperation</c> record representing the completed background job.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> — state/status update applies when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class BackgroundSendEmailRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "BackgroundSendEmail", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var emailId = (Guid)request["EntityId"];

            // Mark the email entity as sent (same as SendEmailRequest)
            var update = new Entity("email", emailId)
            {
                ["statecode"] = new OptionSetValue(1),   // Completed
                ["statuscode"] = new OptionSetValue(3)    // Sent
            };
            service.Update(update);

            var asyncOperationId = AsyncOperationHelper.CreateCompletedAsyncOperation(service, "Background Send Email", "BackgroundSendEmail");

            var response = new OrganizationResponse { ResponseName = "BackgroundSendEmail" };
            response.Results["EntityId"] = asyncOperationId;
            return response;
        }
    }
}
