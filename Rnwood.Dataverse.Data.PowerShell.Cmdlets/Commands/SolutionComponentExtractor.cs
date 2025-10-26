using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
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
        public string LogicalName { get; set; }

        /// <summary>
        /// Gets or sets the component's metadata ID (GUID).
        /// </summary>
        public Guid? MetadataId { get; set; }

        /// <summary>
        /// Gets or sets the component type.
        /// </summary>
        public int ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the root component behavior (0=Include, 1=Do Not Include, 2=Shell).
        /// </summary>
        public int RootComponentBehavior { get; set; }

        /// <summary>
        /// Gets or sets whether this is a subcomponent.
        /// </summary>
        public bool IsSubcomponent { get; set; }

        /// <summary>
        /// Gets or sets the parent component type (for subcomponents).
        /// </summary>
        public int? ParentComponentType { get; set; }

        /// <summary>
        /// Gets or sets the parent component's logical name or object ID (for subcomponents).
        /// </summary>
        public string ParentObjectId { get; set; }
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
                                            LogicalName = componentType == 1 ? componentId : null, // Entity
                                            ObjectId = componentMetadataId,
                                            MetadataId = componentMetadataId,
                                            ComponentType = int.Parse(typeAttr.Value),
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
        /// Extracts components from a solution in the environment using the solutioncomponent table.
        /// </summary>
        public static List<SolutionComponent> ExtractEnvironmentComponents(ServiceClient connection, Guid solutionId)
        {
            var components = new List<SolutionComponent>();

            var query = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet("objectid", "componenttype", "rootcomponentbehavior"),
                Criteria = new FilterExpression
                {
                    Conditions =
            {
           new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId),
         new ConditionExpression("rootcomponentbehavior", ConditionOperator.NotNull)
     }
                }
            };

            var results = connection.RetrieveMultiple(query);

            foreach (var entity in results.Entities)
            {
                var objectid = entity.GetAttributeValue<Guid>("objectid");
                var componentType = entity.GetAttributeValue<OptionSetValue>("componenttype").Value;

                string logicalName = null;
                Guid? metadataId;

                if (componentType == 1) // Entity
                {
                    logicalName = GetEntityLogicalName(connection, objectid);
                    metadataId = objectid;
                }
                else
                {
                    metadataId = objectid;
                }

                var component = new SolutionComponent
                {
                    LogicalName = logicalName,
                    ObjectId = objectid,
                    MetadataId = metadataId,
                    ComponentType = componentType,
                    RootComponentBehavior = entity.Contains("rootcomponentbehavior")
      ? entity.GetAttributeValue<OptionSetValue>("rootcomponentbehavior").Value
     : 0
                };
                components.Add(component);
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
        /// Gets an entity's logical name from its metadata ID.
        /// </summary>
        private static string GetEntityLogicalName(ServiceClient connection, Guid metadataId)
        {
            var metadataQuery = new RetrieveMetadataChangesRequest
            {
                Query = new EntityQueryExpression
                {
                    Criteria = new MetadataFilterExpression(LogicalOperator.And)
                    {
                        Conditions =
      {
              new MetadataConditionExpression("MetadataId", MetadataConditionOperator.Equals, metadataId)
     }
                    },
                    Properties = new MetadataPropertiesExpression { PropertyNames = { "LogicalName" } }
                }
            };

            var metadataResponse = (RetrieveMetadataChangesResponse)connection.Execute(metadataQuery);

            if (metadataResponse.EntityMetadata.Count > 0)
            {
                return metadataResponse.EntityMetadata[0].LogicalName;
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
   byte[] sourceSolutionBytes = null,
 Guid? sourceSolutionId = null,
          Guid? targetSolutionId = null)
        {
            var results = new List<SolutionComponentComparisonResult>();

            // Expand components to include subcomponents based on behavior
            var expandedSourceComponents = ExpandComponentsWithSubcomponents(sourceComponents, connection, cmdlet, sourceSolutionBytes, isSource: true, solutionId: sourceSolutionId);
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

        private static List<SolutionComponentInfo> ExpandComponentsWithSubcomponents(List<SolutionComponent> components, ServiceClient connection, PSCmdlet cmdlet, byte[] sourceSolutionBytes, bool isSource, Guid? solutionId = null)
        {
            var expandedComponents = new List<SolutionComponentInfo>();

            foreach (var component in components)
     {
      // Always add the root component itself
     expandedComponents.Add(new SolutionComponentInfo
    {
            LogicalName = component.LogicalName,
   ObjectId = component.ObjectId,
          MetadataId = component.MetadataId,
      ComponentType = component.ComponentType,
         RootComponentBehavior = component.RootComponentBehavior
    });

         var subcomponents = GetSubcomponents(component, connection, cmdlet, sourceSolutionBytes, isSource, solutionId: solutionId);
        expandedComponents.AddRange(subcomponents);
        }

            return expandedComponents;
        }

  private static List<SolutionComponentInfo> GetSubcomponents(SolutionComponent parentComponent, ServiceClient connection, PSCmdlet cmdlet, byte[] sourceSolutionBytes, bool isSource, Guid? solutionId = null)
        {
            var subcomponents = new List<SolutionComponentInfo>();

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

            // Convert back to SolutionComponentInfo
            foreach (var subcomponent in retrievedSubcomponents)
            {
       subcomponents.Add(new SolutionComponentInfo
         {
      LogicalName = subcomponent.LogicalName,
          ObjectId = subcomponent.ObjectId,
      MetadataId = subcomponent.MetadataId,
  ComponentType = subcomponent.ComponentType,
        RootComponentBehavior = subcomponent.RootComponentBehavior,
  IsSubcomponent = subcomponent.IsSubcomponent,
    ParentComponentType = subcomponent.ParentComponentType,
     ParentObjectId = subcomponent.ParentObjectId
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
        private static string GetComponentKey(SolutionComponentInfo component)
        {
            // Prefer logical name (case-insensitive for comparison)
            if (!string.IsNullOrEmpty(component.LogicalName))
            {
                return component.LogicalName.ToLowerInvariant();
            }

            // Fall back to ObjectId as string
            return component.ObjectId?.ToString() ?? Guid.Empty.ToString();
        }
    }

    /// <summary>
    /// Represents the result of comparing two solution components.
    /// </summary>
    public class SolutionComponentComparisonResult
    {
        /// <summary>
        /// Gets or sets the source component.
        /// </summary>
        public SolutionComponentInfo SourceComponent { get; set; }

        /// <summary>
        /// Gets or sets the target component.
        /// </summary>
        public SolutionComponentInfo TargetComponent { get; set; }

        /// <summary>
        /// Gets or sets the comparison status.
        /// </summary>
        public SolutionComponentStatus Status { get; set; }
    }

    /// <summary>
    /// Represents a solution component for comparison or analysis.
    /// </summary>
    public class SolutionComponentInfo
    {
        /// <summary>
        /// Gets or sets the component's logical name (for entities and attributes).
        /// </summary>
        public string LogicalName { get; set; }

        /// <summary>
        /// Gets or sets the component's object ID (GUID for most component types).
        /// </summary>
        public Guid? ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the component's metadata ID (GUID).
        /// </summary>
        public Guid? MetadataId { get; set; }

        /// <summary>
        /// Gets or sets the component type.
        /// </summary>
        public int ComponentType { get; set; }

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
        /// Gets or sets the parent component's logical name or object ID (for subcomponents).
        /// </summary>
        public string ParentObjectId { get; set; }
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
}
