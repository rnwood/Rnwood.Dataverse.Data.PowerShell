using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Deletes an attribute (column) from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseAttribute", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
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
        /// Gets or sets whether to force deletion without confirmation.
        /// </summary>
        [Parameter(HelpMessage = "Force deletion without confirmation")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!Force && !ShouldContinue($"Are you sure you want to delete attribute '{AttributeName}' from entity '{EntityName}'? This action cannot be undone.", "Confirm Deletion"))
            {
                return;
            }

            var request = new DeleteAttributeRequest
            {
                EntityLogicalName = EntityName,
                LogicalName = AttributeName
            };

            if (!ShouldProcess($"Entity '{EntityName}'", $"Delete attribute '{AttributeName}'"))
            {
                return;
            }

            WriteVerbose($"Deleting attribute '{AttributeName}' from entity '{EntityName}'");

            Connection.Execute(request);

            WriteVerbose($"Attribute '{AttributeName}' deleted successfully");

            // Invalidate cache for this entity
            var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
            MetadataCache.InvalidateEntity(connectionKey, EntityName);
            WriteVerbose($"Invalidated metadata cache for entity '{EntityName}'");
        }
    }
}
