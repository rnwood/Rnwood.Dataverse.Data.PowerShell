using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Management.Automation;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Gets organization settings from the single organization record in a Dataverse environment.
    /// When -OrgDbOrgSettings is specified, returns only the parsed OrgDbOrgSettings as properties.
    /// Otherwise, returns all organization table columns as PSObject properties.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseOrganizationSettings")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseOrganizationSettingsCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// If specified, returns only the OrgDbOrgSettings as a PSObject with settings as properties.
        /// </summary>
        [Parameter(HelpMessage = "If specified, returns only OrgDbOrgSettings instead of the full organization record")]
        public SwitchParameter OrgDbOrgSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include the raw OrgDbOrgSettings XML in the output.
        /// Only applies when -OrgDbOrgSettings is NOT specified.
        /// </summary>
        [Parameter(HelpMessage = "If specified, includes the raw OrgDbOrgSettings XML string in the output (only when -OrgDbOrgSettings is not used)")]
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
                ColumnSet = OrgDbOrgSettings.IsPresent 
                    ? new ColumnSet("organizationid", "orgdborgsettings")  // Only need OrgDbOrgSettings
                    : new ColumnSet(true),  // Get all columns
                TopCount = 1
            };

            EntityCollection results = Connection.RetrieveMultiple(query);

            if (results.Entities.Count == 0)
            {
                WriteWarning("No organization record found");
                return;
            }

            Entity orgEntity = results.Entities[0];

            if (OrgDbOrgSettings.IsPresent)
            {
                // Return only the parsed OrgDbOrgSettings
                PSObject parsedSettings = ParseOrgDbOrgSettings(orgEntity);
                WriteObject(parsedSettings);
            }
            else
            {
                // Return the full organization record
                EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
                DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                PSObject result = converter.ConvertToPSObject(orgEntity, new ColumnSet(true), _ => ValueType.Display);

                // Optionally include raw XML when not using -OrgDbOrgSettings
                if (!IncludeRawXml && orgEntity.Contains("orgdborgsettings"))
                {
                    result.Properties.Remove("orgdborgsettings");
                }

                WriteObject(result);
            }
        }

        private PSObject ParseOrgDbOrgSettings(Entity orgEntity)
        {
            PSObject parsedSettings = new PSObject();

            if (orgEntity.Contains("orgdborgsettings") && orgEntity["orgdborgsettings"] != null)
            {
                string xmlContent = orgEntity["orgdborgsettings"].ToString();

                if (!string.IsNullOrWhiteSpace(xmlContent))
                {
                    try
                    {
                        XDocument doc = XDocument.Parse(xmlContent);

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
                    }
                    catch
                    {
                        // If parsing fails, return empty object with warning
                        WriteWarning("Failed to parse OrgDbOrgSettings XML.");
                    }
                }
            }

            return parsedSettings;
        }
    }
}
