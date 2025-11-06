using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Management.Automation;
using System.ServiceModel;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates an app module component in Dataverse. If a component with the specified ID exists, it will be updated, otherwise a new component is created.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseAppModuleComponent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Guid))]
    public class SetDataverseAppModuleComponentCmdlet : OrganizationServiceCmdlet
    {
        private EntityMetadataFactory entityMetadataFactory;

        /// <summary>
        /// Gets or sets the ID of the app module component to update. If not specified or if the component doesn't exist, a new component is created.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the app module component to update. If not specified or if the component doesn't exist, a new component is created.")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the app module ID that this component belongs to. Required when creating a new component.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "App module ID that this component belongs to. Required when creating a new component.")]
        [Alias("AppModuleId")]
        public Guid AppModuleIdValue { get; set; }

        /// <summary>
        /// Gets or sets the object ID (component entity record ID). Required when creating a new component.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Object ID (the ID of the component entity record). Required when creating a new component.")]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the component type. Required when creating a new component.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Component type (1=Entity, 29=Business Process Flow, 60=Chart, 62=Sitemap, etc.). Required when creating a new component.")]
        public int? ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the root component behavior.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Root component behavior (0=IncludeSubcomponents, 1=DoNotIncludeSubcomponents, 2=IncludeAsShellOnly)")]
        public int? RootComponentBehavior { get; set; }

        /// <summary>
        /// Gets or sets whether this is the default component.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Whether this is the default component")]
        public bool? IsDefault { get; set; }

        /// <summary>
        /// Gets or sets whether this is a metadata component.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Whether this is a metadata component")]
        public bool? IsMetadata { get; set; }

        /// <summary>
        /// If specified, existing components matching the ID will not be updated.
        /// </summary>
        [Parameter(HelpMessage = "If specified, existing components matching the ID will not be updated")]
        public SwitchParameter NoUpdate { get; set; }

        /// <summary>
        /// If specified, then no component will be created even if no existing component matching the ID is found.
        /// </summary>
        [Parameter(HelpMessage = "If specified, then no component will be created even if no existing component matching the ID is found")]
        public SwitchParameter NoCreate { get; set; }

        /// <summary>
        /// If specified, returns the ID of the created or updated component.
        /// </summary>
        [Parameter(HelpMessage = "If specified, returns the ID of the created or updated component")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes each record in the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            entityMetadataFactory = new EntityMetadataFactory(Connection);

            try
            {
                Entity componentEntity = null;
                bool isUpdate = false;
                Guid componentId = Id;

                // Try to retrieve existing component by ID
                if (Id != Guid.Empty)
                {
                    try
                    {
                        componentEntity = Connection.Retrieve("appmodulecomponent", Id, new ColumnSet(true));
                        isUpdate = true;
                        WriteVerbose($"Found existing app module component with ID: {Id}");
                    }
                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        if (ex.HResult == -2146233088) // Object does not exist
                        {
                            WriteVerbose($"App module component with ID {Id} not found");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (isUpdate)
                {
                    // Update existing component
                    if (NoUpdate)
                    {
                        WriteVerbose("NoUpdate flag specified, skipping update");
                        if (PassThru)
                        {
                            WriteObject(componentId);
                        }
                        return;
                    }

                    if (ShouldProcess($"App module component with ID '{componentId}'", "Update"))
                    {
                        Entity updateEntity = new Entity("appmodulecomponent") { Id = componentId };
                        bool updated = false;

                        // Update RootComponentBehavior if provided and different
                        if (RootComponentBehavior.HasValue)
                        {
                            int? currentRootComponentBehavior = componentEntity.Contains("rootcomponentbehavior") 
                                ? componentEntity.GetAttributeValue<int>("rootcomponentbehavior") 
                                : (int?)null;
                            if (currentRootComponentBehavior != RootComponentBehavior.Value)
                            {
                                updateEntity["rootcomponentbehavior"] = RootComponentBehavior.Value;
                                updated = true;
                            }
                        }

                        // Update IsDefault if provided and different
                        if (IsDefault.HasValue)
                        {
                            bool currentIsDefault = componentEntity.GetAttributeValue<bool>("isdefault");
                            if (currentIsDefault != IsDefault.Value)
                            {
                                updateEntity["isdefault"] = IsDefault.Value;
                                updated = true;
                            }
                        }

                        // Update IsMetadata if provided and different
                        if (IsMetadata.HasValue)
                        {
                            bool currentIsMetadata = componentEntity.GetAttributeValue<bool>("ismetadata");
                            if (currentIsMetadata != IsMetadata.Value)
                            {
                                updateEntity["ismetadata"] = IsMetadata.Value;
                                updated = true;
                            }
                        }

                        if (updated)
                        {
                            var converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                            string columnSummary = QueryHelpers.GetColumnSummary(updateEntity, converter, false);
                            Connection.Update(updateEntity);
                            WriteVerbose($"Updated app module component with ID: {componentId} columns:\n{columnSummary}");
                        }
                        else
                        {
                            WriteWarning("No modifications specified. App module component was not updated.");
                        }

                        if (PassThru)
                        {
                            WriteObject(componentId);
                        }
                    }
                }
                else
                {
                    // Create new component
                    if (NoCreate)
                    {
                        WriteVerbose("NoCreate flag specified and component not found, skipping creation");
                        return;
                    }

                    // Validate required parameters for creation
                    if (AppModuleIdValue == Guid.Empty)
                    {
                        throw new ArgumentException("AppModuleIdValue is required when creating a new app module component");
                    }
                    if (ObjectId == Guid.Empty)
                    {
                        throw new ArgumentException("ObjectId is required when creating a new app module component");
                    }
                    if (!ComponentType.HasValue)
                    {
                        throw new ArgumentException("ComponentType is required when creating a new app module component");
                    }

                    if (ShouldProcess($"App module component for AppModuleId '{AppModuleIdValue}', ObjectId '{ObjectId}'", "Create"))
                    {
                        Entity newEntity = new Entity("appmodulecomponent");

                        if (Id != Guid.Empty)
                        {
                            newEntity.Id = Id;
                        }

                        newEntity["appmoduleidunique"] = AppModuleIdValue;
                        newEntity["objectid"] = ObjectId;
                        newEntity["componenttype"] = ComponentType.Value;

                        if (RootComponentBehavior.HasValue)
                        {
                            newEntity["rootcomponentbehavior"] = RootComponentBehavior.Value;
                        }

                        if (IsDefault.HasValue)
                        {
                            newEntity["isdefault"] = IsDefault.Value;
                        }

                        if (IsMetadata.HasValue)
                        {
                            newEntity["ismetadata"] = IsMetadata.Value;
                        }

                        var converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                        string columnSummary = QueryHelpers.GetColumnSummary(newEntity, converter, false);
                        componentId = Connection.Create(newEntity);
                        WriteVerbose($"Created new app module component with ID: {componentId} columns:\n{columnSummary}");

                        if (PassThru)
                        {
                            WriteObject(componentId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "SetDataverseAppModuleComponentError", ErrorCategory.InvalidOperation, null));
            }
        }
    }
}
