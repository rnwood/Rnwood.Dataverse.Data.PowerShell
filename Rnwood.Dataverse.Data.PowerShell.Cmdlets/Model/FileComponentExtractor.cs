using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
    /// <summary>
    /// Extracts solution components from a solution file.
    /// </summary>
    public class FileComponentExtractor : IComponentExtractor
    {
        private readonly ServiceClient _connection;
        private readonly PSCmdlet _cmdlet;
        private readonly byte[] _solutionBytes;

        /// <summary>
        /// Initializes a new instance of the FileComponentExtractor class.
        /// </summary>
        public FileComponentExtractor(ServiceClient connection, PSCmdlet cmdlet, byte[] solutionBytes)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _cmdlet = cmdlet ?? throw new ArgumentNullException(nameof(cmdlet));
            _solutionBytes = solutionBytes ?? throw new ArgumentNullException(nameof(solutionBytes));
        }

        /// <summary>
        /// Gets a value indicating whether the solution is managed. Always returns null for files.
        /// </summary>
        public bool? IsManagedSolution => null;

        /// <summary>
        /// Gets the components from the solution file.
        /// </summary>
        public List<SolutionComponent> GetComponents(bool includeSubcomponents)
        {
            var components = ExtractRootComponents();

            if (includeSubcomponents)
            {
                components = ExpandWithSubcomponents(components);
            }

            return components;
        }

        /// <summary>
        /// Extracts root components from the solution file.
        /// </summary>
        private List<SolutionComponent> ExtractRootComponents()
        {
            var components = new List<SolutionComponent>();
            XDocument customizationsXdoc = null;

            using (var memoryStream = new MemoryStream(_solutionBytes))
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                // Load customizations.xml for later use
                var customizationsEntry = archive.Entries.FirstOrDefault(e =>
      e.FullName.Equals("customizations.xml", StringComparison.OrdinalIgnoreCase));

                if (customizationsEntry != null)
                {
                    using (var stream = customizationsEntry.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        var xmlContent = reader.ReadToEnd();
                        customizationsXdoc = XDocument.Parse(xmlContent);
                    }
                }

                // Get the solution unique name and root components from solution.xml
                var solutionXmlEntry = archive.Entries.FirstOrDefault(e =>
                   e.FullName.Equals("solution.xml", StringComparison.OrdinalIgnoreCase));

                if (solutionXmlEntry != null)
                {
                    using (var stream = solutionXmlEntry.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        var xmlContent = reader.ReadToEnd();
                        var xdoc = XDocument.Parse(xmlContent);
                        var solutionManifest = xdoc.Root?.Element("SolutionManifest");

                        // Parse root components from solution.xml
                        var rootComponents = solutionManifest?.Element("RootComponents");
                        if (rootComponents != null && customizationsXdoc != null)
                        {
                            foreach (var rootComponent in rootComponents.Elements())
                            {
                                if (rootComponent.Name.LocalName == "RootComponent")
                                {
                                    var idAttr = rootComponent.Attribute("id");
                                    var schemaNameAttr = rootComponent.Attribute("schemaName");
                                    var typeAttr = rootComponent.Attribute("type");
                                    var behaviorAttr = rootComponent.Attribute("behavior");

                                    string componentId = null;
                                    Guid? componentMetadataId = null;
                                    int? componentType = null;

                                    if (idAttr != null)
                                    {
                                        componentId = idAttr.Value;
                                        if (Guid.TryParse(componentId, out var parsedGuid))
                                        {
                                            componentMetadataId = parsedGuid;
                                        }
                                    }
                                    else if (schemaNameAttr != null && typeAttr != null)
                                    {
                                        componentType = int.Parse(typeAttr.Value);
                                        if (componentType == 1) // Entity
                                        {
                                            componentId = schemaNameAttr.Value;
                                            componentMetadataId = FindEntityMetadataIdBySchemaName(customizationsXdoc, schemaNameAttr.Value);
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(componentId) && typeAttr != null)
                                    {
                                        var component = new SolutionComponent
                                        {
                                            UniqueName = componentType == 1 ? componentId : null, // Entity
                                            ObjectId = componentMetadataId,
                                            ComponentType = int.Parse(typeAttr.Value),
                                            ComponentTypeName = null, // Placeholder, will be populated from msdyn_componenttypename if available
                                            RootComponentBehavior = behaviorAttr != null
                                        ? int.Parse(behaviorAttr.Value)
                                                 : 0
                                        };
                                        components.Add(component);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return components;
        }

        /// <summary>
        /// Expands components with their subcomponents.
        /// </summary>
        private List<SolutionComponent> ExpandWithSubcomponents(List<SolutionComponent> components)
        {
            var expandedComponents = new List<SolutionComponent>();

            foreach (var component in components)
            {
                // Always add the root component itself
                expandedComponents.Add(new SolutionComponent
                {
                    UniqueName = component.UniqueName,
                    ObjectId = component.ObjectId,
                    ComponentType = component.ComponentType,
                    ComponentTypeName = component.ComponentTypeName,
                    RootComponentBehavior = component.RootComponentBehavior
                });

                var subcomponents = GetSubcomponents(component);
                expandedComponents.AddRange(subcomponents);
            }

            return expandedComponents;
        }

        /// <summary>
        /// Gets the subcomponents for a specific parent component.
        /// </summary>
        public List<SolutionComponent> GetSubcomponents(SolutionComponent parentComponent)
        {
            return ExtractSubcomponentsFromFile(parentComponent);
        }

        /// <summary>
        /// Extracts subcomponents from a solution file.
        /// </summary>
        private List<SolutionComponent> ExtractSubcomponentsFromFile(SolutionComponent parentComponent)
        {
            var subcomponents = new List<SolutionComponent>();

            if (string.IsNullOrEmpty(parentComponent.UniqueName))
            {
                return subcomponents;
            }

            using (var memoryStream = new MemoryStream(_solutionBytes))
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                var customizationsEntry = archive.Entries.FirstOrDefault(e =>
                        e.FullName.Equals("customizations.xml", StringComparison.OrdinalIgnoreCase));

                if (customizationsEntry != null)
                {
                    using (var stream = customizationsEntry.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        var xmlContent = reader.ReadToEnd();
                        var xdoc = XDocument.Parse(xmlContent);
                        subcomponents.AddRange(ExtractEntitySubcomponentsFromXml(xdoc, parentComponent.UniqueName));
                    }
                }

                // Extract appactions
                var appactionEntries = archive.Entries.Where(e => e.FullName.StartsWith("appactions/", StringComparison.OrdinalIgnoreCase));
                foreach (var entry in appactionEntries)
                {
                    using (var stream = entry.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        var xmlContent = reader.ReadToEnd();
                        var xdocApp = XDocument.Parse(xmlContent);
                        var appaction = xdocApp.Root;

                        var uniquename = appaction.Attribute("uniquename")?.Value;

                        var parts = uniquename.Split('!');
                        if (parts.Length >= 2 && parts[1].Equals(parentComponent.UniqueName, StringComparison.OrdinalIgnoreCase))
                        {
                            subcomponents.Add(new SolutionComponent
                            {
                                ComponentType = 10192,
                                UniqueName = uniquename,
                                RootComponentBehavior = 0,
                                IsSubcomponent = true,
                                ParentComponentType = 1,
                                ParentTableName = parentComponent.UniqueName
                            });
                        }
                    }
                }
            }

            return subcomponents;
        }

        /// <summary>
        /// Extracts entity subcomponents from customizations XML.
        /// </summary>
        private List<SolutionComponent> ExtractEntitySubcomponentsFromXml(XDocument xdoc, string entityName)
        {
            var subcomponents = new List<SolutionComponent>();

            var entities = xdoc.Root.Element("Entities")?.Elements("Entity") ?? Enumerable.Empty<XElement>();

            XElement targetEntity = null;
            foreach (var entity in entities)
            {
                var nameElement = entity.Elements().FirstOrDefault(e => e.Name.LocalName == "Name");
                if (nameElement?.Value.Equals(entityName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    targetEntity = entity;
                    break;
                }
            }

            if (targetEntity == null)
            {
                _cmdlet.WriteVerbose($"Entity with name {entityName} not found in customizations.xml");
                return subcomponents;
            }

            _cmdlet.WriteVerbose($"Found entity name for {entityName} in customizations.xml");

            // Extract attributes
            var attributes = targetEntity.Element("EntityInfo")?.Element("entity").Element("attributes")?.Elements("attribute") ?? Enumerable.Empty<XElement>();
            foreach (var attribute in attributes)
            {
                subcomponents.Add(new SolutionComponent
                {
                    ComponentType = 2, // Attribute
                    UniqueName = attribute.Element("Name")?.Value,
                    RootComponentBehavior = 0,
                    IsSubcomponent = true,
                    ParentComponentType = 1,
                    ParentTableName = entityName
                });
            }

            // Extract messages
            var messages = targetEntity.Element("Strings")?.Elements("Strings") ?? Enumerable.Empty<XElement>();
            foreach (var message in messages)
            {
                subcomponents.Add(new SolutionComponent
                {
                    ComponentType = 22, // Display String
                    UniqueName = message.Attribute("ResourceKey")?.Value,
                    RootComponentBehavior = 0,
                    IsSubcomponent = true,
                    ParentComponentType = 1,
                    ParentTableName = entityName
                });
            }

            // Extract relationships
            var relationships = xdoc.Root.Element("EntityRelationships")?.Elements("EntityRelationship") ?? Enumerable.Empty<XElement>();
            foreach (var rel in relationships)
            {
                var referencingEntity = rel.Element("ReferencingEntityName")?.Value;
                if (referencingEntity?.Equals(entityName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    var name = rel.Attribute("Name")?.Value;
                    subcomponents.Add(new SolutionComponent
                    {
                        ComponentType = 10, // EntityRelationship
                        UniqueName = name,
                        RootComponentBehavior = 0,
                        IsSubcomponent = true,
                        ParentComponentType = 1,
                        ParentTableName = entityName
                    });
                }

                var referencedEntity = rel.Element("ReferencedEntityName")?.Value;
                if (referencedEntity != referencingEntity && referencedEntity?.Equals(entityName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    var name = rel.Attribute("Name")?.Value;
                    subcomponents.Add(new SolutionComponent
                    {
                        ComponentType = 10, // EntityRelationship
                        UniqueName = name,
                        RootComponentBehavior = 0,
                        IsSubcomponent = true,
                        ParentComponentType = 1,
                        ParentTableName = entityName
                    });
                }
            }

            // Extract entity keys
            var entityKeys = targetEntity.Element("EntityInfo")?.Element("entity").Element("EntityKeys")?.Elements("EntityKey") ?? Enumerable.Empty<XElement>();
            foreach (var key in entityKeys)
            {
                var logicalName = key.Element("LogicalName")?.Value;
                subcomponents.Add(new SolutionComponent
                {
                    ComponentType = 14, // EntityKey
                    UniqueName = logicalName,
                    RootComponentBehavior = 0,
                    IsSubcomponent = true,
                    ParentComponentType = 1,
                    ParentTableName = entityName
                });
            }

            // Extract forms
            var forms = targetEntity.Element("FormXml")?.Elements("forms")?.SelectMany(f => f.Elements("systemform")) ?? Enumerable.Empty<XElement>();
            foreach (var form in forms)
            {
                subcomponents.Add(new SolutionComponent
                {
                    ComponentType = 60, // SystemForm
                    ObjectId = (Guid)form.Element("formid"),
                    RootComponentBehavior = 0,
                    IsSubcomponent = true,
                    ParentComponentType = 1,
                    ParentTableName = entityName
                });
            }

            // Extract saved queries
            var savedQueries = targetEntity.Element("SavedQueries")?.Element("savedqueries")?.Elements("savedquery") ?? Enumerable.Empty<XElement>();
            foreach (var query in savedQueries)
            {
                var id = (Guid)query.Element("savedqueryid");
                subcomponents.Add(new SolutionComponent
                {
                    ComponentType = 26, // SavedQuery
                    ObjectId = id,
                    RootComponentBehavior = 0,
                    IsSubcomponent = true,
                    ParentComponentType = 1,
                    ParentTableName = entityName
                });
            }

            // Extract visualizations
            var visualizations = targetEntity.Element("Visualizations")?.Elements("visualization") ?? Enumerable.Empty<XElement>();
            foreach (var viz in visualizations)
            {
                var id = (Guid)viz.Element("savedqueryvisualizationid");
                subcomponents.Add(new SolutionComponent
                {
                    ComponentType = 59, // SavedQueryVisualization
                    ObjectId = id,
                    RootComponentBehavior = 0,
                    IsSubcomponent = true,
                    ParentComponentType = 1,
                    ParentTableName = entityName
                });
            }

            _cmdlet.WriteVerbose($"Found {subcomponents.Count} subcomponents for entity {entityName}");

            return subcomponents;
        }

        /// <summary>
        /// Finds an entity's metadata ID by its schema name in customizations.xml.
        /// </summary>
        private static Guid? FindEntityMetadataIdBySchemaName(XDocument xdoc, string schemaName)
        {
            var entities = xdoc.Descendants()
            .Where(e => e.Name.LocalName == "Entity");

            foreach (var entity in entities)
            {
                var nameElement = entity.Elements().FirstOrDefault(e => e.Name.LocalName == "Name");
                var name = nameElement?.Value;

                if (string.Equals(name, schemaName, StringComparison.OrdinalIgnoreCase))
                {
                    var metadataId = entity.Descendants()
                        .FirstOrDefault(e => e.Name.LocalName == "MetadataId")
                        ?.Value;

                    if (metadataId != null && Guid.TryParse(metadataId, out var parsedId))
                    {
                        return parsedId;
                    }

                    return null;
                }
            }

            return null;
        }
    }
}
