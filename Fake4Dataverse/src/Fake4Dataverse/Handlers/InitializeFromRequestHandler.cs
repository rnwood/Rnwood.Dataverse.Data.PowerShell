using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="InitializeFromRequest"/> by copying attribute values from a source record
    /// to a new entity of the target entity type.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Copies the attribute collection of the source record to a new unsaved entity of the target type. Field-level mapping rules (<c>AttributeMap</c> definitions) from the metadata store are not applied; all non-system attributes are copied verbatim.</para>
    /// <para><strong>Configuration:</strong> None — attribute copying is unconditional.</para>
    /// </remarks>
    internal sealed class InitializeFromRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "InitializeFrom", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var initRequest = OrganizationRequestTypeAdapter.AsTyped<InitializeFromRequest>(request);
            var entityMoniker = initRequest.EntityMoniker;
            var targetEntityName = initRequest.TargetEntityName;

            // Retrieve the source record with all columns
            var source = service.Retrieve(entityMoniker.LogicalName, entityMoniker.Id, new ColumnSet(true));

            // Create a new entity of the target type and copy attributes
            var target = new Entity(targetEntityName);
            foreach (var attr in source.Attributes)
            {
                // Skip system fields and the source's primary ID
                var key = attr.Key;
                if (key == source.LogicalName + "id" || key == "createdon" || key == "modifiedon"
                    || key == "createdby" || key == "modifiedby" || key == "ownerid"
                    || key == "statecode" || key == "statuscode" || key == "versionnumber")
                    continue;

                target[key] = InMemoryEntityStore.CloneAttributeValue(attr.Value);
            }

            var response = new InitializeFromResponse();
            response.Results["Entity"] = target;
            return response;
        }
    }
}
