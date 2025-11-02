using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Sets environment variable values in Dataverse. Requires the environment variable definition to already exist.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseEnvironmentVariableValue", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = "Single")]
    public class SetDataverseEnvironmentVariableValueCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the schema name of the environment variable to set the value for (for single parameter set).
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Single", HelpMessage = "Schema name of the environment variable to set the value for.")]
        [ValidateNotNullOrEmpty]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the value to set for the environment variable (for single parameter set).
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Single", HelpMessage = "Value to set for the environment variable.")]
        [AllowEmptyString]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets environment variable values as a hashtable (for multiple parameter set).
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Multiple", HelpMessage = "Hashtable of environment variable schema names to values (e.g., @{'new_apiurl' = 'https://api.example.com'}).")]
        [ValidateNotNullOrEmpty]
        public Hashtable EnvironmentVariableValues { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Dictionary<string, string> variablesToSet = new Dictionary<string, string>();

            // Build the list of variables to set based on parameter set
            if (ParameterSetName == "Single")
            {
                if (!ShouldProcess($"Environment variable value for '{SchemaName}'", "Set"))
                {
                    return;
                }

                variablesToSet[SchemaName] = Value;
            }
            else // Multiple
            {
                if (!ShouldProcess($"{EnvironmentVariableValues.Count} environment variable value(s)", "Set"))
                {
                    return;
                }

                foreach (DictionaryEntry entry in EnvironmentVariableValues)
                {
                    var schemaName = entry.Key.ToString();
                    var value = entry.Value?.ToString() ?? string.Empty;
                    variablesToSet[schemaName] = value;
                }
            }

            WriteVerbose($"Setting {variablesToSet.Count} environment variable value(s)...");

            // Query for existing environment variable values by schema name
            var existingEnvVarValuesBySchemaName = GetExistingEnvironmentVariableValueIds(variablesToSet.Keys.ToList());

            // Process each environment variable
            foreach (var kvp in variablesToSet)
            {
                var schemaName = kvp.Key;
                var value = kvp.Value;

                WriteVerbose($"Setting environment variable value for '{schemaName}' to '{value}'");

                // Query for the environment variable definition by schema name
                var defQuery = new QueryExpression("environmentvariabledefinition")
                {
                    ColumnSet = new ColumnSet("environmentvariabledefinitionid", "schemaname", "displayname"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("schemaname", ConditionOperator.Equal, schemaName)
                        }
                    },
                    TopCount = 1
                };

                var defResults = Connection.RetrieveMultiple(defQuery);

                if (defResults.Entities.Count == 0)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Environment variable definition with schema name '{schemaName}' not found. Use Set-DataverseEnvironmentVariableDefinition to create the definition first."),
                        "EnvironmentVariableDefinitionNotFound",
                        ErrorCategory.ObjectNotFound,
                        schemaName));
                    continue;
                }

                var envVarDef = defResults.Entities[0];
                var envVarDefId = envVarDef.Id;
                var displayName = envVarDef.GetAttributeValue<string>("displayname");

                WriteVerbose($"  Found environment variable definition: '{displayName}' (ID: {envVarDefId})");

                // Check if there's an existing value record
                if (existingEnvVarValuesBySchemaName.TryGetValue(schemaName, out var existingValueId))
                {
                    WriteVerbose($"  Updating existing value record (ID: {existingValueId})");

                    // Update existing value record
                    var updateEntity = new Entity("environmentvariablevalue", existingValueId);
                    updateEntity["value"] = value;

                    Connection.Update(updateEntity);
                    WriteVerbose($"  Successfully updated environment variable value for '{schemaName}'");
                }
                else
                {
                    WriteVerbose($"  Creating new value record");

                    // Create new value record
                    var createEntity = new Entity("environmentvariablevalue");
                    createEntity["schemaname"] = schemaName;
                    createEntity["value"] = value;
                    createEntity["environmentvariabledefinitionid"] = new EntityReference("environmentvariabledefinition", envVarDefId);

                    var newValueId = Connection.Create(createEntity);
                    WriteVerbose($"  Successfully created environment variable value for '{schemaName}' (ID: {newValueId})");
                }
            }
        }

        /// <summary>
        /// Gets existing environment variable value IDs by schema name.
        /// </summary>
        private Dictionary<string, Guid> GetExistingEnvironmentVariableValueIds(List<string> schemaNames)
        {
            var result = new Dictionary<string, Guid>();

            if (schemaNames == null || schemaNames.Count == 0)
            {
                return result;
            }

            WriteVerbose($"Querying for existing environment variable values for {schemaNames.Count} schema name(s)...");

            // Query for environment variable values by schema name
            var query = new QueryExpression("environmentvariablevalue")
            {
                ColumnSet = new ColumnSet("environmentvariablevalueid", "schemaname"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("schemaname", ConditionOperator.In, schemaNames.ToArray())
                    }
                },
                PageInfo = new PagingInfo { PageNumber = 1, Count = 5000 }
            };

            var allResults = new List<Entity>();
            EntityCollection ec;
            do
            {
                ec = Connection.RetrieveMultiple(query);
                allResults.AddRange(ec.Entities);
                if (ec.MoreRecords)
                {
                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = ec.PagingCookie;
                }
            } while (ec.MoreRecords);

            foreach (var entity in allResults)
            {
                if (entity.Contains("schemaname"))
                {
                    var schemaName = entity.GetAttributeValue<string>("schemaname");
                    result[schemaName] = entity.Id;
                    WriteVerbose($"  Found existing value for '{schemaName}': {entity.Id}");
                }
            }

            WriteVerbose($"Found {result.Count} existing environment variable value(s).");
            return result;
        }
    }
}
