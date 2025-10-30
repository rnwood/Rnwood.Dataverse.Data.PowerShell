using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves metadata for a specific attribute (column) from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseAttribute")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseAttributeCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Logical name of the entity (table)")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the attribute.
        /// If not specified, returns all attributes for the entity.
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, HelpMessage = "Logical name of the attribute (column). If not specified, returns all attributes for the entity.")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        [Alias("ColumnName")]
        public string AttributeName { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (string.IsNullOrWhiteSpace(AttributeName))
            {
                // List all attributes for the entity
                RetrieveAllAttributes();
            }
            else
            {
                // Retrieve specific attribute
                RetrieveSingleAttribute();
            }
        }

        private void RetrieveAllAttributes()
        {
            var request = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = EntityName,
                RetrieveAsIfPublished = false
            };

            WriteVerbose($"Retrieving all attributes for entity '{EntityName}'");

            var response = (RetrieveEntityResponse)Connection.Execute(request);
            var entityMetadata = response.EntityMetadata;

            if (entityMetadata.Attributes == null || entityMetadata.Attributes.Length == 0)
            {
                WriteVerbose($"No attributes found for entity '{EntityName}'");
                return;
            }

            WriteVerbose($"Retrieved {entityMetadata.Attributes.Length} attributes");

            var results = entityMetadata.Attributes
                .OrderBy(a => a.LogicalName, StringComparer.OrdinalIgnoreCase)
                .Select(a => ConvertAttributeMetadataToPSObject(a))
                .ToArray();

            WriteObject(results, true);
        }

        private void RetrieveSingleAttribute()
        {
            var request = new RetrieveAttributeRequest
            {
                EntityLogicalName = EntityName,
                LogicalName = AttributeName,
                RetrieveAsIfPublished = false
            };

            WriteVerbose($"Retrieving attribute metadata for '{EntityName}.{AttributeName}'");

            var response = (RetrieveAttributeResponse)Connection.Execute(request);
            var attributeMetadata = response.AttributeMetadata;

            var result = ConvertAttributeMetadataToPSObject(attributeMetadata);

            WriteObject(result);
        }

        private PSObject ConvertAttributeMetadataToPSObject(AttributeMetadata attr)
        {
            var result = new PSObject();

            // Basic properties
            result.Properties.Add(new PSNoteProperty("LogicalName", attr.LogicalName));
            result.Properties.Add(new PSNoteProperty("SchemaName", attr.SchemaName));
            result.Properties.Add(new PSNoteProperty("DisplayName", attr.DisplayName?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("Description", attr.Description?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("AttributeType", attr.AttributeType?.ToString()));
            result.Properties.Add(new PSNoteProperty("AttributeTypeName", attr.AttributeTypeName?.Value));
            result.Properties.Add(new PSNoteProperty("IsCustomAttribute", attr.IsCustomAttribute));
            result.Properties.Add(new PSNoteProperty("IsCustomizable", attr.IsCustomizable?.Value));
            result.Properties.Add(new PSNoteProperty("IsManaged", attr.IsManaged));
            result.Properties.Add(new PSNoteProperty("IsPrimaryId", attr.IsPrimaryId));
            result.Properties.Add(new PSNoteProperty("IsPrimaryName", attr.IsPrimaryName));
            result.Properties.Add(new PSNoteProperty("IsValidForRead", attr.IsValidForRead));
            result.Properties.Add(new PSNoteProperty("IsValidForCreate", attr.IsValidForCreate));
            result.Properties.Add(new PSNoteProperty("IsValidForUpdate", attr.IsValidForUpdate));
            result.Properties.Add(new PSNoteProperty("IsValidForAdvancedFind", attr.IsValidForAdvancedFind?.Value));
            result.Properties.Add(new PSNoteProperty("IsAuditEnabled", attr.IsAuditEnabled?.Value));
            result.Properties.Add(new PSNoteProperty("IsSecured", attr.IsSecured));
            result.Properties.Add(new PSNoteProperty("RequiredLevel", attr.RequiredLevel?.Value.ToString()));
            result.Properties.Add(new PSNoteProperty("MetadataId", attr.MetadataId));
            result.Properties.Add(new PSNoteProperty("EntityLogicalName", attr.EntityLogicalName));
            result.Properties.Add(new PSNoteProperty("IsLogical", attr.IsLogical));
            result.Properties.Add(new PSNoteProperty("ColumnNumber", attr.ColumnNumber));
            result.Properties.Add(new PSNoteProperty("SourceType", attr.SourceType));

            // Add type-specific properties
            AddTypeSpecificProperties(result, attr);

            return result;
        }

        private void AddTypeSpecificProperties(PSObject result, AttributeMetadata attr)
        {
            // String attributes
            if (attr is StringAttributeMetadata stringAttr)
            {
                result.Properties.Add(new PSNoteProperty("MaxLength", stringAttr.MaxLength));
                result.Properties.Add(new PSNoteProperty("Format", stringAttr.Format?.ToString()));
                result.Properties.Add(new PSNoteProperty("FormatName", stringAttr.FormatName?.Value));
                result.Properties.Add(new PSNoteProperty("ImeMode", stringAttr.ImeMode?.ToString()));
            }
            // Memo attributes
            else if (attr is MemoAttributeMetadata memoAttr)
            {
                result.Properties.Add(new PSNoteProperty("MaxLength", memoAttr.MaxLength));
                result.Properties.Add(new PSNoteProperty("ImeMode", memoAttr.ImeMode?.ToString()));
            }
            // Integer attributes
            else if (attr is IntegerAttributeMetadata intAttr)
            {
                result.Properties.Add(new PSNoteProperty("MinValue", intAttr.MinValue));
                result.Properties.Add(new PSNoteProperty("MaxValue", intAttr.MaxValue));
                result.Properties.Add(new PSNoteProperty("Format", intAttr.Format?.ToString()));
            }
            // BigInt attributes
            else if (attr is BigIntAttributeMetadata bigIntAttr)
            {
                // BigInt doesn't have min/max in metadata
            }
            // Decimal attributes
            else if (attr is DecimalAttributeMetadata decimalAttr)
            {
                result.Properties.Add(new PSNoteProperty("MinValue", decimalAttr.MinValue));
                result.Properties.Add(new PSNoteProperty("MaxValue", decimalAttr.MaxValue));
                result.Properties.Add(new PSNoteProperty("Precision", decimalAttr.Precision));
                result.Properties.Add(new PSNoteProperty("ImeMode", decimalAttr.ImeMode?.ToString()));
            }
            // Double attributes
            else if (attr is DoubleAttributeMetadata doubleAttr)
            {
                result.Properties.Add(new PSNoteProperty("MinValue", doubleAttr.MinValue));
                result.Properties.Add(new PSNoteProperty("MaxValue", doubleAttr.MaxValue));
                result.Properties.Add(new PSNoteProperty("Precision", doubleAttr.Precision));
                result.Properties.Add(new PSNoteProperty("ImeMode", doubleAttr.ImeMode?.ToString()));
            }
            // Money attributes
            else if (attr is MoneyAttributeMetadata moneyAttr)
            {
                result.Properties.Add(new PSNoteProperty("MinValue", moneyAttr.MinValue));
                result.Properties.Add(new PSNoteProperty("MaxValue", moneyAttr.MaxValue));
                result.Properties.Add(new PSNoteProperty("Precision", moneyAttr.Precision));
                result.Properties.Add(new PSNoteProperty("PrecisionSource", moneyAttr.PrecisionSource));
                result.Properties.Add(new PSNoteProperty("ImeMode", moneyAttr.ImeMode?.ToString()));
                result.Properties.Add(new PSNoteProperty("CalculationOf", moneyAttr.CalculationOf));
            }
            // DateTime attributes
            else if (attr is DateTimeAttributeMetadata dateTimeAttr)
            {
                result.Properties.Add(new PSNoteProperty("Format", dateTimeAttr.Format?.ToString()));
                result.Properties.Add(new PSNoteProperty("DateTimeBehavior", dateTimeAttr.DateTimeBehavior?.Value));
                result.Properties.Add(new PSNoteProperty("ImeMode", dateTimeAttr.ImeMode?.ToString()));
                result.Properties.Add(new PSNoteProperty("CanChangeDateTimeBehavior", dateTimeAttr.CanChangeDateTimeBehavior?.Value));
            }
            // Picklist (OptionSet) attributes
            else if (attr is PicklistAttributeMetadata picklistAttr)
            {
                result.Properties.Add(new PSNoteProperty("OptionSet", ConvertOptionSetToPSObject(picklistAttr.OptionSet)));
                result.Properties.Add(new PSNoteProperty("DefaultFormValue", picklistAttr.DefaultFormValue));
            }
            // MultiSelectPicklist attributes
            else if (attr is MultiSelectPicklistAttributeMetadata multiPicklistAttr)
            {
                result.Properties.Add(new PSNoteProperty("OptionSet", ConvertOptionSetToPSObject(multiPicklistAttr.OptionSet)));
                result.Properties.Add(new PSNoteProperty("DefaultFormValue", multiPicklistAttr.DefaultFormValue));
            }
            // Boolean attributes
            else if (attr is BooleanAttributeMetadata booleanAttr)
            {
                result.Properties.Add(new PSNoteProperty("DefaultValue", booleanAttr.DefaultValue));
                if (booleanAttr.OptionSet != null)
                {
                    var optionSet = new PSObject();
                    optionSet.Properties.Add(new PSNoteProperty("TrueOption", new PSObject(new
                    {
                        Value = booleanAttr.OptionSet.TrueOption?.Value,
                        Label = booleanAttr.OptionSet.TrueOption?.Label?.UserLocalizedLabel?.Label
                    })));
                    optionSet.Properties.Add(new PSNoteProperty("FalseOption", new PSObject(new
                    {
                        Value = booleanAttr.OptionSet.FalseOption?.Value,
                        Label = booleanAttr.OptionSet.FalseOption?.Label?.UserLocalizedLabel?.Label
                    })));
                    result.Properties.Add(new PSNoteProperty("OptionSet", optionSet));
                }
            }
            // Lookup attributes
            else if (attr is LookupAttributeMetadata lookupAttr)
            {
                result.Properties.Add(new PSNoteProperty("Targets", lookupAttr.Targets));
            }
            // State attributes
            else if (attr is StateAttributeMetadata stateAttr)
            {
                result.Properties.Add(new PSNoteProperty("OptionSet", ConvertOptionSetToPSObject(stateAttr.OptionSet)));
            }
            // Status attributes
            else if (attr is StatusAttributeMetadata statusAttr)
            {
                result.Properties.Add(new PSNoteProperty("OptionSet", ConvertOptionSetToPSObject(statusAttr.OptionSet)));
            }
            // Image attributes
            else if (attr is ImageAttributeMetadata imageAttr)
            {
                result.Properties.Add(new PSNoteProperty("MaxHeight", imageAttr.MaxHeight));
                result.Properties.Add(new PSNoteProperty("MaxWidth", imageAttr.MaxWidth));
                result.Properties.Add(new PSNoteProperty("MaxSizeInKB", imageAttr.MaxSizeInKB));
                result.Properties.Add(new PSNoteProperty("CanStoreFullImage", imageAttr.CanStoreFullImage));
                result.Properties.Add(new PSNoteProperty("IsPrimaryImage", imageAttr.IsPrimaryImage));
            }
            // File attributes
            else if (attr is FileAttributeMetadata fileAttr)
            {
                result.Properties.Add(new PSNoteProperty("MaxSizeInKB", fileAttr.MaxSizeInKB));
            }
            // EntityName attributes
            else if (attr is EntityNameAttributeMetadata entityNameAttr)
            {
                // EntityName attributes don't have additional properties
            }
            // Managed Property attributes
            else if (attr is ManagedPropertyAttributeMetadata managedPropertyAttr)
            {
                result.Properties.Add(new PSNoteProperty("ManagedPropertyLogicalName", managedPropertyAttr.ManagedPropertyLogicalName));
                result.Properties.Add(new PSNoteProperty("ParentAttributeName", managedPropertyAttr.ParentAttributeName));
                result.Properties.Add(new PSNoteProperty("ParentComponentType", managedPropertyAttr.ParentComponentType));
                result.Properties.Add(new PSNoteProperty("ValueAttributeTypeCode", managedPropertyAttr.ValueAttributeTypeCode.ToString()));
            }
        }

        private PSObject ConvertOptionSetToPSObject(OptionSetMetadata optionSet)
        {
            if (optionSet == null) return null;

            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("Name", optionSet.Name));
            result.Properties.Add(new PSNoteProperty("DisplayName", optionSet.DisplayName?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("Description", optionSet.Description?.UserLocalizedLabel?.Label));
            result.Properties.Add(new PSNoteProperty("IsGlobal", optionSet.IsGlobal));
            result.Properties.Add(new PSNoteProperty("IsCustomOptionSet", optionSet.IsCustomOptionSet));
            result.Properties.Add(new PSNoteProperty("IsManaged", optionSet.IsManaged));
            result.Properties.Add(new PSNoteProperty("MetadataId", optionSet.MetadataId));
            result.Properties.Add(new PSNoteProperty("OptionSetType", optionSet.OptionSetType?.ToString()));

            if (optionSet.Options != null)
            {
                var options = optionSet.Options
                    .OrderBy(o => o.Value)
                    .Select(o => new PSObject(new
                    {
                        Value = o.Value,
                        Label = o.Label?.UserLocalizedLabel?.Label,
                        Color = o.Color,
                        Description = o.Description?.UserLocalizedLabel?.Label,
                        IsManaged = o.IsManaged
                    }))
                    .ToArray();
                result.Properties.Add(new PSNoteProperty("Options", options));
            }

            return result;
        }
    }
}
