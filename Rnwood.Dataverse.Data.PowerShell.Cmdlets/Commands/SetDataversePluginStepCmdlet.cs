using Microsoft.Xrm.Sdk;
using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates an SDK message processing step (plugin step) in a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataversePluginStep", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataversePluginStepCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the plugin step to update.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the plugin step to update. If not specified, a new step is created.")]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the plugin step.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Name of the plugin step")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the plugin type ID this step executes.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Plugin type ID this step executes")]
        public Guid PluginTypeId { get; set; }

        /// <summary>
        /// Gets or sets the SDK message ID this step is registered for.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "SDK message ID this step is registered for")]
        public Guid SdkMessageId { get; set; }

        /// <summary>
        /// Gets or sets the SDK message filter ID (for entity-specific messages).
        /// </summary>
        [Parameter(HelpMessage = "SDK message filter ID (for entity-specific messages)")]
        public Guid? SdkMessageFilterId { get; set; }

        /// <summary>
        /// Gets or sets the stage of execution. 10=PreValidation, 20=PreOperation, 40=PostOperation, 50=PostOperationDeprecated
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Stage of execution: 10=PreValidation, 20=PreOperation, 40=PostOperation, 50=PostOperationDeprecated")]
        public int Stage { get; set; }

        /// <summary>
        /// Gets or sets the execution mode. 0=Synchronous, 1=Asynchronous
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Execution mode: 0=Synchronous, 1=Asynchronous")]
        public int Mode { get; set; }

        /// <summary>
        /// Gets or sets the execution order (rank).
        /// </summary>
        [Parameter(HelpMessage = "Execution order (rank). Default is 1.")]
        public int Rank { get; set; } = 1;

        /// <summary>
        /// Gets or sets the description of the step.
        /// </summary>
        [Parameter(HelpMessage = "Description of the step")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the filtering attributes (comma-separated list).
        /// </summary>
        [Parameter(HelpMessage = "Filtering attributes (comma-separated list of attribute logical names)")]
        public string FilteringAttributes { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the plugin step.
        /// </summary>
        [Parameter(HelpMessage = "Configuration for the plugin step (unsecure configuration)")]
        public string Configuration { get; set; }

        /// <summary>
        /// Gets or sets the secure configuration for the plugin step.
        /// </summary>
        [Parameter(HelpMessage = "Secure configuration for the plugin step")]
        public Guid? SecureConfigurationId { get; set; }

        /// <summary>
        /// Gets or sets the impersonating user ID.
        /// </summary>
        [Parameter(HelpMessage = "Impersonating user ID (runs as this user)")]
        public Guid? ImpersonatingUserId { get; set; }

        /// <summary>
        /// Gets or sets the state of the step. 0=Enabled, 1=Disabled
        /// </summary>
        [Parameter(HelpMessage = "State of the step: 0=Enabled, 1=Disabled")]
        public int? StateCode { get; set; }

        /// <summary>
        /// Gets or sets the status of the step. 1=Enabled, 2=Disabled
        /// </summary>
        [Parameter(HelpMessage = "Status of the step: 1=Enabled, 2=Disabled")]
        public int? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the supported deployment. 0=ServerOnly, 1=MicrosoftDynamics365Client, 2=Both
        /// </summary>
        [Parameter(HelpMessage = "Supported deployment: 0=ServerOnly, 1=MicrosoftDynamics365Client, 2=Both. Default is 0.")]
        public int SupportedDeployment { get; set; } = 0;

        /// <summary>
        /// If specified, the created/updated step is written to the pipeline as a PSObject.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the created/updated step is written to the pipeline as a PSObject")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Entity step = new Entity("sdkmessageprocessingstep");
            if (Id.HasValue)
            {
                step.Id = Id.Value;
            }

            step["name"] = Name;
            step["plugintypeid"] = new EntityReference("plugintype", PluginTypeId);
            step["sdkmessageid"] = new EntityReference("sdkmessage", SdkMessageId);
            step["stage"] = new OptionSetValue(Stage);
            step["mode"] = new OptionSetValue(Mode);
            step["rank"] = Rank;
            step["supporteddeployment"] = new OptionSetValue(SupportedDeployment);

            if (SdkMessageFilterId.HasValue)
            {
                step["sdkmessagefilterid"] = new EntityReference("sdkmessagefilter", SdkMessageFilterId.Value);
            }

            if (!string.IsNullOrEmpty(Description))
            {
                step["description"] = Description;
            }

            if (!string.IsNullOrEmpty(FilteringAttributes))
            {
                step["filteringattributes"] = FilteringAttributes;
            }

            if (!string.IsNullOrEmpty(Configuration))
            {
                step["configuration"] = Configuration;
            }

            if (SecureConfigurationId.HasValue)
            {
                step["sdkmessageprocessingstepsecureconfigid"] = new EntityReference("sdkmessageprocessingstepsecureconfig", SecureConfigurationId.Value);
            }

            if (ImpersonatingUserId.HasValue)
            {
                step["impersonatinguserid"] = new EntityReference("systemuser", ImpersonatingUserId.Value);
            }

            if (StateCode.HasValue)
            {
                step["statecode"] = new OptionSetValue(StateCode.Value);
            }

            if (StatusCode.HasValue)
            {
                step["statuscode"] = new OptionSetValue(StatusCode.Value);
            }

            if (ShouldProcess($"Plugin Step: {Name}", Id.HasValue ? "Update" : "Create"))
            {
                Guid stepId;
                if (Id.HasValue)
                {
                    Connection.Update(step);
                    stepId = Id.Value;
                    WriteVerbose($"Updated plugin step: {Name} (ID: {stepId})");
                }
                else
                {
                    stepId = Connection.Create(step);
                    WriteVerbose($"Created plugin step: {Name} (ID: {stepId})");
                }

                if (PassThru)
                {
                    Entity retrieved = Connection.Retrieve("sdkmessageprocessingstep", stepId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    EntityMetadataFactory entityMetadataFactory = new EntityMetadataFactory(Connection);
                    DataverseEntityConverter converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                    PSObject psObject = converter.ConvertToPSObject(retrieved, new Microsoft.Xrm.Sdk.Query.ColumnSet(true), _ => ValueType.Display);
                    WriteObject(psObject);
                }
            }
        }
    }
}
