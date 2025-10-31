using System;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes environment variable definitions and values from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseEnvironmentVariableDefinition", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseEnvironmentVariableDefinitionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the schema name of the environment variable to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Schema name of the environment variable to remove.")]
        [ValidateNotNullOrEmpty]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets whether to remove only the value (not the definition).
        /// </summary>
        [Parameter(HelpMessage = "If specified, removes only the environment variable value, not the definition.")]
        public SwitchParameter ValueOnly { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose($"Looking for environment variable '{SchemaName}'");

            // Query for the environment variable definition
            var defQuery = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("environmentvariabledefinitionid", "schemaname", "displayname"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("schemaname", ConditionOperator.Equal, SchemaName)
                    }
                },
                TopCount = 1
            };

            var defResults = Connection.RetrieveMultiple(defQuery);

            if (defResults.Entities.Count == 0)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Environment variable definition with schema name '{SchemaName}' not found."),
                    "EnvironmentVariableDefinitionNotFound",
                    ErrorCategory.ObjectNotFound,
                    SchemaName));
                return;
            }

            var envVarDef = defResults.Entities[0];
            var envVarDefId = envVarDef.Id;
            var displayName = envVarDef.GetAttributeValue<string>("displayname");

            WriteVerbose($"Found environment variable definition: '{displayName}' (ID: {envVarDefId})");

            // Query for existing value
            var valueQuery = new QueryExpression("environmentvariablevalue")
            {
                ColumnSet = new ColumnSet("environmentvariablevalueid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("environmentvariabledefinitionid", ConditionOperator.Equal, envVarDefId)
                    }
                },
                TopCount = 1
            };

            var valueResults = Connection.RetrieveMultiple(valueQuery);

            if (ValueOnly.IsPresent)
            {
                // Remove only the value
                if (valueResults.Entities.Count > 0)
                {
                    var valueId = valueResults.Entities[0].Id;
                    
                    if (!ShouldProcess($"Environment variable value for '{SchemaName}'", "Remove"))
                    {
                        return;
                    }

                    WriteVerbose($"Removing environment variable value (ID: {valueId})");
                    Connection.Delete("environmentvariablevalue", valueId);
                    WriteVerbose($"Successfully removed environment variable value for '{SchemaName}'");
                }
                else
                {
                    WriteWarning($"No value found for environment variable '{SchemaName}'");
                }
            }
            else
            {
                // Remove both value and definition
                if (!ShouldProcess($"Environment variable '{SchemaName}' (definition and value)", "Remove"))
                {
                    return;
                }

                // Remove value first if it exists
                if (valueResults.Entities.Count > 0)
                {
                    var valueId = valueResults.Entities[0].Id;
                    WriteVerbose($"Removing environment variable value (ID: {valueId})");
                    Connection.Delete("environmentvariablevalue", valueId);
                }

                // Remove definition
                WriteVerbose($"Removing environment variable definition (ID: {envVarDefId})");
                Connection.Delete("environmentvariabledefinition", envVarDefId);
                WriteVerbose($"Successfully removed environment variable '{SchemaName}'");
            }
        }
    }
}
