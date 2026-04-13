using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="UpsertRequest"/>: inserts the entity if it does not exist, updates it if it does.
    /// Supports lookup by primary key (Id) or alternate key (KeyAttributes).
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Checks whether the record exists (by ID or alternate key); calls <c>service.Create(...)</c> if not found, <c>service.Update(...)</c> if found. Sets <c>UpsertResponse.RecordCreated</c> accordingly.</para>
    /// <para><strong>Configuration:</strong> All options apply as they do for <c>CreateRequest</c>/<c>UpdateRequest</c> processing depending on which path is taken.</para>
    /// </remarks>
    internal sealed class UpsertRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "Upsert", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var upsertRequest = OrganizationRequestTypeAdapter.AsTyped<UpsertRequest>(request);
            var target = upsertRequest.Target;

            if (target == null)
                throw DataverseFault.InvalidArgumentFault("UpsertRequest.Target must not be null.");

            var fakeService = service as FakeOrganizationService;
            bool exists = false;
            Guid existingId = target.Id;

            // Try to find by alternate key first
            if (existingId == Guid.Empty && target.KeyAttributes != null && target.KeyAttributes.Count > 0 && fakeService != null)
            {
                try
                {
                    var existing = fakeService.RetrieveByAlternateKey(target.LogicalName, target.KeyAttributes, new ColumnSet(false));
                    existingId = existing.Id;
                    exists = true;
                }
                catch (FaultException<OrganizationServiceFault> ex) when (ex.Detail.ErrorCode == DataverseFault.ObjectDoesNotExist)
                {
                    // Not found — will create
                }
            }
            else if (existingId != Guid.Empty && fakeService != null)
            {
                exists = fakeService.Environment.Store.Exists(target.LogicalName, existingId);
            }

            var response = new UpsertResponse();

            if (exists)
            {
                target.Id = existingId;
                service.Update(target);
                response.Results["RecordCreated"] = false;
                response.Results["Target"] = new EntityReference(target.LogicalName, existingId);
            }
            else
            {
                var id = service.Create(target);
                response.Results["RecordCreated"] = true;
                response.Results["Target"] = new EntityReference(target.LogicalName, id);
            }

            return response;
        }
    }
}
