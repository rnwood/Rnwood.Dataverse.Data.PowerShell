using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="CalculateRollupFieldRequest"/> by triggering rollup field calculation
    /// via the <see cref="CalculatedFieldManager"/>.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Triggers rollup recalculation via <see cref="CalculatedFieldManager"/>; only fields registered via <c>env.RegisterRollupField(...)</c> are recalculated. Real Dataverse recalculates system-managed rollup columns.</para>
    /// <para><strong>Configuration:</strong> None — rollup fields must be explicitly registered on <see cref="FakeDataverseEnvironment"/>.</para>
    /// </remarks>
    internal sealed class CalculateRollupFieldRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "CalculateRollupField", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var calcRequest = OrganizationRequestTypeAdapter.AsTyped<CalculateRollupFieldRequest>(request);
            var target = calcRequest.Target;
            var fieldName = calcRequest.FieldName;

            // Retrieve the entity to apply rollup calculation
            var entity = service.Retrieve(target.LogicalName, target.Id, new ColumnSet(true));

            if (service is FakeOrganizationService fakeService && fakeService.Environment.CalculatedFields.HasFields)
            {
                fakeService.Environment.CalculatedFields.ApplyCalculatedFields(entity, fakeService.Environment.Store);
            }

            var response = new CalculateRollupFieldResponse();
            response.Results["Entity"] = entity;
            return response;
        }
    }
}
