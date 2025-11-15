using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Deletes an attribute (column) from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseAttributeMetadata", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseAttributeMetadataCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Logical name of the entity (table)")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the logical name of the attribute to delete.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the attribute (column) to delete")]
        [ArgumentCompleter(typeof(ColumnNameArgumentCompleter))]
        [Alias("ColumnName")]
        public string AttributeName { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!ShouldProcess($"Entity '{EntityName}'", $"Delete attribute '{AttributeName}'"))
            {
                return;
            }

            var request = new DeleteAttributeRequest
            {
                EntityLogicalName = EntityName,
                LogicalName = AttributeName
            };

            WriteVerbose($"Deleting attribute '{AttributeName}' from entity '{EntityName}'");

            Connection.Execute(request);

            WriteVerbose($"Attribute '{AttributeName}' deleted successfully");

            // Invalidate cache for this entity
            var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
            if (connectionKey != null)
            {
                MetadataCache.InvalidateEntity(connectionKey, EntityName);
                WriteVerbose($"Invalidated metadata cache for entity '{EntityName}'");
            }
        }
    }
}
