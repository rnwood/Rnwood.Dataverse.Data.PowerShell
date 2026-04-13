using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse
{
    /// <summary>
    /// Handles a specific type of <see cref="OrganizationRequest"/>.
    /// </summary>
    public interface IOrganizationRequestHandler
    {
        /// <summary>
        /// Determines whether this handler can process the given request.
        /// </summary>
        bool CanHandle(OrganizationRequest request);

        /// <summary>
        /// Processes the request and returns a response.
        /// </summary>
        OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service);
    }
}
