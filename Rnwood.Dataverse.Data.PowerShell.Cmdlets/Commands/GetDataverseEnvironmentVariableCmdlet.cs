using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Gets environment variable definitions and values from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseEnvironmentVariable")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseEnvironmentVariableCmdlet : OrganizationServiceCmdlet
    {
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
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Build query for environment variable definitions
            var defQuery = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("environmentvariabledefinitionid", "schemaname", "displayname", "description", "type", "defaultvalue")
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
                ec = Connection.RetrieveMultiple(defQuery);
                allDefinitions.AddRange(ec.Entities);
                if (ec.MoreRecords)
                {
                    defQuery.PageInfo.PageNumber++;
                    defQuery.PageInfo.PagingCookie = ec.PagingCookie;
                }
            } while (ec.MoreRecords);

            WriteVerbose($"Found {allDefinitions.Count} environment variable definition(s)");

            if (allDefinitions.Count == 0)
            {
                return;
            }

            // Query for values for all found definitions
            var definitionIds = allDefinitions.Select(d => d.Id).ToArray();
            var valueQuery = new QueryExpression("environmentvariablevalue")
            {
                ColumnSet = new ColumnSet("environmentvariablevalueid", "schemaname", "value", "environmentvariabledefinitionid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("environmentvariabledefinitionid", ConditionOperator.In, definitionIds)
                    }
                }
            };

            var allValues = new List<Entity>();
            do
            {
                ec = Connection.RetrieveMultiple(valueQuery);
                allValues.AddRange(ec.Entities);
                if (ec.MoreRecords)
                {
                    valueQuery.PageInfo.PageNumber++;
                    valueQuery.PageInfo.PagingCookie = ec.PagingCookie;
                }
            } while (ec.MoreRecords);

            WriteVerbose($"Found {allValues.Count} environment variable value(s)");

            // Create a dictionary for quick lookup
            var valuesByDefinitionId = allValues
                .Where(v => v.Contains("environmentvariabledefinitionid"))
                .GroupBy(v => v.GetAttributeValue<EntityReference>("environmentvariabledefinitionid").Id)
                .ToDictionary(g => g.Key, g => g.First());

            // Output results
            foreach (var definition in allDefinitions)
            {
                var result = new PSObject();
                result.Properties.Add(new PSNoteProperty("EnvironmentVariableDefinitionId", definition.Id));
                result.Properties.Add(new PSNoteProperty("SchemaName", definition.GetAttributeValue<string>("schemaname")));
                result.Properties.Add(new PSNoteProperty("DisplayName", definition.GetAttributeValue<string>("displayname")));
                result.Properties.Add(new PSNoteProperty("Description", definition.GetAttributeValue<string>("description")));
                
                var typeOption = definition.GetAttributeValue<OptionSetValue>("type");
                result.Properties.Add(new PSNoteProperty("Type", typeOption?.Value));
                result.Properties.Add(new PSNoteProperty("DefaultValue", definition.GetAttributeValue<string>("defaultvalue")));

                if (valuesByDefinitionId.TryGetValue(definition.Id, out var valueRecord))
                {
                    result.Properties.Add(new PSNoteProperty("Value", valueRecord.GetAttributeValue<string>("value")));
                    result.Properties.Add(new PSNoteProperty("EnvironmentVariableValueId", valueRecord.Id));
                }
                else
                {
                    result.Properties.Add(new PSNoteProperty("Value", null));
                    result.Properties.Add(new PSNoteProperty("EnvironmentVariableValueId", null));
                }

                WriteObject(result);
            }
        }
    }
}
