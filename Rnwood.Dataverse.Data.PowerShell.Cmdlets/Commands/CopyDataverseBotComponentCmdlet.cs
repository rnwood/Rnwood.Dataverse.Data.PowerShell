using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Copies/clones a Copilot Studio bot component to create a new component.
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "DataverseBotComponent", SupportsShouldProcess = true)]
    [OutputType(typeof(PSObject))]
    public class CopyDataverseBotComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the source bot component ID to copy.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Source bot component ID (GUID) to copy.")]
        public Guid BotComponentId { get; set; }

        /// <summary>
        /// Gets or sets the new name for the copied component.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Name for the new copied component.")]
        public string NewName { get; set; }

        /// <summary>
        /// Gets or sets the new schema name for the copied component.
        /// </summary>
        [Parameter(HelpMessage = "Schema name for the new copied component. If not specified, will auto-generate based on NewName.")]
        public string NewSchemaName { get; set; }

        /// <summary>
        /// Gets or sets the new description for the copied component.
        /// </summary>
        [Parameter(HelpMessage = "Description for the new copied component.")]
        public string NewDescription { get; set; }

        /// <summary>
        /// If specified, returns the newly created component.
        /// </summary>
        [Parameter(HelpMessage = "If specified, returns the newly created bot component.")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Retrieve the source component
            Entity sourceComponent = Connection.Retrieve("botcomponent", BotComponentId, new ColumnSet(true));

            if (!ShouldProcess($"Copy bot component '{sourceComponent.GetAttributeValue<string>("name")}' to new component '{NewName}'"))
            {
                return;
            }

            // Create a new entity for the copy
            Entity newComponent = new Entity("botcomponent");

            // Copy relevant attributes
            string[] attributesToCopy = new[]
            {
                "componenttype",
                "data",
                "content",
                "category",
                "language",
                "parentbotid",
                "parentbotcomponentcollectionid",
                "accentcolor",
                "helplink",
                "iconurl",
                "reusepolicy"
            };

            foreach (string attr in attributesToCopy)
            {
                if (sourceComponent.Contains(attr))
                {
                    newComponent[attr] = sourceComponent[attr];
                }
            }

            // Set new name and description
            newComponent["name"] = NewName;

            if (!string.IsNullOrEmpty(NewSchemaName))
            {
                newComponent["schemaname"] = NewSchemaName;
            }
            else
            {
                // Auto-generate schema name based on source schema name and new name
                string sourceSchemaName = sourceComponent.GetAttributeValue<string>("schemaname");
                if (!string.IsNullOrEmpty(sourceSchemaName))
                {
                    // Keep the prefix from original schema name and append a timestamp
                    string[] parts = sourceSchemaName.Split('.');
                    if (parts.Length > 1)
                    {
                        // Use timestamp to ensure uniqueness
                        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                        string sanitizedName = NewName.Replace(" ", "_").Replace("-", "_");
                        // Remove any non-alphanumeric characters except underscore
                        sanitizedName = System.Text.RegularExpressions.Regex.Replace(sanitizedName, @"[^a-zA-Z0-9_]", "");
                        newComponent["schemaname"] = $"{parts[0]}.{sanitizedName}_{timestamp}";
                    }
                    else
                    {
                        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                        string sanitizedName = NewName.Replace(" ", "_").Replace("-", "_");
                        sanitizedName = System.Text.RegularExpressions.Regex.Replace(sanitizedName, @"[^a-zA-Z0-9_]", "");
                        newComponent["schemaname"] = $"{sanitizedName}_{timestamp}";
                    }
                }
            }

            if (!string.IsNullOrEmpty(NewDescription))
            {
                newComponent["description"] = NewDescription;
            }
            else if (sourceComponent.Contains("description"))
            {
                newComponent["description"] = $"Copy of {sourceComponent.GetAttributeValue<string>("description")}";
            }

            // Create the new component
            Guid newComponentId = Connection.Create(newComponent);

            WriteVerbose($"Created new bot component with ID: {newComponentId}");

            if (PassThru)
            {
                // Retrieve and return the newly created component
                Entity createdComponent = Connection.Retrieve("botcomponent", newComponentId, new ColumnSet(true));
                var entityMetadataFactory = new EntityMetadataFactory(Connection);
                var converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                var psObject = converter.ConvertToPSObject(createdComponent, new ColumnSet(true), _ => ValueType.Raw);
                WriteObject(psObject);
            }
        }
    }
}
