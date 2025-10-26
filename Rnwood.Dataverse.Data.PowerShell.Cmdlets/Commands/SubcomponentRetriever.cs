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

            var parentIdentifier = parentComponent.LogicalName ?? parentComponent.ObjectId?.ToString() ?? "Unknown";
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

            // Otherwise, retrieve from environment based on behavior
            if (parentComponent.RootComponentBehavior != 0)
            {
                _cmdlet.WriteVerbose($"Parent behavior is {parentComponent.RootComponentBehavior} (not 'Include Subcomponents'), querying solutioncomponent table");
                return GetSubcomponentsFromSolutionTable(parentComponent);
            }

            // Default behavior: retrieve all attributes from metadata
            return GetAllAttributesFromMetadata(parentComponent);
        }

        private List<SolutionComponent> GetAllAttributesFromMetadata(SolutionComponent parentComponent)
        {
            var subcomponents = new List<SolutionComponent>();

            var metadataQuery = new RetrieveMetadataChangesRequest
            {
                Query = new EntityQueryExpression
                {
                    Criteria = new MetadataFilterExpression(LogicalOperator.And)
                    {
                        Conditions =
  {
    new MetadataConditionExpression("LogicalName", MetadataConditionOperator.Equals, parentComponent.LogicalName)
       }
                    },
                    Properties = new MetadataPropertiesExpression { AllProperties = true },
                    AttributeQuery = new AttributeQueryExpression
                    {
                        Properties = new MetadataPropertiesExpression { AllProperties = true }
                    }
                }
            };

            var metadataResponse = (RetrieveMetadataChangesResponse)_connection.Execute(metadataQuery);

            if (metadataResponse.EntityMetadata != null && metadataResponse.EntityMetadata.Count > 0)
            {
                var entityMetadata = metadataResponse.EntityMetadata[0];

                // Add attributes with logical names
                if (entityMetadata.Attributes != null)
                {
                    foreach (var attribute in entityMetadata.Attributes)
                    {
                        if (attribute.MetadataId.HasValue)
                        {
                            subcomponents.Add(new SolutionComponent
                            {
                                LogicalName = attribute.LogicalName,
                                MetadataId = attribute.MetadataId.Value,
                                ComponentType = 2, // Attribute
                                RootComponentBehavior = 0,
                                IsSubcomponent = true,
                                ParentComponentType = 1,
                                ParentObjectId = parentComponent.LogicalName
                            });
                        }
                    }
                }
            }

            return subcomponents;
        }

        private List<SolutionComponent> GetSubcomponentsFromSolutionTable(SolutionComponent parentComponent)
        {
            var subcomponents = new List<SolutionComponent>();

      // For entities, we need to use the LogicalName to query
         string entityLogicalName = parentComponent.LogicalName;
            if (string.IsNullOrEmpty(entityLogicalName))
 {
    _cmdlet.WriteVerbose($"Parent entity component has no LogicalName and cannot be resolved");
        return subcomponents;
   }

            // Get the entity's metadata ID from the logical name
    Guid parentObjectId = GetEntityMetadataId(entityLogicalName);
    if (parentObjectId == Guid.Empty)
   {
              _cmdlet.WriteVerbose($"Could not resolve metadata ID for entity {entityLogicalName}");
            return subcomponents;
    }

         // Get the parent component's solutioncomponentid from the solutioncomponent table
  // We need to find the parent component record first to get its solutioncomponentid
       var parentComponentQuery = new QueryExpression("solutioncomponent")
            {
   ColumnSet = new ColumnSet("solutioncomponentid"),
    Criteria = new FilterExpression
      {
         Conditions =
    {
     new ConditionExpression("objectid", ConditionOperator.Equal, parentObjectId),
      new ConditionExpression("componenttype", ConditionOperator.Equal, parentComponent.ComponentType)
        }
             }
 };

          // Add solutionid filter if available
            if (_solutionId.HasValue)
         {
     parentComponentQuery.Criteria.AddCondition("solutionid", ConditionOperator.Equal, _solutionId.Value);
            }

            var parentResults = _connection.RetrieveMultiple(parentComponentQuery);

            if (parentResults.Entities.Count == 0)
            {
    _cmdlet.WriteVerbose($"Parent component not found in solutioncomponent table{(_solutionId.HasValue ? $" for solution {_solutionId.Value}" : "")}");
           return subcomponents;
            }

       var parentSolutionComponentId = parentResults.Entities[0].GetAttributeValue<Guid>("solutioncomponentid");
      _cmdlet.WriteVerbose($"Found parent component {entityLogicalName} with solutioncomponentid {parentSolutionComponentId}");

            // Query solutioncomponent table for components that have this parent's solutioncomponentid
     var componentQuery = new QueryExpression("solutioncomponent")
      {
    ColumnSet = new ColumnSet("objectid", "componenttype", "rootcomponentbehavior"),
      Criteria = new FilterExpression
      {
     Conditions =
         {
       new ConditionExpression("rootsolutioncomponentid", ConditionOperator.Equal, parentSolutionComponentId)
        }
  }
            };

      // Add solutionid filter if available
     if (_solutionId.HasValue)
            {
     componentQuery.Criteria.AddCondition("solutionid", ConditionOperator.Equal, _solutionId.Value);
        }

            var results = _connection.RetrieveMultiple(componentQuery);
    _cmdlet.WriteVerbose($"Found {results.Entities.Count} child components for parent {entityLogicalName}");

  foreach (var entity in results.Entities)
            {
       var objectId = entity.GetAttributeValue<Guid>("objectid");
var componentType = entity.GetAttributeValue<OptionSetValue>("componenttype")?.Value ?? 0;

      string logicalName = null;
        // For attributes (type 2), try to get the logical name
      if (componentType == 2 // Attribute
    )
  {
  logicalName = GetAttributeLogicalName(entityLogicalName, objectId);
    }

  var componentBehavior = entity.Contains("rootcomponentbehavior")
             ? entity.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value ?? 0
         : 0;

  subcomponents.Add(new SolutionComponent
          {
    ComponentType = componentType,
        LogicalName = logicalName,
    ObjectId = objectId,
   MetadataId = objectId,
           RootComponentBehavior = componentBehavior,
    IsSubcomponent = true,
            ParentComponentType = 1,
              ParentObjectId = entityLogicalName
        });
            }

          _cmdlet.WriteVerbose($"Extracted {subcomponents.Count} child components from solutioncomponent table for entity {entityLogicalName}");
  return subcomponents;
        }

        private Guid GetEntityMetadataId(string entityLogicalName)
        {
            var metadataQuery = new RetrieveMetadataChangesRequest
            {
                Query = new EntityQueryExpression
                {
                    Criteria = new MetadataFilterExpression(LogicalOperator.And)
                    {
                        Conditions =
           {
   new MetadataConditionExpression("LogicalName", MetadataConditionOperator.Equals, entityLogicalName)
     }
                    },
                    Properties = new MetadataPropertiesExpression { PropertyNames = { "MetadataId" } }
                }
            };

            try
            {
                var metadataResponse = (RetrieveMetadataChangesResponse)_connection.Execute(metadataQuery);
                if (metadataResponse.EntityMetadata != null && metadataResponse.EntityMetadata.Count > 0)
                {
                    var metadataId = metadataResponse.EntityMetadata[0].MetadataId;
                    if (metadataId.HasValue)
                    {
                        return metadataId.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _cmdlet.WriteVerbose($"Error retrieving metadata ID for entity {entityLogicalName}: {ex.Message}");
            }

            return Guid.Empty;
        }

        private string GetAttributeLogicalName(string entityLogicalName, Guid attributeMetadataId)
        {
            var metadataQuery = new RetrieveMetadataChangesRequest
            {
                Query = new EntityQueryExpression
                {
                    Criteria = new MetadataFilterExpression(LogicalOperator.And)
                    {
                        Conditions =
     {
           new MetadataConditionExpression("LogicalName", MetadataConditionOperator.Equals, entityLogicalName)
              }
                    },
                    Properties = new MetadataPropertiesExpression { PropertyNames = { "Attributes" } },
                    AttributeQuery = new AttributeQueryExpression
                    {
                        Criteria = new MetadataFilterExpression(LogicalOperator.And)
                        {
                            Conditions =
   {
            new MetadataConditionExpression("MetadataId", MetadataConditionOperator.Equals, attributeMetadataId)
    }
                        },
                        Properties = new MetadataPropertiesExpression { PropertyNames = { "LogicalName" } }
                    }
                }
            };

            var metadataResponse = (RetrieveMetadataChangesResponse)_connection.Execute(metadataQuery);
            if (metadataResponse.EntityMetadata != null && metadataResponse.EntityMetadata.Count > 0)
            {
                var entity = metadataResponse.EntityMetadata[0];
                if (entity.Attributes != null && entity.Attributes.Length > 0)
                {
                    return entity.Attributes[0].LogicalName;
                }
            }

            return null;
        }

        private List<SolutionComponent> ExtractSubcomponentsFromFile(SolutionComponent parentComponent)
        {
            var subcomponents = new List<SolutionComponent>();

            if (_solutionFileBytes == null || parentComponent.ComponentType != 1 || string.IsNullOrEmpty(parentComponent.LogicalName))
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
                        subcomponents.AddRange(ExtractEntitySubcomponentsFromXml(xdoc, parentComponent.LogicalName));
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
                        if (parts.Length >= 2 && parts[1].Equals(parentComponent.LogicalName, StringComparison.OrdinalIgnoreCase))
                        {
                            subcomponents.Add(new SolutionComponent
                            {
                                ComponentType = 10192,
                                LogicalName = uniquename,
                                RootComponentBehavior = 0,
                                IsSubcomponent = true,
                                ParentComponentType = 1,
                                ParentObjectId = parentComponent.LogicalName
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
                    LogicalName = attribute.Element("Name")?.Value,
                    RootComponentBehavior = 0,
                    IsSubcomponent = true,
                    ParentComponentType = 1,
                    ParentObjectId = entityName
                });
            }

            // Extract messages
            var messages = targetEntity?.Elements("Strings") ?? Enumerable.Empty<XElement>();

            foreach (var message in messages)
            {
                var resourceKey = message.Attribute("ResourceKey")?.Value;
                if (!string.IsNullOrEmpty(resourceKey))
                {
                    var logicalName = $"{entityName}.{resourceKey}";
                    subcomponents.Add(new SolutionComponent
                    {
                        ComponentType = 22, // Display String
                        LogicalName = logicalName,
                        RootComponentBehavior = 0,
                        IsSubcomponent = true,
                        ParentComponentType = 1,
                        ParentObjectId = entityName
                    });
                }
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
                        LogicalName = name,
                        RootComponentBehavior = 0,
                        IsSubcomponent = true,
                        ParentComponentType = 1,
                        ParentObjectId = entityName
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
                    LogicalName = logicalName,
                    RootComponentBehavior = 0,
                    IsSubcomponent = true,
                    ParentComponentType = 1,
                    ParentObjectId = entityName
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
                    ParentObjectId = entityName
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
                    ParentObjectId = entityName
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
                    ParentObjectId = entityName
                });
            }

            _cmdlet.WriteVerbose($"Found {subcomponents.Count} subcomponents for entity {entityName}");

            return subcomponents;
        }
    }
}
