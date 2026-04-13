using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles InstantiateTemplate requests by creating an email entity based on a template.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Full</para>
    /// <para>Creates an <c>email</c> entity pre-populated from the template and performs token substitution for common <c>{!entity:attribute;}</c> placeholders against the regarding record.</para>
    /// <para><strong>Configuration:</strong></para>
    /// <list type="bullet">
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetTimestamps"/> — applies to the created email entity.</description></item>
    ///   <item><description><see cref="FakeOrganizationServiceOptions.AutoSetOwner"/> — applies to the created email entity.</description></item>
    /// </list>
    /// </remarks>
    internal sealed class InstantiateTemplateRequestHandler : IOrganizationRequestHandler
    {
        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "InstantiateTemplate", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            var instantiateRequest = OrganizationRequestTypeAdapter.AsTyped<InstantiateTemplateRequest>(request);
            var email = TemplateEmailHelper.BuildEmailFromTemplate(
                service,
                instantiateRequest.TemplateId,
                instantiateRequest.ObjectType,
                instantiateRequest.ObjectId,
                null,
                null,
                null,
                null);

            var collection = new EntityCollection();
            collection.Entities.Add(email);

            var response = new InstantiateTemplateResponse();
            response["EntityCollection"] = collection;
            return response;
        }
    }
}
