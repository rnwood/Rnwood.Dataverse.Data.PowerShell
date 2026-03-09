using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Updates organization settings in the single organization record in a Dataverse environment.
    /// Automatically discovers the organization record and updates specified columns from the InputObject.
    /// Can also update individual settings within the OrgDbOrgSettings XML column using the -OrgDbOrgSettings switch.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseOrganizationSettings", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseOrganizationSettingsCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Object containing values to update.
        /// When -OrgDbOrgSettings is specified: Property names are OrgDbOrgSettings setting names.
        /// When -OrgDbOrgSettings is NOT specified: Property names must match organization table column names.
        /// Accepts either PSObject or Hashtable.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, 
            HelpMessage = "Object containing values to update. When -OrgDbOrgSettings is specified, property names are setting names. Otherwise, property names must match organization table column names. Accepts PSObject or Hashtable.")]
        public object InputObject { get; set; }

        /// <summary>
        /// If specified, updates settings within the OrgDbOrgSettings XML column instead of organization table columns.
        /// Use $null values to remove settings.
        /// </summary>
        [Parameter(HelpMessage = "If specified, updates OrgDbOrgSettings XML column instead of organization table columns. Use $null to remove settings.")]
        public SwitchParameter OrgDbOrgSettings { get; set; }

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

            // Convert Hashtable to PSObject if necessary
            PSObject inputPSObject = ConvertToPSObject(InputObject);

            // Retrieve the single organization record with all columns to compare existing values
            QueryExpression query = new QueryExpression("organization")
            {
                ColumnSet = new ColumnSet(true),
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

            if (OrgDbOrgSettings.IsPresent)
            {
                // Update OrgDbOrgSettings XML
                UpdateOrgDbOrgSettingsXml(orgEntity, updateEntity, inputPSObject);
            }
            else
            {
                // Update regular organization table columns
                UpdateOrganizationColumns(orgEntity, updateEntity, inputPSObject);
            }

            // Check if there are any updates to make
            if (updateEntity.Attributes.Count == 0)
            {
                WriteVerbose("No changes detected. All values match existing values.");
                
                // Return the existing record if PassThru is specified
                if (PassThru)
                {
                    EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
                    DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                    PSObject result = converter.ConvertToPSObject(orgEntity, new ColumnSet(true), _ => ValueType.Display);
                    WriteObject(result);
                }
                return;
            }

            // Execute the update with ShouldProcess
            string target = OrgDbOrgSettings.IsPresent 
                ? $"OrgDbOrgSettings in organization record {orgId}" 
                : $"Organization record {orgId}";
            
            if (ShouldProcess(target, "Update organization settings"))
            {
                Connection.Update(updateEntity);
                WriteVerbose($"Updated {updateEntity.Attributes.Count} attribute(s) in organization record {orgId}");

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

        private PSObject ConvertToPSObject(object input)
        {
            // If already a PSObject, return as-is
            if (input is PSObject psObj)
            {
                return psObj;
            }

            // If it's a Hashtable, convert to PSObject
            if (input is Hashtable hashtable)
            {
                PSObject result = new PSObject();
                foreach (DictionaryEntry kvp in hashtable)
                {
                    // Validate that the key is a string
                    if (!(kvp.Key is string keyName))
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException($"Hashtable keys must be strings. Found key of type '{kvp.Key?.GetType().Name ?? "null"}' with value '{kvp.Key}'."),
                            "InvalidHashtableKey",
                            ErrorCategory.InvalidArgument,
                            kvp.Key));
                        continue;
                    }
                    
                    result.Properties.Add(new PSNoteProperty(keyName, kvp.Value));
                }
                return result;
            }

            // For any other object type, wrap it in a PSObject
            return new PSObject(input);
        }

        private void UpdateOrganizationColumns(Entity existingEntity, Entity updateEntity, PSObject inputObject)
        {
            // Process properties from inputObject for regular organization columns
            foreach (PSPropertyInfo property in inputObject.Properties)
            {
                // Skip standard PS properties
                if (property.Name == "psobject" || property.Name == "pstypenames" || property.Name == "psextended")
                {
                    continue;
                }

                string columnName = property.Name;
                object newValue = property.Value;

                // Get existing value
                object existingValue = existingEntity.Contains(columnName) ? existingEntity[columnName] : null;

                // Compare values - only update if changed
                if (!ValuesAreEqual(existingValue, newValue))
                {
                    WriteVerbose($"Column '{columnName}': Changing from '{FormatValue(existingValue)}' to '{FormatValue(newValue)}'");
                    updateEntity[columnName] = newValue;
                }
                else
                {
                    WriteVerbose($"Column '{columnName}': No change (value is '{FormatValue(existingValue)}')");
                }
            }
        }

        private void UpdateOrgDbOrgSettingsXml(Entity existingEntity, Entity updateEntity, PSObject inputObject)
        {
            // Load existing XML
            string existingXml = existingEntity.Contains("orgdborgsettings") && existingEntity["orgdborgsettings"] != null
                ? existingEntity["orgdborgsettings"].ToString()
                : "<OrgSettings></OrgSettings>";

            try
            {
                XDocument doc = XDocument.Parse(existingXml);
                
                if (doc.Root == null)
                {
                    doc.Add(new XElement("OrgSettings"));
                }

                bool hasChanges = false;

                // Process each setting from inputObject
                foreach (PSPropertyInfo setting in inputObject.Properties)
                {
                    // Skip standard PS properties
                    if (setting.Name == "psobject" || setting.Name == "pstypenames" || setting.Name == "psextended")
                    {
                        continue;
                    }

                    string settingName = setting.Name;
                    object newValue = setting.Value;
                    XElement existingElement = doc.Root.Element(settingName);
                    string existingValue = existingElement?.Value;

                    // Handle null value - remove the setting
                    if (newValue == null)
                    {
                        if (existingElement != null)
                        {
                            WriteVerbose($"Setting '{settingName}': Removing (was '{existingValue}')");
                            existingElement.Remove();
                            hasChanges = true;
                        }
                        else
                        {
                            WriteVerbose($"Setting '{settingName}': Already not present (no change)");
                        }
                    }
                    else
                    {
                        string newValueStr = newValue.ToString();
                        
                        // Compare values
                        if (existingValue != newValueStr)
                        {
                            if (existingElement != null)
                            {
                                WriteVerbose($"Setting '{settingName}': Changing from '{existingValue}' to '{newValueStr}'");
                                existingElement.Value = newValueStr;
                            }
                            else
                            {
                                WriteVerbose($"Setting '{settingName}': Adding with value '{newValueStr}'");
                                doc.Root.Add(new XElement(settingName, newValueStr));
                            }
                            hasChanges = true;
                        }
                        else
                        {
                            WriteVerbose($"Setting '{settingName}': No change (value is '{existingValue}')");
                        }
                    }
                }

                // Only update if there are changes
                if (hasChanges)
                {
                    updateEntity["orgdborgsettings"] = doc.ToString();
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Failed to update OrgDbOrgSettings XML: {ex.Message}", ex),
                    "XmlUpdateFailed",
                    ErrorCategory.InvalidData,
                    inputObject));
            }
        }

        private bool ValuesAreEqual(object value1, object value2)
        {
            // Handle null cases
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Use Equals for comparison
            return value1.Equals(value2);
        }

        private string FormatValue(object value)
        {
            if (value == null) return "<null>";
            if (value is string str) return $"\"{str}\"";
            return value.ToString();
        }
    }
}
