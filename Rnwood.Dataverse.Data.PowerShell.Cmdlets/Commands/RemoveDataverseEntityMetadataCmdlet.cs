using Microsoft.Xrm.Sdk.Messages;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Deletes an entity (table) from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseEntityMetadata", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class RemoveDataverseEntityMetadataCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity to delete.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity (table) to delete")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("TableName")]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to bypass confirmation prompts.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Bypass confirmation prompts")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!ShouldProcess($"Entity '{EntityName}'", "Delete entity and all its data"))
            {
                return;
            }

            if (!Force && !ShouldContinue($"Are you sure you want to delete entity '{EntityName}'? This will delete all data and cannot be undone.", "Confirm Deletion"))
            {
                return;
            }

            var request = new DeleteEntityRequest
            {
                LogicalName = EntityName
            };

            WriteVerbose($"Deleting entity '{EntityName}'");

            Connection.Execute(request);

            WriteVerbose($"Entity '{EntityName}' deleted successfully");

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
