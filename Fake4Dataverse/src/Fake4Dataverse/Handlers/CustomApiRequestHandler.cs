using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="OrganizationRequest"/> messages by matching on <see cref="OrganizationRequest.RequestName"/>.
    /// Used to simulate custom API endpoints.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full for custom-registered behaviors</para>
    /// <para>The delegate passed to the constructor is called with the request and service. Unregistered request names are not handled.</para>
    /// <para><strong>Configuration:</strong> None — the delegate is invoked unconditionally.</para>
    /// </remarks>
    internal sealed class CustomApiRequestHandler : IOrganizationRequestHandler
    {
        private readonly string _requestName;
        private readonly Func<OrganizationRequest, IOrganizationService, OrganizationResponse> _handler;

        public CustomApiRequestHandler(string requestName, Func<OrganizationRequest, IOrganizationService, OrganizationResponse> handler)
        {
            _requestName = requestName ?? throw new ArgumentNullException(nameof(requestName));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public bool CanHandle(OrganizationRequest request)
            => string.Equals(request.RequestName, _requestName, StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
            => _handler(request, service);
    }
}
