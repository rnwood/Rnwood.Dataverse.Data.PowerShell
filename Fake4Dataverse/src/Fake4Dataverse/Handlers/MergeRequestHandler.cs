using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles Merge requests by copying update content to the target and deactivating the subordinate.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Applies <c>UpdateContent</c> attributes to the target, then deactivates the subordinate record (sets <c>statecode=1</c>, <c>statuscode=2</c>). Does not replicate child record re-parenting, duplicate detection suppression, or the full Dataverse merge pipeline.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> — used when deactivating the subordinate.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class MergeRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "Merge", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var target = (EntityReference)request["Target"];
            var subordinateId = (Guid)request["SubordinateId"];
            var updateContent = request.Parameters.ContainsKey("UpdateContent") ? (Entity)request["UpdateContent"] : null;

            // Apply update content to target if provided
            if (updateContent != null)
            {
                updateContent.Id = target.Id;
                updateContent.LogicalName = target.LogicalName;
                service.Update(updateContent);
            }

            // Deactivate the subordinate
            var deactivate = new Entity(target.LogicalName, subordinateId);
            deactivate["statecode"] = new OptionSetValue(1); // Inactive
            deactivate["statuscode"] = new OptionSetValue(2); // Deactivated/Merged
            service.Update(deactivate);

            return new OrganizationResponse { ResponseName = "Merge" };
        }
    }
}
