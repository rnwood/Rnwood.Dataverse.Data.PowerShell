using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Handlers
{
    /// <summary>
    /// Handles <see cref="Microsoft.Crm.Sdk.Messages.PublishXmlRequest"/> and <c>PublishAllXmlRequest</c>.
    /// </summary>
    /// <remarks>
    /// <para><strong>Fidelity:</strong> Functional</para>
    /// <para>When solution-aware entities are registered, <c>PublishAllXmlRequest</c> publishes all
    /// unpublished records across all solution-aware entity types. <c>PublishXmlRequest</c> parses
    /// the <c>ParameterXml</c> to determine which entity types to publish. When no solution-aware
    /// entities are registered, both messages are no-ops (backward compatible).</para>
    /// <para><strong>Configuration:</strong> Requires solution-aware entity registration via
    /// <see cref="FakeDataverseEnvironment.RegisterSolutionAwareEntity"/>.</para>
    /// </remarks>
    internal sealed class PublishXmlRequestHandler : IOrganizationRequestHandler
    {
        private readonly UnpublishedRecordStore _unpublishedStore;
        private readonly InMemoryEntityStore _publishedStore;

        internal PublishXmlRequestHandler(UnpublishedRecordStore unpublishedStore, InMemoryEntityStore publishedStore)
        {
            _unpublishedStore = unpublishedStore;
            _publishedStore = publishedStore;
        }

        public bool CanHandle(OrganizationRequest request) =>
            string.Equals(request.RequestName, "PublishXml", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(request.RequestName, "PublishAllXml", StringComparison.OrdinalIgnoreCase);

        public OrganizationResponse Handle(OrganizationRequest request, IOrganizationService service)
        {
            if (string.Equals(request.RequestName, "PublishAllXml", StringComparison.OrdinalIgnoreCase))
            {
                _unpublishedStore.PublishAll(_publishedStore);
            }
            else if (request.Parameters.ContainsKey("ParameterXml"))
            {
                var parameterXml = request.Parameters["ParameterXml"] as string;
                if (!string.IsNullOrEmpty(parameterXml))
                {
                    var entityNames = ParseEntityNamesFromXml(parameterXml!);
                    foreach (var entityName in entityNames)
                    {
                        if (_unpublishedStore.IsSolutionAware(entityName))
                            _unpublishedStore.Publish(entityName, _publishedStore);
                    }
                }
            }

            return new OrganizationResponse { ResponseName = request.RequestName };
        }

        private static List<string> ParseEntityNamesFromXml(string xml)
        {
            var entityNames = new List<string>();
            try
            {
                var doc = XDocument.Parse(xml);
                // PublishXml ParameterXml format: <importexportxml><entities><entity>entityname</entity>...</entities>...</importexportxml>
                var entitiesElement = doc.Root?.Element("entities");
                if (entitiesElement != null)
                {
                    foreach (var entityElement in entitiesElement.Elements("entity"))
                    {
                        var name = entityElement.Value?.Trim();
                        if (!string.IsNullOrEmpty(name))
                            entityNames.Add(name!);
                    }
                }
            }
            catch
            {
                // If XML is malformed, treat as no-op (matches lenient Dataverse behavior)
            }
            return entityNames;
        }
    }
}
