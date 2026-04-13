using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>AddToQueueRequest</c> by creating a <c>queueitem</c> record associating the target record with the queue.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Creates a <c>queueitem</c> entity record associating the target record with the queue. Does not replicate worker assignment logic or queue routing rules.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetTimestamps"/>, <see cref="FakeOrganizationServiceOptions.AutoSetOwner"/> — apply to the created <c>queueitem</c> record.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class AddToQueueRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "AddToQueue", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var target = (EntityReference)request["Target"];
            var destinationQueueId = (Guid)request["DestinationQueueId"];

            // Create a queueitem record
            var queueItem = new Entity("queueitem");
            queueItem["objectid"] = target;
            queueItem["queueid"] = new EntityReference("queue", destinationQueueId);
            var queueItemId = service.Create(queueItem);

            var response = new OrganizationResponse { ResponseName = "AddToQueue" };
            response["QueueItemId"] = queueItemId;
            return response;
        }
    }

    /// <summary>
    /// Handles <c>RemoveFromQueueRequest</c> by deleting the <c>queueitem</c> record.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Deletes the <c>queueitem</c> record. Does not validate that the record is actually currently in the queue before deletion.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RemoveFromQueueRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RemoveFromQueue", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var queueItemId = (Guid)request["QueueItemId"];
            service.Delete("queueitem", queueItemId);
            return new OrganizationResponse { ResponseName = "RemoveFromQueue" };
        }
    }
}
