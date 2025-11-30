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
    /// Gets environment variable values from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseEnvironmentVariableValue")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseEnvironmentVariableValueCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the schema name of the environment variable to retrieve values for.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Schema name of the environment variable to retrieve values for.")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Build query for environment variable values
            // Join with environmentvariabledefinition to get the reliable schemaname from the definition,
            // since the schemaname field in environmentvariablevalue may not be reliably populated
            // (older records may contain a GUID instead of the actual schema name).
            var query = new QueryExpression("environmentvariablevalue")
            {
                ColumnSet = new ColumnSet("environmentvariablevalueid", "schemaname", "value", "environmentvariabledefinitionid"),
                PageInfo = new PagingInfo { PageNumber = 1, Count = 5000 }
            };

            // Add link to environmentvariabledefinition to get the reliable schema name
            var defLink = query.AddLink("environmentvariabledefinition", "environmentvariabledefinitionid", "environmentvariabledefinitionid");
            defLink.Columns = new ColumnSet("schemaname");
            defLink.EntityAlias = "def";

            if (!string.IsNullOrEmpty(SchemaName))
            {
                // Filter by the definition's schemaname, not the value's schemaname
                if (SchemaName.Contains("*") || SchemaName.Contains("?"))
                {
                    defLink.LinkCriteria.AddCondition("schemaname", ConditionOperator.Like, SchemaName.Replace("*", "%").Replace("?", "_"));
                }
                else
                {
                    defLink.LinkCriteria.AddCondition("schemaname", ConditionOperator.Equal, SchemaName);
                }
            }

            WriteVerbose("Querying for environment variable values...");

            var allValues = new List<Entity>();
            EntityCollection ec;
            do
            {
                ec = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, query);
                allValues.AddRange(ec.Entities);
                if (ec.MoreRecords)
                {
                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = ec.PagingCookie;
                }
            } while (ec.MoreRecords);

            WriteVerbose($"Found {allValues.Count} environment variable value(s)");

            // Output results
            foreach (var value in allValues)
            {
                var result = new PSObject();
                result.Properties.Add(new PSNoteProperty("EnvironmentVariableValueId", value.Id));
                
                // Use the schema name from the linked definition entity (via alias) for reliability
                var schemaNameAlias = value.GetAttributeValue<AliasedValue>("def.schemaname");
                var schemaName = schemaNameAlias?.Value as string;
                result.Properties.Add(new PSNoteProperty("SchemaName", schemaName));
                result.Properties.Add(new PSNoteProperty("Value", value.GetAttributeValue<string>("value")));
                
                var defRef = value.GetAttributeValue<EntityReference>("environmentvariabledefinitionid");
                result.Properties.Add(new PSNoteProperty("EnvironmentVariableDefinitionId", defRef?.Id));

                WriteObject(result);
            }
        }
    }
}
