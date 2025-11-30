using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Management.Automation;
using System.ServiceModel;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;

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
        /// Gets or sets the app module ID that this component belongs to. Required when creating a new component if AppModuleUniqueName is not specified.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "App module ID that this component belongs to. Required when creating a new component if AppModuleUniqueName is not specified.")]
        public Guid AppModuleId { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the app module that this component belongs to. If specified, takes precedence over AppModuleId.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Unique name of the app module that this component belongs to. If specified, takes precedence over AppModuleId.")]
        public string AppModuleUniqueName { get; set; }

        /// <summary>
        /// Gets or sets the object ID (component entity record ID). Required when creating a new component.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Object ID (the ID of the component entity record). Required when creating a new component.")]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the component type. Required when creating a new component.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Component type (Entity, View, BusinessProcessFlow, RibbonCommand, Chart, Form, SiteMap). Required when creating a new component.")]
        public AppModuleComponentType? ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the root component behavior.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Root component behavior (IncludeSubcomponents, DoNotIncludeSubcomponents, IncludeAsShell)")]
        public RootComponentBehavior? RootComponentBehavior { get; set; }

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
        /// Gets the logical name of the table for the specified component type.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <returns>The logical name of the corresponding table.</returns>
        private string GetTableNameForComponentType(AppModuleComponentType componentType)
        {
            switch (componentType)
            {
                case AppModuleComponentType.Entity:
                    return "entity";
                case AppModuleComponentType.View:
                    return "savedquery";
                case AppModuleComponentType.BusinessProcessFlow:
                    return "workflow";
                case AppModuleComponentType.RibbonCommand:
                    return "ribboncommand";
                case AppModuleComponentType.Chart:
                    return "savedqueryvisualization";
                case AppModuleComponentType.Form:
                    return "systemform";
                case AppModuleComponentType.SiteMap:
                    return "sitemap";
                default:
                    throw new ArgumentException($"Unknown component type: {componentType}");
            }
        }

        /// <summary>
        /// Processes each record in the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            entityMetadataFactory = new EntityMetadataFactory(Connection);

            Entity componentEntity = null;
            bool isUpdate = false;
            Guid componentId = Id;

            // Try to retrieve existing component by ID
            if (Id != Guid.Empty)
            {
                try
                {
                    componentEntity = QueryHelpers.RetrieveWithThrottlingRetry(Connection, "appmodulecomponent", Id, new ColumnSet(true));
                    isUpdate = true;
                    WriteVerbose($"Found existing app module component with ID: {Id}");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    if (QueryHelpers.IsNotFoundException(ex))
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
                        var updateEntity = new Entity("appmodulecomponent", componentId);
                        bool hasUpdates = false;

                        // Only update properties that were explicitly provided
                        if (RootComponentBehavior.HasValue)
                        {
                            updateEntity["rootcomponentbehavior"] = new OptionSetValue((int)RootComponentBehavior.Value);
                            hasUpdates = true;
                        }

                        if (IsDefault.HasValue)
                        {
                            updateEntity["isdefault"] = IsDefault.Value;
                            hasUpdates = true;
                        }

                        if (IsMetadata.HasValue)
                        {
                            updateEntity["ismetadata"] = IsMetadata.Value;
                            hasUpdates = true;
                        }

                        if (hasUpdates)
                        {
                            QueryHelpers.UpdateWithThrottlingRetry(Connection, updateEntity);
                            WriteVerbose($"Updated app module component with ID: {componentId}");
                        }
                        else
                        {
                            WriteVerbose("No properties specified for update");
                        }

                        if (PassThru)
                        {
                            WriteObject(componentId);
                        }
                    }
                }
                else
                {
                    // Component doesn't exist
                    
                    // If NoUpdate is specified, just exit gracefully
                    if (NoUpdate)
                    {
                        WriteVerbose("NoUpdate flag specified and component not found, skipping");
                        return;
                    }
                    
                    // Create new component using AddAppComponentsRequest
                    if (NoCreate)
                    {
                        WriteVerbose("NoCreate flag specified and component not found, skipping creation");
                        return;
                    }

                    // Check if user provided any creation parameters
                    // If they provided at least one creation param, validate all are present
                    bool hasAnyCreationParam = AppModuleId != Guid.Empty || !string.IsNullOrEmpty(AppModuleUniqueName) || 
                                               ObjectId != Guid.Empty || ComponentType.HasValue;
                    
                    Guid appModuleId = AppModuleId;
                    
                    if (hasAnyCreationParam)
                    {
                        // User is trying to create - validate all required parameters
                        if (appModuleId == Guid.Empty && string.IsNullOrEmpty(AppModuleUniqueName))
                        {
                            throw new ArgumentException("AppModuleId or AppModuleUniqueName is required when creating a new app module component");
                        }
                        if (ObjectId == Guid.Empty)
                        {
                            throw new ArgumentException("ObjectId is required when creating a new app module component");
                        }
                        if (!ComponentType.HasValue)
                        {
                            throw new ArgumentException("ComponentType is required when creating a new app module component");
                        }
                    }
                    else
                    {
                        // User only provided ID and update properties - can't create, just return gracefully
                        WriteVerbose("Component not found and no creation parameters provided - skipping");
                        return;
                    }

                    // Now resolve the AppModule ID if needed (skip lookup if WhatIf to avoid validation errors)
                    Guid appModuleIdUnique = Guid.Empty;
                    bool isWhatIf = this.MyInvocation.BoundParameters.ContainsKey("WhatIf") && 
                                    ((SwitchParameter)this.MyInvocation.BoundParameters["WhatIf"]).IsPresent;
                    
                    if (!string.IsNullOrEmpty(AppModuleUniqueName))
                    {
                        if (!isWhatIf)
                        {
                            // Query for appmodule by uniquename, preferring unpublished
                            var query = new QueryExpression("appmodule")
                            {
                                ColumnSet = new ColumnSet("appmoduleid", "appmoduleidunique"),
                                Criteria = new FilterExpression
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression("uniquename", ConditionOperator.Equal, AppModuleUniqueName)
                                    }
                                },
                                TopCount = 1
                            };

                            // First try unpublished
                            var request = new RetrieveUnpublishedMultipleRequest { Query = query };
                            var response = (RetrieveUnpublishedMultipleResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, request);
                            var results = response.EntityCollection;

                            if (results.Entities.Count == 0)
                            {
                                // Try published
                                var pubRequest = new RetrieveMultipleRequest { Query = query };
                                var pubResponse = (RetrieveMultipleResponse)QueryHelpers.ExecuteWithThrottlingRetry(Connection, pubRequest);
                                results = pubResponse.EntityCollection;
                            }

                            if (results.Entities.Count == 0)
                            {
                                throw new ArgumentException($"App module with unique name '{AppModuleUniqueName}' not found.");
                            }

                            appModuleId = results.Entities[0].Id;
                            appModuleIdUnique = results.Entities[0].GetAttributeValue<Guid>("appmoduleidunique");
                        }
                    }
                    else if (appModuleId != Guid.Empty)
                    {
                        if (!isWhatIf)
                        {
                            // If AppModuleId is provided, retrieve the appmoduleidunique for querying
                            var query = new QueryExpression("appmodule")
                            {
                                ColumnSet = new ColumnSet("appmoduleidunique"),
                                Criteria = new FilterExpression
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression("appmoduleid", ConditionOperator.Equal, appModuleId)
                                    }
                                },
                                TopCount = 1
                            };

                            var results = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, query);
                            if (results.Entities.Count > 0)
                            {
                                appModuleIdUnique = results.Entities[0].GetAttributeValue<Guid>("appmoduleidunique");
                            }
                            else
                            {
                                throw new ArgumentException($"App module with ID '{appModuleId}' not found.");
                            }
                        }
                    }

                    if (ShouldProcess($"App module component for AppModuleId '{appModuleId}', ObjectId '{ObjectId}'", "Create"))
                    {
                        // Use AddAppComponentsRequest to add the component to the app
                        var tableName = GetTableNameForComponentType(ComponentType.Value);
                        var entityReference = new EntityReference(tableName, ObjectId);

                        var addRequest = new AddAppComponentsRequest
                        {
                            AppId = appModuleId,
                            Components = new EntityReferenceCollection { entityReference }
                        };

                        QueryHelpers.ExecuteWithThrottlingRetry(Connection, addRequest);
                        WriteVerbose($"Added new app module component to app {appModuleId}");

                        // The AddAppComponentsRequest doesn't return the component ID directly,
                        // so we need to query for it if PassThru is requested
                        if (PassThru)
                        {
                            // Query to find the created component
                            var query = new QueryExpression("appmodulecomponent")
                            {
                                ColumnSet = new ColumnSet("appmodulecomponentid"),
                                Criteria = new FilterExpression
                                {
                                    Conditions =
                                    {
                                        new ConditionExpression("appmoduleidunique", ConditionOperator.Equal, appModuleIdUnique),
                                        new ConditionExpression("objectid", ConditionOperator.Equal, ObjectId),
                                        new ConditionExpression("componenttype", ConditionOperator.Equal, (int)ComponentType.Value)
                                    }
                                },
                                TopCount = 1
                            };

                            var results = QueryHelpers.RetrieveMultipleWithThrottlingRetry(Connection, query);
                            if (results.Entities.Count > 0)
                            {
                                componentId = results.Entities[0].Id;
                                WriteObject(componentId);
                            }
                            else
                            {
                                WriteWarning("Component was created but could not be retrieved for PassThru");
                            }
                        }
                    }
                }
            }
    }
}
