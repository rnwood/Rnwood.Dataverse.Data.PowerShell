using System.Collections.Generic;
using System.Linq;
using Fake4Dataverse.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="OrderOptionRequest"/> by reordering the options within a global option set according to the specified order array.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Partial</para>
    /// <para>Reorders the options list in the in-memory metadata store; does not validate that all existing option values are included in the order array.</para>
    /// <para><strong>Configuration:</strong> None — metadata update is unconditional.</para>
    /// </remarks>
    internal sealed class OrderOptionRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "OrderOption", System.StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var orderRequest = OrganizationRequestTypeAdapter.AsTyped<OrderOptionRequest>(request);
            var fakeService = (FakeOrganizationService)service;
            var store = fakeService.Environment.MetadataStore;

            if (!string.IsNullOrEmpty(orderRequest.OptionSetName) && orderRequest.Values != null)
            {
                var optionSet = store.GetGlobalOptionSet(orderRequest.OptionSetName);
                if (optionSet != null)
                {
                    var ordered = new List<OptionInfo>();
                    foreach (var val in orderRequest.Values)
                    {
                        var opt = optionSet.Options.FirstOrDefault(o => o.Value == val);
                        if (opt != null)
                            ordered.Add(opt);
                    }
                    // Add any remaining options not in the order list
                    foreach (var opt in optionSet.Options)
                    {
                        if (!ordered.Contains(opt))
                            ordered.Add(opt);
                    }
                    optionSet.Options.Clear();
                    optionSet.Options.AddRange(ordered);
                }
            }

            store.IncrementMetadataTimestamp();

            return new OrderOptionResponse();
        }
    }
}
