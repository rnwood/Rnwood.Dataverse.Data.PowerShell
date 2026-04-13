using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>UpsertMultipleRequest</c> by delegating each entity to an individual <see cref="UpsertRequest"/>.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Iterates <c>Targets</c> and dispatches each as an <see cref="UpsertRequest"/>; supports alternate-key upsert per record.</para>
    /// <para><strong>Configuration:</strong> All options apply per record, identical to <see cref="UpsertRequest"/> processing.</para>
    /// </remarks>
    internal sealed class UpsertMultipleRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "UpsertMultiple", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var targets = (EntityCollection)request["Targets"];
            var responses = new OrganizationResponseCollection();

            foreach (var target in targets.Entities)
            {
                var upsertReq = new UpsertRequest { Target = target };
                var resp = (UpsertResponse)service.Execute(upsertReq);
                responses.Add(resp);
            }

            var response = new OrganizationResponse { ResponseName = "UpsertMultiple" };
            response["Results"] = responses;
            return response;
        }
    }
}
