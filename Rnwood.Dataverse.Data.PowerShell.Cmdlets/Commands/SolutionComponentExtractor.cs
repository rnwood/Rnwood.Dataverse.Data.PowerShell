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
    /// Represents a solution component for comparison or analysis.
    /// </summary>
    public class SolutionComponent
    {
        /// <summary>
        /// Gets or sets the component's object ID (GUID for most component types).
        /// </summary>
        public Guid? ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the component's logical name (for entities and attributes).
        /// </summary>
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the component's metadata ID (GUID).
        /// </summary>
        public Guid? MetadataId { get; set; }

        /// <summary>
        /// Gets or sets the component type.
        /// </summary>
        public int ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the display name of the component type.
        /// </summary>
        public string ComponentTypeName { get; set; }

        /// <summary>
        /// Gets or sets the root component behavior (0=Include, 1=Do Not Include, 2=Shell).
        /// </summary>
        public int? RootComponentBehavior { get; set; }

        /// <summary>
        /// Gets or sets whether this is a subcomponent.
        /// </summary>
        public bool IsSubcomponent { get; set; }

        /// <summary>
        /// Gets or sets the parent component type (for subcomponents).
        /// </summary>
        public int? ParentComponentType { get; set; }

        /// <summary>
        /// Gets or sets the parent component's table name (for subcomponents).
        /// </summary>
        public string ParentTableName { get; set; }
    }

    /// <summary>
    /// Represents the result of comparing two solution components.
    /// </summary>
    public class SolutionComponentComparisonResult
    {
        /// <summary>
        /// Gets or sets the source component.
        /// </summary>
        public SolutionComponent SourceComponent { get; set; }

        /// <summary>
        /// Gets or sets the target component.
        /// </summary>
        public SolutionComponent TargetComponent { get; set; }

        /// <summary>
        /// Gets or sets the comparison status.
        /// </summary>
        public SolutionComponentStatus Status { get; set; }
    }

    /// <summary>
    /// Status of a solution component comparison.
    /// </summary>
    public enum SolutionComponentStatus
    {
        /// <summary>
        /// Component exists only in the source.
        /// </summary>
        InSourceOnly,

        /// <summary>
        /// Component exists only in the target.
        /// </summary>
        InTargetOnly,

        /// <summary>
        /// Component exists in both source and target with the same behavior.
        /// </summary>
        InSourceAndTarget,

        /// <summary>
        /// Component exists in both but source behavior is more inclusive.
        /// </summary>
        InSourceAndTarget_BehaviourMoreInclusiveInSource,

        /// <summary>
        /// Component exists in both but source behavior is less inclusive.
        /// </summary>
        InSourceAndTarget_BehaviourLessInclusiveInSource
    }

    /// <summary>
    /// Utility class for extracting solution components from solution files and environments.
    /// </summary>
    public static class SolutionComponentExtractor
    {
        /// <summary>
        /// Extracts components from a solution file's solution.xml.
        /// </summary>
        public static (string UniqueName, List<SolutionComponent> Components) ExtractSolutionFileComponents(byte[] solutionBytes)
        {
            var components = new List<SolutionComponent>();
            string uniqueName = null;
            XDocument customizationsXdoc = null;

            using (var memoryStream = new MemoryStream(solutionBytes))
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
                        uniqueName = solutionManifest?.Element("UniqueName")?.Value;

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
                                            MetadataId = componentMetadataId,
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

            return (uniqueName, components);
        }

        /// <summary>
        /// Extracts components from a solution in the environment using the msdyn_solutioncomponentsummary table via REST API.
        /// </summary>
        public static List<SolutionComponent> ExtractEnvironmentComponents(ServiceClient connection, Guid solutionId)
        {
            var components = new List<SolutionComponent>();

            try
            {
                // Use REST API to query msdyn_solutioncomponentsummary (virtual table)
                var filter = $"msdyn_solutionid eq {solutionId:B}";
                var select = "msdyn_objectid,msdyn_componenttype,msdyn_uniquename,msdyn_primaryentityname,msdyn_componenttypename,msdyn_name,msdyn_schemaname";
                var queryString = $"$filter={Uri.EscapeDataString(filter)}&$select={select}";

                var response = connection.ExecuteWebRequest(HttpMethod.Get, $"msdyn_solutioncomponentsummaries?{queryString}", null, null);

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

                            string logicalName = schemaName ?? uniqueName;

                            var component = new SolutionComponent
                            {
                                UniqueName = logicalName.ToLower(),
                                ObjectId = objectId,
                                MetadataId = objectId,
                                ComponentType = componentType,
                                ComponentTypeName = componentTypeName,
                                RootComponentBehavior = 0 // Will be updated from solutioncomponent query
                            };
                            components.Add(component);
                        }
                    }
                }

                // Query solutioncomponent table to get rootcomponentbehavior for each component
                if (components.Count > 0)
                {
                    var solutionComponentQuery = new QueryExpression("solutioncomponent")
                    {
                        ColumnSet = new ColumnSet("objectid", "rootcomponentbehavior"),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId)
                            }
                        }
                    };

                    var solutionComponents = connection.RetrieveMultiple(solutionComponentQuery);

                    // Create a lookup dictionary for quick matching by objectid
                    var behaviorLookup = solutionComponents.Entities
                        .Where(e => e.Contains("rootcomponentbehavior"))
                        .ToDictionary(
                            e => e.GetAttributeValue<Guid>("objectid"),
                            e => e.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value ?? 0
                        );

                    // Update components with behavior information
                    foreach (var component in components)
                    {
                        if (component.ObjectId.HasValue && behaviorLookup.TryGetValue(component.ObjectId.Value, out var behavior))
                        {
                            component.RootComponentBehavior = behavior;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error extracting environment components: {ex.Message}", ex);
            }

            return components;
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

        /// <summary>
        /// Compares solution components between source and target and returns the comparison results.
        /// </summary>
        public static List<SolutionComponentComparisonResult> CompareSolutionComponents(
      ServiceClient connection,
   PSCmdlet cmdlet,
       List<SolutionComponent> sourceComponents,
    List<SolutionComponent> targetComponents,
            byte[] sourceSolutionBytes,
      Guid targetSolutionId)
        {
            var results = new List<SolutionComponentComparisonResult>();

            // Expand components to include subcomponents based on behavior
            var expandedSourceComponents = ExpandComponentsWithSubcomponents(sourceComponents, connection, cmdlet, sourceSolutionBytes, isSource: true, solutionId: null);
            var expandedTargetComponents = ExpandComponentsWithSubcomponents(targetComponents, connection, cmdlet, null, isSource: false, solutionId: targetSolutionId);

            cmdlet.WriteVerbose($"Expanded source components: {sourceComponents.Count} root -> {expandedSourceComponents.Count} total");
            cmdlet.WriteVerbose($"Expanded target components: {targetComponents.Count} root -> {expandedTargetComponents.Count} total");

            // Sort components for consistent comparison
            expandedSourceComponents = expandedSourceComponents.OrderBy(c => c.ComponentType).ThenBy(c => GetComponentKey(c)).ToList();
            expandedTargetComponents = expandedTargetComponents.OrderBy(c => c.ComponentType).ThenBy(c => GetComponentKey(c)).ToList();

            // Create lookup dictionaries for efficient comparison (compare by logical name first, then object ID)
            var sourceComponentDict = expandedSourceComponents
    .GroupBy(c => new { c.ComponentType, Key = GetComponentKey(c) })
    .ToDictionary(g => g.Key, g => g.First());

            var targetComponentDict = expandedTargetComponents
     .GroupBy(c => new { c.ComponentType, Key = GetComponentKey(c) })
        .ToDictionary(g => g.Key, g => g.First());

            // Find added components (in source but not in target)
            foreach (var sourceComponent in expandedSourceComponents)
            {
                var key = new { sourceComponent.ComponentType, Key = GetComponentKey(sourceComponent) };

                if (!targetComponentDict.ContainsKey(key))
                {
                    results.Add(new SolutionComponentComparisonResult
                    {
                        SourceComponent = sourceComponent,
                        TargetComponent = null,
                        Status = SolutionComponentStatus.InSourceOnly
                    });
                }
                else
                {
                    var targetComponent = targetComponentDict[key];

                    // Check if behavior changed
                    if (sourceComponent.RootComponentBehavior != targetComponent.RootComponentBehavior)
                    {
                        // Determine if this is an inclusion or exclusion based on behavior change
                        SolutionComponentStatus status = DetermineBehaviorChangeStatus(sourceComponent.RootComponentBehavior ?? 0, targetComponent.RootComponentBehavior ?? 0);

                        results.Add(new SolutionComponentComparisonResult
                        {
                            SourceComponent = sourceComponent,
                            TargetComponent = targetComponent,
                            Status = status
                        });
                    }
                    else
                    {
                        // Component exists in both with same behavior
                        results.Add(new SolutionComponentComparisonResult
                        {
                            SourceComponent = sourceComponent,
                            TargetComponent = targetComponent,
                            Status = SolutionComponentStatus.InSourceAndTarget
                        });
                    }
                }
            }

            // Find removed components (in target but not in source)
            foreach (var targetComponent in expandedTargetComponents)
            {
                var key = new { targetComponent.ComponentType, Key = GetComponentKey(targetComponent) };

                if (!sourceComponentDict.ContainsKey(key))
                {
                    results.Add(new SolutionComponentComparisonResult
                    {
                        SourceComponent = null,
                        TargetComponent = targetComponent,
                        Status = SolutionComponentStatus.InTargetOnly
                    });
                }
            }

            return results;
        }

        private static List<SolutionComponent> ExpandComponentsWithSubcomponents(List<SolutionComponent> components, ServiceClient connection, PSCmdlet cmdlet, byte[] sourceSolutionBytes, bool isSource, Guid? solutionId)
        {
            var expandedComponents = new List<SolutionComponent>();

            foreach (var component in components)
            {
                // Always add the root component itself
                expandedComponents.Add(new SolutionComponent
                {
                    UniqueName = component.UniqueName,
                    ObjectId = component.ObjectId,
                    MetadataId = component.MetadataId,
                    ComponentType = component.ComponentType,
                    ComponentTypeName = component.ComponentTypeName,
                    RootComponentBehavior = component.RootComponentBehavior
                });

                var subcomponents = GetSubcomponents(component, connection, cmdlet, sourceSolutionBytes, isSource, solutionId: solutionId);
                expandedComponents.AddRange(subcomponents);
            }

            return expandedComponents;
        }

        private static List<SolutionComponent> GetSubcomponents(SolutionComponent parentComponent, ServiceClient connection, PSCmdlet cmdlet, byte[] sourceSolutionBytes, bool isSource, Guid? solutionId = null)
        {
            var subcomponents = new List<SolutionComponent>();

            // Use SubcomponentRetriever for retrieval
            SubcomponentRetriever retriever;
            if (isSource && sourceSolutionBytes != null)
            {
                retriever = new SubcomponentRetriever(connection, cmdlet, sourceSolutionBytes, solutionId);
            }
            else
            {
                retriever = new SubcomponentRetriever(connection, cmdlet, solutionId);
            }

            var retrievedSubcomponents = retriever.GetSubcomponents(parentComponent);

            // Convert back to SolutionComponent
            foreach (var subcomponent in retrievedSubcomponents)
            {
                subcomponents.Add(new SolutionComponent
                {
                    UniqueName = subcomponent.UniqueName,
                    ObjectId = subcomponent.ObjectId,
                    MetadataId = subcomponent.MetadataId,
                    ComponentType = subcomponent.ComponentType,
                    ComponentTypeName = subcomponent.ComponentTypeName,
                    RootComponentBehavior = subcomponent.RootComponentBehavior,
                    IsSubcomponent = subcomponent.IsSubcomponent,
                    ParentComponentType = subcomponent.ParentComponentType,
                    ParentTableName = subcomponent.ParentTableName
                });
            }

            return subcomponents;
        }

        private static SolutionComponentStatus DetermineBehaviorChangeStatus(int sourceBehavior, int targetBehavior)
        {
            // Behavior levels: 0 (Full/Include Subcomponents) > 1 (Do Not Include Subcomponents) > 2 (Shell)
            // Going from higher number to lower number = including more data (BehaviorIncluded)
            // Going from lower number to higher number = excluding data (BehaviorExcluded)

            if (sourceBehavior < targetBehavior)
            {
                // e.g., 0 (Full) -> 2 (Shell): excluding/removing data
                return SolutionComponentStatus.InSourceAndTarget_BehaviourLessInclusiveInSource;
            }
            else if (sourceBehavior > targetBehavior)
            {
                // e.g., 2 (Shell) -> 0 (Full): including more data
                return SolutionComponentStatus.InSourceAndTarget_BehaviourMoreInclusiveInSource;
            }

            return SolutionComponentStatus.InSourceAndTarget;
        }

        /// <summary>
        /// Gets a comparison key for a component, preferring LogicalName if available, otherwise ObjectId.
        /// </summary>
        private static string GetComponentKey(SolutionComponent component)
        {
            // Prefer logical name (case-insensitive for comparison)
            if (!string.IsNullOrEmpty(component.UniqueName))
            {
                return component.UniqueName.ToLowerInvariant();
            }

            // Fall back to ObjectId as string
            return component.ObjectId?.ToString() ?? Guid.Empty.ToString();
        }
    }
}
