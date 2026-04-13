using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>RouteToRequest</c> by moving a queue item from its current queue to a destination queue.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Creates a new <c>queueitem</c> for the target record in the destination queue and removes any existing <c>queueitem</c> for the record in the source queue. Does not validate queue routing rules or worker assignment.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetTimestamps"/> — applies to the newly created <c>queueitem</c>.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class RouteToRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RouteTo", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var target = (EntityReference)request["Target"];
            var queueId = (Guid)request["QueueId"];

            // Remove from any current queue by finding existing queue item
            if (service is FakeOrganizationService fakeService)
            {
                var existingItems = fakeService.Environment.Store.GetAll("queueitem");
                foreach (var item in existingItems)
                {
                    var objectRef = item.GetAttributeValue<EntityReference>("objectid");
                    if (objectRef != null && objectRef.Id == target.Id
                        && string.Equals(objectRef.LogicalName, target.LogicalName, StringComparison.OrdinalIgnoreCase))
                    {
                        service.Delete("queueitem", item.Id);
                    }
                }
            }

            // Create new queue item in destination queue
            var queueItem = new Entity("queueitem")
            {
                ["objectid"] = target,
                ["queueid"] = new EntityReference("queue", queueId)
            };
            var queueItemId = service.Create(queueItem);

            var response = new OrganizationResponse { ResponseName = "RouteTo" };
            response["QueueItemId"] = queueItemId;
            return response;
        }
    }
}
