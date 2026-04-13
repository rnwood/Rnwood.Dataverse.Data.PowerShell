using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles the IsValidStateTransition request by checking registered status transitions.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Consults state/status transition rules registered via <c>env.RegisterStateTransition(...)</c>. When no transitions are registered for an entity, all transitions are considered valid (permissive default).</para>
    /// <para><strong>Configuration:</strong> None — transition rules must be explicitly registered on <see cref="FakeDataverseEnvironment"/>; unconditionally permissive when none are registered.</para>
    /// </remarks>
    internal sealed class IsValidStateTransitionRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "IsValidStateTransition", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var entityRef = (EntityReference)request["Entity"];
            var newState = request["NewState"].ToString()!;
            var newStatus = (int)request["NewStatus"];

            var fakeService = (FakeOrganizationService)service;

            var entity = service.Retrieve(entityRef.LogicalName, entityRef.Id,
                new ColumnSet("statecode", "statuscode"));
            var currentState = entity.GetAttributeValue<OptionSetValue>("statecode")?.Value ?? 0;
            var currentStatus = entity.GetAttributeValue<OptionSetValue>("statuscode")?.Value ?? 1;

            int newStateInt = int.TryParse(newState, out var s) ? s : 0;

            var isValid = fakeService.Environment.IsValidTransition(entityRef.LogicalName, currentState, currentStatus, newStateInt, newStatus);

            var response = new OrganizationResponse { ResponseName = "IsValidStateTransition" };
            response["IsValid"] = isValid;
            return response;
        }
    }
}
