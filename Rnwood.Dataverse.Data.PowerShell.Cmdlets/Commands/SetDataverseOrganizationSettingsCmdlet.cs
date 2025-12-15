using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Updates organization settings in the single organization record in a Dataverse environment.
    /// Automatically discovers the organization record and updates specified columns from the InputObject.
    /// Can also update individual settings within the OrgDbOrgSettings XML column.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseOrganizationSettings", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseOrganizationSettingsCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Object containing values to update in the organization record.
        /// Property names must match the logical names of organization table columns.
        /// To update OrgDbOrgSettings, provide a hashtable or PSObject with setting names and values.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, 
            HelpMessage = "Object containing values to update. Property names must match organization table column names. To update OrgDbOrgSettings, use OrgDbOrgSettingsUpdate property with a hashtable of setting names and values.")]
        public PSObject InputObject { get; set; }

        /// <summary>
        /// If specified, the updated organization record is written to the pipeline as a PSObject.
        /// </summary>
        [Parameter(HelpMessage = "If specified, returns the updated organization record")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // First, retrieve the single organization record to get its ID
            QueryExpression query = new QueryExpression("organization")
            {
                ColumnSet = new ColumnSet("organizationid", "orgdborgsettings"),
                TopCount = 1
            };

            EntityCollection results = Connection.RetrieveMultiple(query);

            if (results.Entities.Count == 0)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException("No organization record found"),
                    "NoOrganizationRecord",
                    ErrorCategory.ObjectNotFound,
                    null));
                return;
            }

            Entity orgEntity = results.Entities[0];
            Guid orgId = orgEntity.Id;

            // Build the update entity
            Entity updateEntity = new Entity("organization")
            {
                Id = orgId
            };

            bool hasOrgDbOrgSettingsUpdate = false;
            PSObject orgDbOrgSettingsUpdate = null;

            // Process properties from InputObject
            foreach (PSPropertyInfo property in InputObject.Properties)
            {
                string propertyName = property.Name.ToLower();

                // Special handling for OrgDbOrgSettings updates
                if (propertyName == "orgdborgsettingsupdate")
                {
                    hasOrgDbOrgSettingsUpdate = true;
                    orgDbOrgSettingsUpdate = PSObject.AsPSObject(property.Value);
                    continue;
                }

                // Skip standard PS properties
                if (propertyName == "psobject" || propertyName == "pstypenames" || propertyName == "psextended")
                {
                    continue;
                }

                // Convert the property value to Dataverse format using ConvertToDataverseEntity
                // For now, just use the property value directly
                if (property.Value != null)
                {
                    updateEntity[property.Name] = property.Value;
                }
            }

            // Handle OrgDbOrgSettings XML updates
            if (hasOrgDbOrgSettingsUpdate && orgDbOrgSettingsUpdate != null)
            {
                // Load existing XML
                string existingXml = orgEntity.Contains("orgdborgsettings") && orgEntity["orgdborgsettings"] != null
                    ? orgEntity["orgdborgsettings"].ToString()
                    : "<OrgSettings></OrgSettings>";

                try
                {
                    XDocument doc = XDocument.Parse(existingXml);
                    
                    if (doc.Root == null)
                    {
                        doc.Add(new XElement("OrgSettings"));
                    }

                    // Update or add settings
                    foreach (PSPropertyInfo setting in orgDbOrgSettingsUpdate.Properties)
                    {
                        if (setting.Name == "psobject" || setting.Name == "pstypenames" || setting.Name == "psextended")
                        {
                            continue;
                        }

                        XElement existingElement = doc.Root.Element(setting.Name);
                        
                        if (existingElement != null)
                        {
                            existingElement.Value = setting.Value?.ToString() ?? string.Empty;
                        }
                        else
                        {
                            doc.Root.Add(new XElement(setting.Name, setting.Value?.ToString() ?? string.Empty));
                        }
                    }

                    // Update the orgdborgsettings column
                    updateEntity["orgdborgsettings"] = doc.ToString();
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to update OrgDbOrgSettings XML: {ex.Message}", ex),
                        "XmlUpdateFailed",
                        ErrorCategory.InvalidData,
                        orgDbOrgSettingsUpdate));
                    return;
                }
            }

            // Check if there are any updates to make
            if (updateEntity.Attributes.Count == 0)
            {
                WriteWarning("No valid properties provided for update");
                return;
            }

            // Execute the update with ShouldProcess
            if (ShouldProcess($"Organization record {orgId}", "Update organization settings"))
            {
                Connection.Update(updateEntity);
                WriteVerbose($"Updated organization record {orgId}");

                // Return the updated record if PassThru is specified
                if (PassThru)
                {
                    Entity updatedEntity = Connection.Retrieve("organization", orgId, new ColumnSet(true));
                    EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
                    DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                    PSObject result = converter.ConvertToPSObject(updatedEntity, new ColumnSet(true), _ => ValueType.Display);
                    WriteObject(result);
                }
            }
        }
    }
}
