using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="WhoAmIRequest"/> by returning identity information from the
    /// calling <see cref="FakeOrganizationService"/> session.
    /// When the <see cref="UserId"/>, <see cref="OrganizationId"/>, or <see cref="BusinessUnitId"/>
    /// properties are explicitly set, those values take precedence over the session defaults.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Returns <c>UserId</c>, <c>OrganizationId</c>, and <c>BusinessUnitId</c> sourced from the current session or the handler's override properties.</para>
    /// <para><strong>Configuration:</strong> None — reads directly from the <see cref="FakeOrganizationService"/> session properties.</para>
    /// </remarks>
    internal sealed class WhoAmIRequestHandler : IOrganizationRequestHandler
    {
        /// <summary>
        /// Explicitly configured user id. When <c>null</c>, the value is read from the calling
        /// <see cref="FakeOrganizationService.CallerId"/>.
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Explicitly configured organization id. When <c>null</c>, the value is read from
        /// <see cref="FakeDataverseEnvironment.OrganizationId"/>.
        /// </summary>
        public Guid? OrganizationId { get; set; }

        /// <summary>
        /// Explicitly configured business unit id. When <c>null</c>, the value is read from
        /// <see cref="FakeOrganizationService.BusinessUnitId"/>.
        /// </summary>
        public Guid? BusinessUnitId { get; set; }

        /// <inheritdoc />
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "WhoAmI", System.StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc />
        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var response = new WhoAmIResponse();

            if (service is FakeOrganizationService fake)
            {
                response.Results["UserId"] = UserId ?? fake.CallerId;
                response.Results["OrganizationId"] = OrganizationId ?? fake.Environment.OrganizationId;
                response.Results["BusinessUnitId"] = BusinessUnitId ?? fake.BusinessUnitId;
            }
            else
            {
                response.Results["UserId"] = UserId ?? new Guid("00000000-0000-0000-0000-000000000001");
                response.Results["OrganizationId"] = OrganizationId ?? new Guid("00000000-0000-0000-0000-000000000002");
                response.Results["BusinessUnitId"] = BusinessUnitId ?? new Guid("00000000-0000-0000-0000-000000000003");
            }

            return response;
        }
    }
}
