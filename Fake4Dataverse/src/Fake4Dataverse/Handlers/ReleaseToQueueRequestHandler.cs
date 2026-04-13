using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="ReleaseToQueueRequest"/> by clearing the worker from a queue item.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Updates the <c>queueitem</c> record to set <c>workerid</c> to <c>null</c>, releasing the item back to the queue.</para>
    /// <para><strong>Configuration:</strong> Delegates to <see cref="IOrganizationService.Update"/>, so pipeline and security settings apply.</para>
    /// </remarks>
    internal sealed class ReleaseToQueueRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "ReleaseToQueue", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var releaseRequest = OrganizationRequestTypeAdapter.AsTyped<ReleaseToQueueRequest>(request);

            var update = new Entity("queueitem", releaseRequest.QueueItemId)
            {
                ["workerid"] = null
            };
            service.Update(update);

            return new ReleaseToQueueResponse();
        }
    }
}
