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
    [OutputType(typeof(AttributeMetadata))]
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

            WriteObject(attributeMetadata);
        }
    }
}
