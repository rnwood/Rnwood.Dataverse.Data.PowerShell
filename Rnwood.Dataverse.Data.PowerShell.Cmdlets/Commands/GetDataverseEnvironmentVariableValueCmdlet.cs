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
            var query = new QueryExpression("environmentvariablevalue")
            {
                ColumnSet = new ColumnSet("environmentvariablevalueid", "schemaname", "value", "environmentvariabledefinitionid")
            };

            if (!string.IsNullOrEmpty(SchemaName))
            {
                if (SchemaName.Contains("*") || SchemaName.Contains("?"))
                {
                    query.Criteria.AddCondition("schemaname", ConditionOperator.Like, SchemaName.Replace("*", "%").Replace("?", "_"));
                }
                else
                {
                    query.Criteria.AddCondition("schemaname", ConditionOperator.Equal, SchemaName);
                }
            }

            WriteVerbose("Querying for environment variable values...");

            var allValues = new List<Entity>();
            EntityCollection ec;
            do
            {
                ec = Connection.RetrieveMultiple(query);
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
                result.Properties.Add(new PSNoteProperty("SchemaName", value.GetAttributeValue<string>("schemaname")));
                result.Properties.Add(new PSNoteProperty("Value", value.GetAttributeValue<string>("value")));
                
                var defRef = value.GetAttributeValue<EntityReference>("environmentvariabledefinitionid");
                result.Properties.Add(new PSNoteProperty("EnvironmentVariableDefinitionId", defRef?.Id));

                WriteObject(result);
            }
        }
    }
}
