using Microsoft.Xrm.Sdk.Messages;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Deletes a relationship from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseRelationship", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseRelationshipMetadataCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the schema name of the relationship to delete.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Schema name of the relationship to delete")]
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the related entity name (for cache invalidation).
        /// </summary>
        [Parameter(HelpMessage = "Entity name involved in the relationship (for cache invalidation). Optional.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets whether to bypass confirmation.
        /// </summary>
        [Parameter(HelpMessage = "Bypass confirmation prompt")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Build confirmation target
            string target = SchemaName;
            if (!string.IsNullOrWhiteSpace(EntityName))
            {
                target = $"{SchemaName} (involving {EntityName})";
            }

            // Check ShouldProcess with Force handling
            if (Force || ShouldProcess(target, "Delete relationship"))
            {
                DeleteRelationship();

                // Invalidate cache if entity name provided
                if (!string.IsNullOrWhiteSpace(EntityName))
                {
                    var connectionKey = MetadataCache.GetConnectionKey(Connection as Microsoft.PowerPlatform.Dataverse.Client.ServiceClient);
                    MetadataCache.InvalidateEntity(connectionKey, EntityName);
                    WriteVerbose($"Invalidated metadata cache for entity '{EntityName}'");
                }
            }
        }

        private void DeleteRelationship()
        {
            var request = new DeleteRelationshipRequest
            {
                Name = SchemaName
            };

            WriteVerbose($"Deleting relationship '{SchemaName}'");

            Connection.Execute(request);

            WriteVerbose($"Relationship '{SchemaName}' deleted successfully");
        }
    }
}
