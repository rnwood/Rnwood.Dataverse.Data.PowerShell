using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="SetStateRequest"/> by updating statecode and statuscode on the target entity.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Sets <c>statecode</c> and <c>statuscode</c> on the target record; validates the transition against rules registered via <c>env.RegisterStateTransition(...)</c>. When no transitions are registered, all transitions are permitted (permissive default).</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetVersionNumber"/> — <c>versionnumber</c> is incremented on the underlying update when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class SetStateRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "SetState", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var setStateRequest = OrganizationRequestTypeAdapter.AsTyped<SetStateRequest>(request);
            var target = setStateRequest.EntityMoniker;

            if (service is FakeOrganizationService fakeService)
            {
                var current = service.Retrieve(target.LogicalName, target.Id, new ColumnSet("statecode", "statuscode"));
                var fromState = current.GetAttributeValue<OptionSetValue>("statecode")?.Value ?? 0;
                var fromStatus = current.GetAttributeValue<OptionSetValue>("statuscode")?.Value ?? 1;
                var toState = setStateRequest.State.Value;
                var toStatus = setStateRequest.Status.Value;

                if (!fakeService.Environment.IsValidTransition(target.LogicalName, fromState, fromStatus, toState, toStatus))
                {
                    throw new FaultException<OrganizationServiceFault>(
                        new OrganizationServiceFault { Message = $"The status transition from state {fromState}/status {fromStatus} to state {toState}/status {toStatus} is not valid for entity '{target.LogicalName}'." },
                        new FaultReason($"The status transition from state {fromState}/status {fromStatus} to state {toState}/status {toStatus} is not valid for entity '{target.LogicalName}'."));
                }
            }

            var update = new Entity(target.LogicalName, target.Id)
            {
                ["statecode"] = setStateRequest.State,
                ["statuscode"] = setStateRequest.Status
            };
            service.Update(update);
            return new SetStateResponse();
        }
    }
}
