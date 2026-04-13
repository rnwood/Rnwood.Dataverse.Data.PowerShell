using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RetrieveUserQueuesRequest"/> by querying <c>queue</c> records owned by the user.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Returns all <c>queue</c> records whose <c>ownerid</c> matches the specified user ID. Does not evaluate queue membership or delegation rules.</para>
    /// <para><strong>Configuration:</strong> None — behavior is unconditional.</para>
    /// </remarks>
    internal sealed class RetrieveUserQueuesRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveUserQueues", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var retrieveRequest = OrganizationRequestTypeAdapter.AsTyped<RetrieveUserQueuesRequest>(request);
            var userId = retrieveRequest.UserId;

            var fakeService = service as FakeOrganizationService
                ?? throw new InvalidOperationException("RetrieveUserQueuesRequestHandler requires FakeOrganizationService.");

            var allQueues = fakeService.Environment.Store.GetAll("queue");
            var userQueues = new System.Collections.Generic.List<Entity>();
            foreach (var queue in allQueues)
            {
                var owner = queue.GetAttributeValue<EntityReference>("ownerid");
                if (owner != null && owner.Id == userId)
                    userQueues.Add(queue);
            }

            var collection = new EntityCollection(userQueues) { EntityName = "queue" };

            var response = new RetrieveUserQueuesResponse();
            response.Results["EntityCollection"] = collection;
            return response;
        }
    }
}
