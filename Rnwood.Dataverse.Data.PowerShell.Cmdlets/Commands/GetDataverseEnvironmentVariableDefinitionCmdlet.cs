using System;
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
    /// Gets environment variable definitions from Dataverse. The Type property in the output shows the human-readable label instead of the numeric value.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseEnvironmentVariableDefinition")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseEnvironmentVariableDefinitionCmdlet : OrganizationServiceCmdlet
    {
        private EntityMetadataFactory entityMetadataFactory;
        private DataverseEntityConverter entityConverter;
        /// <summary>
        /// Gets or sets the schema name of the environment variable to retrieve.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Schema name of the environment variable to retrieve.")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the display name filter for environment variables.
        /// </summary>
        [Parameter(HelpMessage = "Display name filter for environment variables (supports wildcards).")]
        public string DisplayName { get; set; }

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

            // Build query for environment variable definitions
            var defQuery = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("environmentvariabledefinitionid", "schemaname", "displayname", "description", "type", "defaultvalue"),
                PageInfo = new PagingInfo { PageNumber = 1, Count = 5000 }
            };

            var filter = new FilterExpression();

            if (!string.IsNullOrEmpty(SchemaName))
            {
                if (SchemaName.Contains("*") || SchemaName.Contains("?"))
                {
                    filter.AddCondition("schemaname", ConditionOperator.Like, SchemaName.Replace("*", "%").Replace("?", "_"));
                }
                else
                {
                    filter.AddCondition("schemaname", ConditionOperator.Equal, SchemaName);
                }
            }

            if (!string.IsNullOrEmpty(DisplayName))
            {
                if (DisplayName.Contains("*") || DisplayName.Contains("?"))
                {
                    filter.AddCondition("displayname", ConditionOperator.Like, DisplayName.Replace("*", "%").Replace("?", "_"));
                }
                else
                {
                    filter.AddCondition("displayname", ConditionOperator.Equal, DisplayName);
                }
            }

            if (filter.Conditions.Count > 0)
            {
                defQuery.Criteria = filter;
            }

            WriteVerbose("Querying for environment variable definitions...");

            var allDefinitions = new List<Entity>();
            EntityCollection ec;
            do
            {
                ec = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, defQuery);
                allDefinitions.AddRange(ec.Entities);
                if (ec.MoreRecords)
                {
                    defQuery.PageInfo.PageNumber++;
                    defQuery.PageInfo.PagingCookie = ec.PagingCookie;
                }
            } while (ec.MoreRecords);

            WriteVerbose($"Found {allDefinitions.Count} environment variable definition(s)");

            // Output results
            foreach (var definition in allDefinitions)
            {
                var result = new PSObject();
                result.Properties.Add(new PSNoteProperty("EnvironmentVariableDefinitionId", definition.Id));
                result.Properties.Add(new PSNoteProperty("SchemaName", definition.GetAttributeValue<string>("schemaname")));
                result.Properties.Add(new PSNoteProperty("DisplayName", definition.GetAttributeValue<string>("displayname")));
                result.Properties.Add(new PSNoteProperty("Description", definition.GetAttributeValue<string>("description")));
                
                var typeOption = definition.GetAttributeValue<OptionSetValue>("type");
                result.Properties.Add(new PSNoteProperty("Type", GetTypeLabel(typeOption)));
                result.Properties.Add(new PSNoteProperty("DefaultValue", definition.GetAttributeValue<string>("defaultvalue")));

                WriteObject(result);
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
    }
}
