using System;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes environment variable values from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseEnvironmentVariableValue", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class RemoveDataverseEnvironmentVariableValueCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the schema name of the environment variable to remove the value for.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Schema name of the environment variable to remove the value for.")]
        [ValidateNotNullOrEmpty]
        public string SchemaName { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose($"Looking for environment variable value for '{SchemaName}'");

            // Query for the environment variable value by joining with the parent definition
            // and filtering by the definition's schemaname. This is more reliable than
            // filtering by the value's schemaname, which may contain a GUID in older records.
            var valueQuery = new QueryExpression("environmentvariablevalue")
            {
                ColumnSet = new ColumnSet("environmentvariablevalueid"),
                TopCount = 1
            };

            // Add link to environmentvariabledefinition and filter by its schemaname
            var defLink = valueQuery.AddLink("environmentvariabledefinition", "environmentvariabledefinitionid", "environmentvariabledefinitionid");
            defLink.Columns = new ColumnSet("schemaname");
            defLink.EntityAlias = "def";
            defLink.LinkCriteria.AddCondition("schemaname", ConditionOperator.Equal, SchemaName);

            var valueResults = Connection.RetrieveMultiple(valueQuery);

            if (valueResults.Entities.Count == 0)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Environment variable value with schema name '{SchemaName}' not found."),
                    "EnvironmentVariableValueNotFound",
                    ErrorCategory.ObjectNotFound,
                    SchemaName));
                return;
            }

            var valueId = valueResults.Entities[0].Id;

            if (!ShouldProcess($"Environment variable value for '{SchemaName}'", "Remove"))
            {
                return;
            }

            WriteVerbose($"Removing environment variable value (ID: {valueId})");
            Connection.Delete("environmentvariablevalue", valueId);
            WriteVerbose($"Successfully removed environment variable value for '{SchemaName}'");
        }
    }
}
