using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="PickFromQueueRequest"/> by assigning a worker to a queue item.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Updates the <c>queueitem</c> record to set <c>workerid</c> to the specified <see cref="PickFromQueueRequest.WorkerId"/>.</para>
    /// <para><strong>Configuration:</strong> Delegates to <see cref="IOrganizationService.Update"/>, so pipeline and security settings apply.</para>
    /// </remarks>
    internal sealed class PickFromQueueRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "PickFromQueue", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var pickRequest = OrganizationRequestTypeAdapter.AsTyped<PickFromQueueRequest>(request);

            var update = new Entity("queueitem", pickRequest.QueueItemId)
            {
                ["workerid"] = new EntityReference("systemuser", pickRequest.WorkerId)
            };
            service.Update(update);

            return new PickFromQueueResponse();
        }
    }
}
