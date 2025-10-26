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

        /// <summary>
        /// Initializes a new instance of the SubcomponentRetriever class for environment-based retrieval.
  /// </summary>
        public SubcomponentRetriever(ServiceClient connection, PSCmdlet cmdlet)
   {
        _connection = connection;
            _cmdlet = cmdlet;
            _solutionFileBytes = null;
   }

 /// <summary>
        /// Initializes a new instance of the SubcomponentRetriever class for file-based retrieval.
        /// </summary>
    public SubcomponentRetriever(ServiceClient connection, PSCmdlet cmdlet, byte[] solutionFileBytes)
  {
            _connection = connection;
     _cmdlet = cmdlet;
        _solutionFileBytes = solutionFileBytes;
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

 var parentIdentifier = parentComponent.LogicalName ?? parentComponent.ObjectId?.ToString() ?? "Unknown";
        _cmdlet.WriteVerbose($"Found {subcomponents.Count} attributes for entity {parentIdentifier}");
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

            var parentResults = _connection.RetrieveMultiple(parentComponentQuery);
          if (parentResults.Entities.Count == 0)
            {
        _cmdlet.WriteVerbose($"Parent component {entityLogicalName} (ID: {parentObjectId}) not found in solutioncomponent table");
         return subcomponents;
        }

            var parentSolutionComponentId = parentResults.Entities[0].Id;
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

   var results = _connection.RetrieveMultiple(componentQuery);
            _cmdlet.WriteVerbose($"Found {results.Entities.Count} child components for parent {entityLogicalName}");

   foreach (var entity in results.Entities)
       {
 var objectId = entity.GetAttributeValue<Guid>("objectid");
                var componentType = entity.GetAttributeValue<OptionSetValue>("componenttype")?.Value ?? 0;

              string logicalName = null;
    // For attributes (type 2), try to get the logical name
      if (componentType == 2) // Attribute
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

        _cmdlet.WriteVerbose($"Found {subcomponents.Count} attributes for entity {entityName}");

            return subcomponents;
  }
    }
}
