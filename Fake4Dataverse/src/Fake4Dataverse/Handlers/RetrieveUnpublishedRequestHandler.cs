using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <c>RetrieveUnpublishedRequest</c> by retrieving a single record from the unpublished store.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Functional</para>
    /// <para>Returns the unpublished (draft) version of a solution-aware record. Only records staged via
    /// create or update on a solution-aware entity type (registered with
    /// <see cref="FakeDataverseEnvironment.RegisterSolutionAwareEntity"/>) are returned.</para>
    /// <para><strong>Configuration:</strong> Requires solution-aware entity registration.</para>
    /// </remarks>
    internal sealed class RetrieveUnpublishedRequestHandler : IOrganizationRequestHandler
    {
        private readonly UnpublishedRecordStore _unpublishedStore;

        internal RetrieveUnpublishedRequestHandler(UnpublishedRecordStore unpublishedStore)
        {
            _unpublishedStore = unpublishedStore;
        }

        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RetrieveUnpublished", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var target = (EntityReference)request.Parameters["Target"];
            var columnSet = request.Parameters.ContainsKey("ColumnSet")
                ? (ColumnSet)request.Parameters["ColumnSet"]
                : new ColumnSet(true);

            var entity = _unpublishedStore.Store.Retrieve(target.LogicalName, target.Id, columnSet);

            var response = new RetrieveUnpublishedResponse();
            response.Results["Entity"] = entity;
            return response;
        }
    }
}
