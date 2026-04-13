using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="PublishAllXmlAsyncRequest"/>.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Functional</para>
    /// <para>Publishes all unpublished records across all solution-aware entity types,
    /// then returns a structurally valid response. When no solution-aware entities are
    /// registered, behaves as a no-op (backward compatible).</para>
    /// </remarks>
    internal sealed class PublishAllXmlAsyncRequestHandler : IOrganizationRequestHandler
    {
        private readonly UnpublishedRecordStore _unpublishedStore;
        private readonly InMemoryEntityStore _publishedStore;

        internal PublishAllXmlAsyncRequestHandler(UnpublishedRecordStore unpublishedStore, InMemoryEntityStore publishedStore)
        {
            _unpublishedStore = unpublishedStore;
            _publishedStore = publishedStore;
        }

        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "PublishAllXmlAsync", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            _unpublishedStore.PublishAll(_publishedStore);
            return new PublishAllXmlAsyncResponse();
        }
    }
}