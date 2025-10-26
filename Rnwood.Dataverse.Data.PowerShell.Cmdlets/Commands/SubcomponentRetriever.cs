using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves subcomponents for solution components from either the environment or a solution file.
    /// </summary>
    public class SubcomponentRetriever
    {
        private readonly ServiceClient _connection;
        private readonly PSCmdlet _cmdlet;
        private readonly byte[] _solutionFileBytes;
        private readonly Guid? _solutionId;

        /// <summary>
        /// Initializes a new instance of the SubcomponentRetriever class for environment-based retrieval with solution context.
        /// </summary>
        public SubcomponentRetriever(ServiceClient connection, PSCmdlet cmdlet, Guid? solutionId)
        {
            _connection = connection;
            _cmdlet = cmdlet;
            _solutionFileBytes = null;
            _solutionId = solutionId;
        }

        /// <summary>
        /// Initializes a new instance of the SubcomponentRetriever class for file-based retrieval with solution context.
        /// </summary>
        public SubcomponentRetriever(ServiceClient connection, PSCmdlet cmdlet, byte[] solutionFileBytes, Guid? solutionId)
        {
            _connection = connection;
            _cmdlet = cmdlet;
            _solutionFileBytes = solutionFileBytes;
            _solutionId = solutionId;
        }

        /// <summary>
        /// Retrieves subcomponents for the given parent component.
        /// </summary>
        public List<SolutionComponent> GetSubcomponents(SolutionComponent parentComponent)
        {
            var subcomponents = new List<SolutionComponent>();

            // Only entities have subcomponents
            if (parentComponent.ComponentType != 1)
            {
                return subcomponents;
            }

            var parentIdentifier = parentComponent.UniqueName ?? parentComponent.ObjectId?.ToString() ?? "Unknown";
            _cmdlet.WriteVerbose($"Retrieving subcomponents for entity: {parentIdentifier}");

            // If we have solution file bytes, try to extract from file first
            if (_solutionFileBytes != null)
            {
                var fileSubcomponents = ExtractSubcomponentsFromFile(parentComponent);
                if (fileSubcomponents.Count > 0)
                {
                    return fileSubcomponents;
                }
            }

            // Try to retrieve from msdyn_solutioncomponentsummary via REST API first
            return GetSubcomponentsFromMsdynSummary(parentComponent);
        }

        /// <summary>
        /// Gets subcomponents from msdyn_solutioncomponentsummary using REST API for the given parent component.
        /// </summary>
        private List<SolutionComponent> GetSubcomponentsFromMsdynSummary(SolutionComponent parentComponent)
        {
            var subcomponents = new List<SolutionComponent>();

            try
            {
                var parentIdentifier = parentComponent.UniqueName ?? parentComponent.ObjectId?.ToString() ?? "Unknown";
                _cmdlet.WriteVerbose($"Querying msdyn_solutioncomponentsummary for subcomponents of {parentIdentifier}");

                var subcomponentTypes = new[] { 2, 10, 14, 22, 26, 59, 60, 10192, 29};

                foreach (var componentTypeFilter in subcomponentTypes)
                {
                    // Use REST API to query msdyn_solutioncomponentsummary for child components
                    var filter = $"msdyn_primaryentityname eq '{parentComponent.UniqueName}' and msdyn_componenttype eq {componentTypeFilter}";
                    if (_solutionId.HasValue)
                    {
                        filter += $" and msdyn_solutionid eq {_solutionId:B}";
                    }

                    var select = "msdyn_objectid,msdyn_componenttype,msdyn_uniquename,msdyn_primaryentityname,msdyn_componenttypename,msdyn_schemaname";
                    var queryString = $"$filter={Uri.EscapeDataString(filter)}&$select={select}";

                    string nextLink = null;
                    do
                    {
                        var uri = nextLink ?? $"msdyn_solutioncomponentsummaries?{queryString}";
                        var response = _connection.ExecuteWebRequest(HttpMethod.Get, uri, null, null);

                        response.EnsureSuccessStatusCode();

                        var jsonResponse = response.Content.ReadAsStringAsync().Result;
                        using (var doc = JsonDocument.Parse(jsonResponse))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("value", out var valueArray))
                            {
                                foreach (var item in valueArray.EnumerateArray())
                                {
                                    var objectId = Guid.Parse(item.GetProperty("msdyn_objectid").GetString());
                                    var componentType = item.GetProperty("msdyn_componenttype").GetInt32();

                                    var schemaName = item.TryGetProperty("msdyn_schemaname", out var sn) ? sn.GetString() : null;
                                    var uniqueName = item.TryGetProperty("msdyn_uniquename", out var un) ? un.GetString() : null;
                                    var componentTypeName = item.TryGetProperty("msdyn_componenttypename", out var ctn) ? ctn.GetString() : null;

                                    // Use msdyn_uniquename if available, otherwise use msdyn_primaryentityname
                                    string logicalName = schemaName ?? uniqueName;

                                    subcomponents.Add(new SolutionComponent
                                    {
                                        UniqueName = logicalName,
                                        ObjectId = objectId,
                                        MetadataId = objectId,
                                        ComponentType = componentType,
                                        ComponentTypeName = componentTypeName,
                                        RootComponentBehavior = 0, // Will be updated from solutioncomponent query
                                        IsSubcomponent = true,
                                        ParentComponentType = parentComponent.ComponentType,
                                        ParentTableName = parentComponent.UniqueName
                                    });
                                }
                            }

                            nextLink = root.TryGetProperty("@odata.nextLink", out var nl) ? nl.GetString() : null;
                            if (nextLink != null)
                            {
                                // Extract the query part after the entity set
                                var uriObj = new Uri(nextLink);
                                var pathAndQuery = uriObj.PathAndQuery;
                                var index = pathAndQuery.IndexOf("msdyn_solutioncomponentsummaries");
                                if (index >= 0)
                                {
                                    nextLink = pathAndQuery.Substring(index);
                                }
                                else
                                {
                                    nextLink = null; // Fallback
                                }
                            }
                        }
                    } while (nextLink != null);
                }

                // Query solutioncomponent table to get rootcomponentbehavior for each subcomponent
                if (subcomponents.Count > 0 && _solutionId.HasValue)
                {
                    var solutionComponentQuery = new QueryExpression("solutioncomponent")
                    {
                        ColumnSet = new ColumnSet("objectid", "rootcomponentbehavior"),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression("solutionid", ConditionOperator.Equal, _solutionId.Value)
                            }
                        }
                    };

                    var allSolutionComponents = new List<Entity>();
                    EntityCollection result;
                    do
                    {
                        result = _connection.RetrieveMultiple(solutionComponentQuery);
                        allSolutionComponents.AddRange(result.Entities);
                        if (result.MoreRecords)
                        {
                            solutionComponentQuery.PageInfo.PageNumber++;
                            solutionComponentQuery.PageInfo.PagingCookie = result.PagingCookie;
                        }
                    } while (result.MoreRecords);

                    // Create a lookup dictionary for quick matching by objectid
                    var behaviorLookup = allSolutionComponents
                        .Where(e => e.Contains("rootcomponentbehavior"))
                        .ToDictionary(
                            e => e.GetAttributeValue<Guid>("objectid"),
                            e => e.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value ?? 0
                        );

                    // Update subcomponents with behavior information
                    foreach (var subcomponent in subcomponents)
                    {
                        if (subcomponent.ObjectId.HasValue && behaviorLookup.TryGetValue(subcomponent.ObjectId.Value, out var behavior))
                        {
                            subcomponent.RootComponentBehavior = behavior;
                        }
                    }
                }

                _cmdlet.WriteVerbose($"Found {subcomponents.Count} subcomponents from msdyn_solutioncomponentsummary");

                // Retrieve displaystringkey for component type 22 (Display String)
                var displayStringIds = subcomponents.Where(sc => sc.ComponentType == 22).Select(sc => sc.ObjectId.Value).ToList();
                if (displayStringIds.Any())
                {
                    var idsFilter = string.Join(" or ", displayStringIds.Select(id => $"displaystringid eq {id:B}"));
                    var queryString = $"$filter={Uri.EscapeDataString(idsFilter)}&$select=displaystringid,displaystringkey";

                    string nextLink = null;
                    do
                    {
                        var uri = nextLink ?? $"displaystrings?{queryString}";
                        var response = _connection.ExecuteWebRequest(HttpMethod.Get, uri, null, null);

                        response.EnsureSuccessStatusCode();

                        var jsonResponse = response.Content.ReadAsStringAsync().Result;
                        using (var doc = JsonDocument.Parse(jsonResponse))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("value", out var valueArray))
                            {
                                foreach (var item in valueArray.EnumerateArray())
                                {
                                    var displaystringid = Guid.Parse(item.GetProperty("displaystringid").GetString());
                                    var displaystringkey = item.GetProperty("displaystringkey").GetString();

                                    var subcomponent = subcomponents.FirstOrDefault(sc => sc.ObjectId == displaystringid && sc.ComponentType == 22);
                                    if (subcomponent != null)
                                    {
                                        subcomponent.UniqueName = displaystringkey;
                                    }
                                }
                            }

                            nextLink = root.TryGetProperty("@odata.nextLink", out var nl) ? nl.GetString() : null;
                            if (nextLink != null)
                            {
                                // Extract the query part after the entity set
                                var uriObj = new Uri(nextLink);
                                var pathAndQuery = uriObj.PathAndQuery;
                                var index = pathAndQuery.IndexOf("displaystrings");
                                if (index >= 0)
                                {
                                    nextLink = pathAndQuery.Substring(index);
                                }
                                else
                                {
                                    nextLink = null; // Fallback
                                }
                            }
                        }
                    } while (nextLink != null);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error querying msdyn_solutioncomponentsummary: {ex.Message}", ex);
            }

            return subcomponents;
        }

        private List<SolutionComponent> ExtractSubcomponentsFromFile(SolutionComponent parentComponent)
        {
            var subcomponents = new List<SolutionComponent>();

            if (_solutionFileBytes == null || parentComponent.ComponentType != 1 || string.IsNullOrEmpty(parentComponent.UniqueName))
            {
                return subcomponents;
            }

            using (var memoryStream = new MemoryStream(_solutionFileBytes))
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
                        var appaction = xdocApp.Root; ;

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

        private List<SolutionComponent> ExtractEntitySubcomponentsFromXml(XDocument xdoc, string entityName)
        {
            var subcomponents = new List<SolutionComponent>();

            // Find the entity element with matching name
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
                if (referencedEntity != referencingEntity &&  referencedEntity?.Equals(entityName, StringComparison.OrdinalIgnoreCase) == true)
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
    }
}
