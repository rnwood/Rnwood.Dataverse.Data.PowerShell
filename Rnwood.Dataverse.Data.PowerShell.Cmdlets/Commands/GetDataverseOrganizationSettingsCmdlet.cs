using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Management.Automation;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Gets organization settings from the single organization record in a Dataverse environment.
    /// Returns all organization table columns as PSObject properties, with the OrgDbOrgSettings XML column parsed into structured data.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseOrganizationSettings")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseOrganizationSettingsCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include the raw OrgDbOrgSettings XML in the output.
        /// </summary>
        [Parameter(HelpMessage = "If specified, includes the raw OrgDbOrgSettings XML string in the output")]
        public SwitchParameter IncludeRawXml { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            // Query for the single organization record
            QueryExpression query = new QueryExpression("organization")
            {
                ColumnSet = new ColumnSet(true), // Get all columns
                TopCount = 1
            };

            EntityCollection results = Connection.RetrieveMultiple(query);

            if (results.Entities.Count == 0)
            {
                WriteWarning("No organization record found");
                return;
            }

            Entity orgEntity = results.Entities[0];
            
            // Convert to PSObject
            EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
            DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
            PSObject result = converter.ConvertToPSObject(orgEntity, new ColumnSet(true), _ => ValueType.Display);

            // Parse OrgDbOrgSettings XML if present
            if (orgEntity.Contains("orgdborgsettings") && orgEntity["orgdborgsettings"] != null)
            {
                string xmlContent = orgEntity["orgdborgsettings"].ToString();
                
                if (!string.IsNullOrWhiteSpace(xmlContent))
                {
                    try
                    {
                        XDocument doc = XDocument.Parse(xmlContent);
                        
                        // Create a nested PSObject for the parsed settings
                        PSObject parsedSettings = new PSObject();
                        
                        // Iterate through all elements in the XML and add as properties
                        if (doc.Root != null)
                        {
                            foreach (XElement element in doc.Root.Elements())
                            {
                                string elementName = element.Name.LocalName;
                                string elementValue = element.Value;
                                
                                // Try to parse as common types
                                if (bool.TryParse(elementValue, out bool boolValue))
                                {
                                    parsedSettings.Properties.Add(new PSNoteProperty(elementName, boolValue));
                                }
                                else if (int.TryParse(elementValue, out int intValue))
                                {
                                    parsedSettings.Properties.Add(new PSNoteProperty(elementName, intValue));
                                }
                                else
                                {
                                    parsedSettings.Properties.Add(new PSNoteProperty(elementName, elementValue));
                                }
                            }
                        }
                        
                        // Add the parsed settings as a property
                        result.Properties.Add(new PSNoteProperty("OrgDbOrgSettingsParsed", parsedSettings));
                        
                        // Optionally include raw XML
                        if (!IncludeRawXml)
                        {
                            // Remove the raw XML property to keep output clean
                            result.Properties.Remove("orgdborgsettings");
                        }
                    }
                    catch
                    {
                        // If parsing fails, leave the raw XML in the output
                        WriteWarning("Failed to parse OrgDbOrgSettings XML. Returning raw XML value.");
                    }
                }
            }

            WriteObject(result);
        }
    }
}
