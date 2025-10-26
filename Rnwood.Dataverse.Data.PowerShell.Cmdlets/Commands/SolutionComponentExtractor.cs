using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
    }
}
