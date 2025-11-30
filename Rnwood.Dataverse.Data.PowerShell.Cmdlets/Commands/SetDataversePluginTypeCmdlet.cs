using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a plugin type in a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataversePluginType", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataversePluginTypeCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin type to update.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin type to update. If not specified, a new type is created.")]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the plugin assembly ID this type belongs to.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Plugin assembly ID this type belongs to")]
        public Guid PluginAssemblyId { get; set; }

        /// <summary>
        /// Gets or sets the type name of the plugin type (full class name).
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Type name of the plugin type (full class name including namespace)")]
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the plugin type.
        /// </summary>
        [Parameter(HelpMessage = "Friendly name of the plugin type")]
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the plugin type.
        /// </summary>
        [Parameter(HelpMessage = "Name of the plugin type")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the plugin type.
        /// </summary>
        [Parameter(HelpMessage = "Description of the plugin type")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the workflow activity group name.
        /// </summary>
        [Parameter(HelpMessage = "Workflow activity group name (for workflow activities)")]
        public string WorkflowActivityGroupName { get; set; }

        /// <summary>
        /// If specified, the created/updated type is written to the pipeline as a PSObject.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the created/updated type is written to the pipeline as a PSObject")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Entity pluginType = new Entity("plugintype");
            if (Id.HasValue)
            {
                pluginType.Id = Id.Value;
            }

            pluginType["pluginassemblyid"] = new EntityReference("pluginassembly", PluginAssemblyId);
            pluginType["typename"] = TypeName;

            if (!string.IsNullOrEmpty(FriendlyName))
            {
                pluginType["friendlyname"] = FriendlyName;
            }

            if (!string.IsNullOrEmpty(Name))
            {
                pluginType["name"] = Name;
            }

            if (!string.IsNullOrEmpty(Description))
            {
                pluginType["description"] = Description;
            }

            if (!string.IsNullOrEmpty(WorkflowActivityGroupName))
            {
                pluginType["workflowactivitygroupname"] = WorkflowActivityGroupName;
            }

            if (ShouldProcess($"Plugin Type: {TypeName}", Id.HasValue ? "Update" : "Create"))
            {
                Guid typeId;
                if (Id.HasValue)
                {
                    QueryHelpers.UpdateWithThrottlingRetry(Connection, pluginType);
                    typeId = Id.Value;
                    WriteVerbose($"Updated plugin type: {TypeName} (ID: {typeId})");
                }
                else
                {
                    typeId = QueryHelpers.CreateWithThrottlingRetry(Connection, pluginType);
                    WriteVerbose($"Created plugin type: {TypeName} (ID: {typeId})");
                }

                if (PassThru)
                {
                    Entity retrieved = QueryHelpers.RetrieveWithThrottlingRetry(Connection, "plugintype", typeId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
                    DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                    PSObject psObject = converter.ConvertToPSObject(retrieved, new Microsoft.Xrm.Sdk.Query.ColumnSet(true), _ => ValueType.Display);
                    WriteObject(psObject);
                }
            }
        }
    }
}
