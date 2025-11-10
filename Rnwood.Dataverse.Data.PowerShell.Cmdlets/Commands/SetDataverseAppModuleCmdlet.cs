using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates an app module (model-driven app) in Dataverse. If an app module with the specified ID or UniqueName exists, it will be updated, otherwise a new app module is created.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseAppModule", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Guid))]
    public class SetDataverseAppModuleCmdlet : OrganizationServiceCmdlet
    {
        private EntityMetadataFactory entityMetadataFactory;

        /// <summary>
        /// Gets or sets the ID of the app module to update. If not specified or if the app module doesn't exist, a new app module is created.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the app module to update. If not specified or if the app module doesn't exist, a new app module is created.")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique name of the app module. Required when creating a new app module. Can also be used to identify an existing app module for update.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Unique name of the app module. Required when creating a new app module. Can also be used to identify an existing app module for update.")]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the app module.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Display name of the app module")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the app module.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Description of the app module")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the URL of the app module.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "URL of the app module")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the web resource ID for the app module icon.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Web resource ID for the app module icon. When creating, if not specified the default 953b9fac-1e5e-e611-80d6-00155ded156f is used.")]
        public Guid? WebResourceId { get; set; }

        /// <summary>
        /// Gets or sets the form factor for the app module.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Form factor for the app module (1=Main, 2=Quick, 3=Preview, 4=Dashboard)")]
        public int? FormFactor { get; set; }

        /// <summary>
        /// Gets or sets the client type for the app module.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Client type for the app module")]
        public int? ClientType { get; set; }

        /// <summary>
        /// Gets or sets the navigation type for the app module.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Navigation type for the app module (0=Single session, 1=Multi session)")]
        public NavigationType? NavigationType { get; set; }

        /// <summary>
        /// Gets or sets whether the app module is featured.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Whether the app module is featured")]
        public bool? IsFeatured { get; set; }

        /// <summary>
        /// If specified, existing app modules matching the ID or UniqueName will not be updated.
        /// </summary>
        [Parameter(HelpMessage = "If specified, existing app modules matching the ID or UniqueName will not be updated")]
        public SwitchParameter NoUpdate { get; set; }

        /// <summary>
        /// If specified, then no app module will be created even if no existing app module matching the ID or UniqueName is found.
        /// </summary>
        [Parameter(HelpMessage = "If specified, then no app module will be created even if no existing app module matching the ID or UniqueName is found")]
        public SwitchParameter NoCreate { get; set; }

        /// <summary>
        /// If specified, returns the ID of the created or updated app module.
        /// </summary>
        [Parameter(HelpMessage = "If specified, returns the ID of the created or updated app module")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// If specified, publishes the app module after creating or updating.
        /// </summary>
        [Parameter(HelpMessage = "If specified, publishes the app module after creating or updating")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// If specified, validates the app module before publishing.
        /// </summary>
        [Parameter(HelpMessage = "If specified, validates the app module before publishing")]
        public SwitchParameter Validate { get; set; }

        /// <summary>
        /// Processes each record in the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            entityMetadataFactory = new EntityMetadataFactory(Connection);

            Entity appModuleEntity = null;
            bool isUpdate = false;
            Guid appModuleId = Id;

            // Try to retrieve existing app module by ID first
                if (Id != Guid.Empty)
                {
                    try
                    {
                        appModuleEntity = Connection.Retrieve("appmodule", Id, new ColumnSet(true));
                        isUpdate = true;
                        WriteVerbose($"Found existing app module with ID: {Id}");
                    }
                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        if (QueryHelpers.IsNotFoundException(ex))
                        {
                            WriteVerbose($"App module with ID {Id} not found");
                            // Try to retrieve unpublished version
                            try
                            {
                                var retrieveUnpublishedRequest = new RetrieveUnpublishedRequest
                                {
                                    Target = new EntityReference("appmodule", Id),
                                    ColumnSet = new ColumnSet(true)
                                };
                                var response = (RetrieveUnpublishedResponse)Connection.Execute(retrieveUnpublishedRequest);
                                appModuleEntity = response.Entity;
                                isUpdate = true;
                                WriteVerbose($"Found existing unpublished app module with ID: {Id}");
                            }
                            catch (FaultException<OrganizationServiceFault> ex2)
                            {
                                if (QueryHelpers.IsNotFoundException(ex2))
                                {
                                    WriteVerbose($"Unpublished app module with ID {Id} not found");
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                // If not found by ID and UniqueName is provided, try to retrieve by UniqueName
                if (!isUpdate && !string.IsNullOrEmpty(UniqueName))
                {
                    var query = new QueryExpression("appmodule")
                    {
                        ColumnSet = new ColumnSet(true),
                        Criteria = new FilterExpression()
                    };
                    query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, UniqueName);

                    var results = Connection.RetrieveMultiple(query);
                    if (results.Entities.Count == 0)
                    {
                        var retrieveUnpublishedMultipleRequest = new RetrieveUnpublishedMultipleRequest
                        {
                            Query = query
                        };
                        var unpublishedResponse = (RetrieveUnpublishedMultipleResponse)Connection.Execute(retrieveUnpublishedMultipleRequest);
                        results = unpublishedResponse.EntityCollection;
                    }

                    if (results.Entities.Count > 0)
                    {
                        appModuleEntity = results.Entities[0];
                        appModuleId = appModuleEntity.Id;
                        isUpdate = true;
                        WriteVerbose($"Found existing app module with UniqueName: {UniqueName}, ID: {appModuleId}");
                    }
                }

                if (isUpdate)
                {
                    // Update existing app module
                    if (NoUpdate)
                    {
                        WriteVerbose("NoUpdate flag specified, skipping update");
                        if (PassThru)
                        {
                            WriteObject(appModuleId);
                        }
                        return;
                    }

                    // Ensure we have the entity to update
                    if (appModuleEntity == null)
                    {
                        WriteError(new ErrorRecord(
                            new InvalidOperationException("App module entity not found for update"),
                            "AppModuleEntityNotFound",
                            ErrorCategory.ObjectNotFound,
                            appModuleId));
                        return;
                    }

                    if (ShouldProcess($"App module with ID '{appModuleId}'", "Update"))
                    {
                        Entity updateEntity = new Entity("appmodule") { Id = appModuleId };
                        bool updated = false;

                        // Update UniqueName if provided and different
                        if (!string.IsNullOrEmpty(UniqueName))
                        {
                            string currentUniqueName = appModuleEntity.GetAttributeValue<string>("uniquename");
                            if (currentUniqueName != UniqueName)
                            {
                                updateEntity["uniquename"] = UniqueName;
                                updated = true;
                            }
                        }

                        // Update Name if provided and different
                        if (!string.IsNullOrEmpty(Name))
                        {
                            string currentName = appModuleEntity.GetAttributeValue<string>("name");
                            if (currentName != Name)
                            {
                                updateEntity["name"] = Name;
                                updated = true;
                            }
                        }

                        // Update Description if provided and different
                        if (!string.IsNullOrEmpty(Description))
                        {
                            string currentDescription = appModuleEntity.GetAttributeValue<string>("description");
                            if (currentDescription != Description)
                            {
                                updateEntity["description"] = Description;
                                updated = true;
                            }
                        }

                        // Update Url if provided and different
                        if (!string.IsNullOrEmpty(Url))
                        {
                            string currentUrl = appModuleEntity.GetAttributeValue<string>("url");
                            if (currentUrl != Url)
                            {
                                updateEntity["url"] = Url;
                                updated = true;
                            }
                        }

                        // Update WebResourceId if provided and different
                        if (WebResourceId.HasValue)
                        {
                            Guid? currentWebResourceId = appModuleEntity.Contains("webresourceid") 
                                ? appModuleEntity.GetAttributeValue<Guid?>("webresourceid") 
                                : null;
                            if (currentWebResourceId != WebResourceId.Value)
                            {
                                updateEntity["webresourceid"] = WebResourceId;
                                updated = true;
                            }
                        }

                        // Update FormFactor if provided and different
                        if (FormFactor.HasValue)
                        {
                            int? currentFormFactor = appModuleEntity.GetAttributeValue<int?>("formfactor");
                            if (currentFormFactor != FormFactor.Value)
                            {
                                updateEntity["formfactor"] = FormFactor;
                                updated = true;
                            }
                        }

                        // Update ClientType if provided and different
                        if (ClientType.HasValue)
                        {
                            int? currentClientType = appModuleEntity.GetAttributeValue<int?>("clienttype");
                            if (currentClientType != ClientType.Value)
                            {
                                updateEntity["clienttype"] = ClientType;
                                updated = true;
                            }
                        }

                        // Update NavigationType if provided and different
                        if (NavigationType.HasValue)
                        {
                            OptionSetValue currentNavigationType = appModuleEntity.GetAttributeValue<OptionSetValue>("navigationtype");
                            if (currentNavigationType?.Value != (int) NavigationType.Value)
                            {
                                updateEntity["navigationtype"] = NavigationType.HasValue? new OptionSetValue( (int)NavigationType.Value) : null;
                                updated = true;
                            }
                        }

                        // Update IsFeatured if provided and different
                        if (IsFeatured.HasValue)
                        {
                            bool? currentIsFeatured = appModuleEntity.GetAttributeValue<bool?>("isfeatured");
                            if (currentIsFeatured != IsFeatured.Value)
                            {
                                updateEntity["isfeatured"] = IsFeatured;
                                updated = true;
                            }
                        }

                        if (updated)
                        {
                            var converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                            string columnSummary = QueryHelpers.GetColumnSummary(updateEntity, converter, false);
                            Connection.Update(updateEntity);
                            WriteVerbose($"Updated app module with ID: {appModuleId} columns:\n{columnSummary}");
                        }
                        else
                        {
                            WriteWarning("No modifications specified. App module was not updated.");
                        }

                        if (PassThru)
                        {
                            WriteObject(appModuleId);
                        }
                    }
                }
                else
                {
                    // Create new app module
                    if (NoCreate)
                    {
                        WriteVerbose("NoCreate flag specified and app module not found, skipping creation");
                        return;
                    }

                    // Validate required parameters for creation
                    if (string.IsNullOrEmpty(UniqueName))
                    {
                        throw new ArgumentException("UniqueName is required when creating a new app module");
                    }

                    if (ShouldProcess($"App module with UniqueName '{UniqueName}'", "Create"))
                    {
                        Entity newEntity = new Entity("appmodule");

                        if (Id != Guid.Empty)
                        {
                            newEntity.Id = Id;
                        }

                        newEntity["uniquename"] = UniqueName;

                        // If Name not provided, use UniqueName as default
                        newEntity["name"] = !string.IsNullOrEmpty(Name) ? Name : UniqueName;

                        if (!string.IsNullOrEmpty(Description))
                        {
                            newEntity["description"] = Description;
                        }

                        if (!string.IsNullOrEmpty(Url))
                        {
                            newEntity["url"] = Url;
                        }

                        newEntity["webresourceid"] = WebResourceId.HasValue 
                            ? WebResourceId.Value 
                            : new Guid("953b9fac-1e5e-e611-80d6-00155ded156f");

                        if (FormFactor.HasValue)
                        {
                            newEntity["formfactor"] = FormFactor.Value;
                        }

                        if (ClientType.HasValue)
                        {
                            newEntity["clienttype"] = ClientType.Value;
                        }

                        if (NavigationType.HasValue)
                        {
                            newEntity["navigationtype"] = new OptionSetValue((int)NavigationType.Value);
                        }

                        if (IsFeatured.HasValue)
                        {
                            newEntity["isfeatured"] = IsFeatured.Value;
                        }

                        var converter = new DataverseEntityConverter(Connection, entityMetadataFactory);
                        string columnSummary = QueryHelpers.GetColumnSummary(newEntity, converter, false);
                        appModuleId = Connection.Create(newEntity);
                        WriteVerbose($"Created new app module with ID: {appModuleId} columns:\n{columnSummary}");

                        if (PassThru)
                        {
                            WriteObject(appModuleId);
                        }
                    }
                }

                // Validate the app module if specified
                if (Validate)
                {
                    var validateRequest = new ValidateAppRequest
                    {
                        AppModuleId = appModuleId
                    };
                    var validateResponse = (ValidateAppResponse)Connection.Execute(validateRequest);
                    foreach (var issue in validateResponse.AppValidationResponse.ValidationIssueList)
                    {
         
                        if (issue.ErrorType == ErrorType.Error)
                        {
                            WriteError(new ErrorRecord(new Exception(issue.Message), "ValidationError", ErrorCategory.InvalidData, null));
                        }
                        else
                        {
                            WriteWarning($"Validation warning: {issue.Message}");
                        }
                    }
                }

                // Publish the app module if specified
                if (Publish && ShouldProcess($"App module with ID '{appModuleId}'", "Publish"))
                {
                    var publishRequest = new PublishXmlRequest
                    {
                        // Setting the ParameterXml to an empty string to publish all changes
                        ParameterXml = $"<importexportxml><appmodules><appmodule>{appModuleId}</appmodule></appmodules></importexportxml>"
                    };
                    Connection.Execute(publishRequest);
                    WriteVerbose($"Published app module with ID: {appModuleId}");
                }
        }
    }
}
