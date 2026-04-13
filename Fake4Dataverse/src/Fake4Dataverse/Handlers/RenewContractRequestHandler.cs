using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="RenewContractRequest"/> by creating a renewed copy of an expired or cancelled contract.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Creates a new <c>contract</c> record by copying attributes from the original contract (if <c>IncludeCanceledLines</c> is respected) and setting it to Draft state. Does not replicate contract line-item renewal or detailed contract lifecycle rules.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> — state/status update applies when <see langword="true"/> (default).</description></item>
    /// </list>
    /// </remarks>
    internal sealed class RenewContractRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "RenewContract", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var renewRequest = OrganizationRequestTypeAdapter.AsTyped<RenewContractRequest>(request);
            var contractId = renewRequest.ContractId;
            var status = renewRequest.Status;

            // Retrieve the original contract
            var original = service.Retrieve("contract", contractId, new ColumnSet(true));

            // Create a renewed copy as a draft
            var renewed = new Entity("contract");
            foreach (var attr in original.Attributes)
            {
                if (string.Equals(attr.Key, "contractid", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (string.Equals(attr.Key, "statecode", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (string.Equals(attr.Key, "statuscode", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (string.Equals(attr.Key, "createdon", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (string.Equals(attr.Key, "modifiedon", StringComparison.OrdinalIgnoreCase))
                    continue;
                renewed[attr.Key] = attr.Value;
            }
            renewed["statecode"] = new OptionSetValue(0); // Draft
            renewed["statuscode"] = new OptionSetValue(status);
            renewed["originatingcontract"] = new EntityReference("contract", contractId);

            var renewedId = service.Create(renewed);

            var response = new RenewContractResponse();
            response.Results["Entity"] = service.Retrieve("contract", renewedId, new ColumnSet(true));
            return response;
        }
    }
}
