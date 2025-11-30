using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates environment variable definitions in Dataverse. The Type parameter accepts human-readable labels (String, Number, Boolean, JSON, Data Source, Secret).
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseEnvironmentVariableDefinition", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = "Single")]
    public class SetDataverseEnvironmentVariableDefinitionCmdlet : OrganizationServiceCmdlet
    {
        // Environment variable type constant for String type
        private const int EnvironmentVariableTypeString = 100000000;
        
        private EntityMetadataFactory entityMetadataFactory;
        private DataverseEntityConverter entityConverter;
        /// <summary>
        /// Gets or sets the schema name of the environment variable definition to create or update.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Schema name of the environment variable definition to create or update.")]
        [ValidateNotNullOrEmpty]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the display name for the environment variable definition.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Display name for the environment variable definition.")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description for the environment variable definition.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Description for the environment variable definition.")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the environment variable definition.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Type of the environment variable definition. Valid values are: String, Number, Boolean, JSON, Data Source, Secret. Default is String.")]
        [ValidateSet("String", "Number", "Boolean", "JSON", "Data Source", "Secret")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the default value for the environment variable definition.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Default value for the environment variable definition.")]
        [AllowEmptyString]
        public string DefaultValue { get; set; }

        /// <summary>
        /// Initializes the cmdlet and sets up required helpers.
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            entityMetadataFactory = new EntityMetadataFactory(Connection);
            entityConverter = new DataverseEntityConverter(Connection, entityMetadataFactory);
        }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!ShouldProcess($"Environment variable definition '{SchemaName}'", "Set"))
            {
                return;
            }

            // Query for the environment variable definition by schema name
            var defQuery = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("environmentvariabledefinitionid", "schemaname", "displayname", "description", "type", "defaultvalue"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("schemaname", ConditionOperator.Equal, SchemaName)
                    }
                },
                TopCount = 1
            };

            var defResults = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, defQuery);

            if (defResults.Entities.Count == 0)
            {
                WriteVerbose($"  Environment variable definition not found. Creating new definition for '{SchemaName}'");

                // Create the environment variable definition
                var defEntity = new Entity("environmentvariabledefinition");
                defEntity["schemaname"] = SchemaName;
                defEntity["displayname"] = !string.IsNullOrEmpty(DisplayName) ? DisplayName : SchemaName;
                defEntity["type"] = string.IsNullOrEmpty(Type) ? new OptionSetValue(EnvironmentVariableTypeString) : ConvertTypeStringToOptionSetValue(Type); // Text type by default
                
                if (!string.IsNullOrEmpty(Description))
                {
                    defEntity["description"] = Description;
                }

                if (DefaultValue != null)
                {
                    defEntity["defaultvalue"] = DefaultValue;
                }

                var newDefId = QueryHelpers.CreateWithThrottlingRetry(Connection, defEntity);
                WriteVerbose($"  Created environment variable definition with ID: {newDefId}");
            }
            else
            {
                var envVarDef = defResults.Entities[0];
                var envVarDefId = envVarDef.Id;
                WriteVerbose($"  Found environment variable definition: '{envVarDef.GetAttributeValue<string>("displayname")}' (ID: {envVarDefId})");

                // Update the definition if any properties have changed
                var updateEntity = new Entity("environmentvariabledefinition", envVarDefId);
                bool hasChanges = false;

                if (!string.IsNullOrEmpty(DisplayName) && DisplayName != envVarDef.GetAttributeValue<string>("displayname"))
                {
                    updateEntity["displayname"] = DisplayName;
                    hasChanges = true;
                }

                if (!string.IsNullOrEmpty(Description) && Description != envVarDef.GetAttributeValue<string>("description"))
                {
                    updateEntity["description"] = Description;
                    hasChanges = true;
                }

                if (!string.IsNullOrEmpty(Type) && Type != GetTypeLabel(envVarDef.GetAttributeValue<OptionSetValue>("type")))
                {
                    updateEntity["type"] = ConvertTypeStringToOptionSetValue(Type);
                    hasChanges = true;
                }

                if (DefaultValue != null && DefaultValue != envVarDef.GetAttributeValue<string>("defaultvalue"))
                {
                    updateEntity["defaultvalue"] = DefaultValue;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    QueryHelpers.UpdateWithThrottlingRetry(Connection, updateEntity);
                    WriteVerbose($"  Successfully updated environment variable definition '{SchemaName}'");
                }
                else
                {
                    WriteVerbose($"  No changes needed for environment variable definition '{SchemaName}'");
                }
            }
        }

        /// <summary>
        /// Gets the type label from an OptionSetValue.
        /// </summary>
        private string GetTypeLabel(OptionSetValue optionSetValue)
        {
            if (optionSetValue == null) return null;
            var entityMetadata = entityMetadataFactory.GetMetadata("environmentvariabledefinition");
            var typeAttribute = (EnumAttributeMetadata)entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == "type");
            if (typeAttribute != null)
            {
                var option = typeAttribute.OptionSet.Options.FirstOrDefault(o => o.Value == optionSetValue.Value);
                return option?.Label.UserLocalizedLabel.Label;
            }
            return optionSetValue.Value.ToString();
        }

        /// <summary>
        /// Converts a type string to an OptionSetValue.
        /// </summary>
        private OptionSetValue ConvertTypeStringToOptionSetValue(string typeString)
        {
            var entityMetadata = entityMetadataFactory.GetMetadata("environmentvariabledefinition");
            var typeAttribute = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == "type");
            return (OptionSetValue)entityConverter.ConvertToDataverseValue(entityMetadata, "type", typeAttribute, typeString, new ConvertToDataverseEntityColumnOptions());
        }
    }
}
