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
    /// Removes an app module component from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseAppModuleComponent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class RemoveDataverseAppModuleComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the app module component to remove.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ById", ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the app module component to remove")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the app module.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByAppModuleUniqueName", ValueFromPipelineByPropertyName = true, HelpMessage = "Unique name of the app module")]
        public string AppModuleUniqueName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the app module.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByAppModuleId", ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the app module")]
        public Guid AppModuleId { get; set; }

        /// <summary>
        /// Gets or sets the object ID of the component.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByAppModuleUniqueName", ValueFromPipelineByPropertyName = true, HelpMessage = "Object ID of the component")]
        [Parameter(Mandatory = true, ParameterSetName = "ByAppModuleId", ValueFromPipelineByPropertyName = true, HelpMessage = "Object ID of the component")]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// If specified, the cmdlet will not raise an error if the component does not exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the component does not exist")]
        public SwitchParameter IfExists { get; set; }

        /// <summary>
        /// Processes each record in the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Guid componentId = Id;

            if (ParameterSetName == "ByAppModuleUniqueName")
            {
                componentId = ResolveComponentByAppModuleUniqueName();
            }
            else if (ParameterSetName == "ByAppModuleId")
            {
                componentId = ResolveComponentByAppModuleId();
            }

            if (componentId == Guid.Empty)
            {
                return; // Already handled by the resolve methods
            }

            // For WhatIf, just show what would be done without retrieving the component
            if (!ShouldProcess($"App module component with ID '{componentId}'", "Remove"))
            {
                return;
            }

            // Retrieve the component to get the AppModuleIdUnique and ComponentType
            Entity componentEntity = RetrieveComponent(componentId);
            if (componentEntity == null)
            {
                return; // Already handled by RetrieveComponent
            }

            RemoveComponentFromApp(componentEntity, componentId);
        }

        /// <summary>
        /// Resolves the component ID by app module unique name and object ID.
        /// </summary>
        private Guid ResolveComponentByAppModuleUniqueName()
        {
            var appModuleIdUnique = ResolveAppModuleUniqueIdByUniqueName(AppModuleUniqueName);
            if (appModuleIdUnique == null)
            {
                return Guid.Empty;
            }

            return FindComponent(appModuleIdUnique.Value, ObjectId);
        }

        /// <summary>
        /// Resolves the component ID by app module ID and object ID.
        /// </summary>
        private Guid ResolveComponentByAppModuleId()
        {
            var appModuleIdUnique = ResolveAppModuleUniqueIdByAppModuleId(AppModuleId);
            if (appModuleIdUnique == null)
            {
                return Guid.Empty;
            }

            return FindComponent(appModuleIdUnique.Value, ObjectId);
        }

        /// <summary>
        /// Resolves the app module unique ID from the app module unique name.
        /// </summary>
        private Guid? ResolveAppModuleUniqueIdByUniqueName(string uniqueName)
        {
            var appModuleQuery = new QueryExpression("appmodule")
            {
                ColumnSet = new ColumnSet("appmoduleidunique"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("uniquename", ConditionOperator.Equal, uniqueName)
                    }
                },
                TopCount = 1
            };

            // First try unpublished
            var request = new RetrieveUnpublishedMultipleRequest { Query = appModuleQuery };
            var response = (RetrieveUnpublishedMultipleResponse)Connection.Execute(request);
            var appModuleResults = response.EntityCollection;

            if (appModuleResults.Entities.Count == 0)
            {
                // Try published
                var pubRequest = new RetrieveMultipleRequest { Query = appModuleQuery };
                var pubResponse = (RetrieveMultipleResponse)Connection.Execute(pubRequest);
                appModuleResults = pubResponse.EntityCollection;
            }

            if (appModuleResults.Entities.Count == 0)
            {
                if (IfExists)
                {
                    WriteVerbose($"App module with unique name '{uniqueName}' not found");
                    return null;
                }
                else
                {
                    throw new ArgumentException($"App module with unique name '{uniqueName}' not found");
                }
            }

            return appModuleResults.Entities[0].GetAttributeValue<Guid>("appmoduleidunique");
        }

        /// <summary>
        /// Resolves the app module unique ID from the app module ID.
        /// </summary>
        private Guid? ResolveAppModuleUniqueIdByAppModuleId(Guid appModuleId)
        {
            var appModuleQuery = new QueryExpression("appmodule")
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

            // First try unpublished
            var request = new RetrieveUnpublishedMultipleRequest { Query = appModuleQuery };
            var response = (RetrieveUnpublishedMultipleResponse)Connection.Execute(request);
            var appModuleResults = response.EntityCollection;

            if (appModuleResults.Entities.Count == 0)
            {
                // Try published
                var pubRequest = new RetrieveMultipleRequest { Query = appModuleQuery };
                var pubResponse = (RetrieveMultipleResponse)Connection.Execute(pubRequest);
                appModuleResults = pubResponse.EntityCollection;
            }

            if (appModuleResults.Entities.Count == 0)
            {
                if (IfExists)
                {
                    WriteVerbose($"App module with ID '{appModuleId}' not found");
                    return null;
                }
                else
                {
                    throw new ArgumentException($"App module with ID '{appModuleId}' not found");
                }
            }

            return appModuleResults.Entities[0].GetAttributeValue<Guid>("appmoduleidunique");
        }

        /// <summary>
        /// Finds a component by app module unique ID and object ID.
        /// </summary>
        private Guid FindComponent(Guid appModuleIdUnique, Guid objectId)
        {
            var componentQuery = new QueryExpression("appmodulecomponent")
            {
                ColumnSet = new ColumnSet("appmodulecomponentid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("appmoduleidunique", ConditionOperator.Equal, appModuleIdUnique),
                        new ConditionExpression("objectid", ConditionOperator.Equal, objectId)
                    }
                },
                TopCount = 1
            };

            var componentResults = Connection.RetrieveMultiple(componentQuery);
            if (componentResults.Entities.Count == 0)
            {
                if (IfExists)
                {
                    WriteVerbose($"App module component not found for app module unique ID '{appModuleIdUnique}', object ID '{objectId}'");
                    return Guid.Empty;
                }
                else
                {
                    throw new InvalidOperationException($"App module component not found for app module unique ID '{appModuleIdUnique}', object ID '{objectId}'");
                }
            }

            return componentResults.Entities[0].Id;
        }

        /// <summary>
        /// Retrieves the component entity with required attributes.
        /// </summary>
        private Entity RetrieveComponent(Guid componentId)
        {
            try
            {
                return Connection.Retrieve("appmodulecomponent", componentId, new ColumnSet("appmoduleidunique", "objectid", "componenttype"));
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (IfExists && (ex.Detail.ErrorCode == -2147220969 || ex.Message.Contains("Does Not Exist")))
                {
                    WriteVerbose($"App module component with ID {componentId} does not exist: {ex.Message}");
                    return null;
                }
                else
                {
                    throw;
                }
            }
            catch (FaultException ex)
            {
                if (IfExists && ex.Message.Contains("Does Not Exist"))
                {
                    WriteVerbose($"App module component with ID {componentId} does not exist: {ex.Message}");
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Removes the component from the app using RemoveAppComponentsRequest.
        /// </summary>
        private void RemoveComponentFromApp(Entity componentEntity, Guid componentId)
        {
            // Fix: Use appmoduleidunique, not appmoduleid
            Guid appModuleIdUnique = componentEntity.GetAttributeValue<EntityReference>("appmoduleidunique").Id;
            Guid objectId = componentEntity.GetAttributeValue<Guid>("objectid");
            int componentType = componentEntity.GetAttributeValue<OptionSetValue>("componenttype").Value;

            // Need to resolve the actual app module ID from the unique ID for the RemoveAppComponentsRequest
            Guid appModuleId = ResolveAppModuleIdFromUniqueId(appModuleIdUnique);

            var request = new RemoveAppComponentsRequest()
            {
                AppId = appModuleId,
                Components = new EntityReferenceCollection() {
                    new EntityReference(GetTableNameForComponentType((AppModuleComponentType)componentType), objectId)
                }
            };

            Connection.Execute(request);
            WriteVerbose($"Removed app module component with ID: {componentId}");
        }

        /// <summary>
        /// Resolves the app module ID from the app module unique ID.
        /// </summary>
        private Guid ResolveAppModuleIdFromUniqueId(Guid appModuleIdUnique)
        {
            var query = new QueryExpression("appmodule")
            {
                ColumnSet = new ColumnSet("appmoduleid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("appmoduleidunique", ConditionOperator.Equal, appModuleIdUnique)
                    }
                },
                TopCount = 1
            };

            var results = Connection.RetrieveMultiple(query);
            if (results.Entities.Count == 0)
            {
                throw new InvalidOperationException($"Cannot find app module with unique ID '{appModuleIdUnique}'");
            }

            return results.Entities[0].Id;
        }

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
    }
}
