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
        /// <param name="connection">Optional connection (can be null for file-to-file comparisons)</param>
        /// <param name="cmdlet">The cmdlet instance</param>
        /// <param name="solutionBytes">The solution file bytes</param>
        public FileComponentExtractor(ServiceClient connection, PSCmdlet cmdlet, byte[] solutionBytes)
        {
            _connection = connection; // Can be null for file-to-file comparisons
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

                                    Guid? componentId = null;
                                    int? componentType = int.Parse(typeAttr.Value); ;

                                    if (idAttr != null)
                                    {
                                        if (Guid.TryParse(idAttr.Value, out var componentIdValue))
                                        {
                                            componentId = componentIdValue;
                                        }
                                    }


                                    var component = new SolutionComponent
                                    {
                                        UniqueName = schemaNameAttr?.Value,
                                        ObjectId = componentId,
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

                // Discover environment variables from separate files in the archive
                // Environment variables (componenttype=380) are not listed in RootComponents
                // but exist as separate files in the environmentvariabledefinitions/ folder
                var envVarEntries = archive.Entries.Where(e =>
                    e.FullName.Contains("environmentvariabledefinitions/") &&
                    e.FullName.EndsWith("environmentvariabledefinition.xml", StringComparison.OrdinalIgnoreCase));

                foreach (var entry in envVarEntries)
                {
                    try
                    {
                        using (var stream = entry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            var xmlContent = reader.ReadToEnd();
                            var xdoc = XDocument.Parse(xmlContent);

                            // Get the schemaname from the root element attribute
                            var root = xdoc.Root;
                            var schemaName = root?.Attribute("schemaname")?.Value;
                            var envVarDefIdStr = root?.Attribute("environmentvariabledefinitionid")?.Value;

                            if (!string.IsNullOrEmpty(schemaName))
                            {
                                Guid? envVarDefId = null;
                                if (!string.IsNullOrEmpty(envVarDefIdStr) && Guid.TryParse(envVarDefIdStr, out var parsedId))
                                {
                                    envVarDefId = parsedId;
                                }

                                components.Add(new SolutionComponent
                                {
                                    UniqueName = schemaName,
                                    ObjectId = envVarDefId,
                                    ComponentType = 380, // Environment Variable Definition
                                    RootComponentBehavior = 0 // Default behavior
                                });

                                _cmdlet.WriteVerbose($"Found environment variable in solution: {schemaName}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _cmdlet.WriteVerbose($"Error parsing environment variable file {entry.FullName}: {ex.Message}");
                    }
                }

                // Discover connection references from customizations.xml
                // Connection references (componenttype=10150) may not be in RootComponents
                // but are stored in customizations.xml
                if (customizationsXdoc != null)
                {
                    var connRefElements = customizationsXdoc.Descendants()
                        .Where(e => e.Name.LocalName == "connectionreference");

                    foreach (var connRef in connRefElements)
                    {
                        var logicalName = connRef.Attribute("connectionreferencelogicalname")?.Value;
                        var connRefIdStr = connRef.Attribute("connectionreferenceid")?.Value;

                        if (!string.IsNullOrEmpty(logicalName))
                        {
                            Guid? connRefId = null;
                            if (!string.IsNullOrEmpty(connRefIdStr) && Guid.TryParse(connRefIdStr, out var parsedId))
                            {
                                connRefId = parsedId;
                            }

                            components.Add(new SolutionComponent
                            {
                                UniqueName = logicalName,
                                ObjectId = connRefId,
                                ComponentType = 10150, // Connection Reference
                                RootComponentBehavior = 0 // Default behavior
                            });

                            _cmdlet.WriteVerbose($"Found connection reference in solution: {logicalName}");
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
        /// Extracts subcomponents from a solution file.
        /// </summary>
        public List<SolutionComponent> GetSubcomponents(SolutionComponent parentComponent)
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
