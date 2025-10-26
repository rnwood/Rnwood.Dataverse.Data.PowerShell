using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves the root components from a solution in a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseSolutionComponent")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseSolutionComponentCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the solution's unique name.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "ByUniqueName", HelpMessage = "The unique name of the solution.")]
        [ValidateNotNullOrEmpty]
        public string SolutionName { get; set; }

        /// <summary>
        /// Gets or sets the solution ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "BySolutionId", HelpMessage = "The ID of the solution.")]
        [ValidateNotNullOrEmpty]
        public Guid SolutionId { get; set; }

        /// <summary>
        /// Gets or sets whether to include subcomponents in the output.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Include subcomponents (attributes, relationships, forms, views, etc.) for each root component.")]
        public SwitchParameter IncludeSubcomponents { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Resolve solution ID if only name is provided
            Guid resolvedSolutionId;
            if (ParameterSetName == "ByUniqueName")
            {
                var solutionQuery = new QueryExpression("solution")
                {
                    ColumnSet = new ColumnSet("solutionid", "uniquename"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
              {
          new ConditionExpression("uniquename", ConditionOperator.Equal, SolutionName)
              }
                    },
                    TopCount = 1
                };

                var solutions = Connection.RetrieveMultiple(solutionQuery);
                if (solutions.Entities.Count == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Solution '{SolutionName}' not found in the environment."),
                        "SolutionNotFound",
                                ErrorCategory.ObjectNotFound,
                        SolutionName));
                    return;
                }

                resolvedSolutionId = solutions.Entities[0].Id;
                WriteVerbose($"Found solution '{SolutionName}' with ID: {resolvedSolutionId}");
            }
            else
            {
                resolvedSolutionId = SolutionId;
            }

            // Extract components from the environment
            var components = SolutionComponentExtractor.ExtractEnvironmentComponents(Connection, resolvedSolutionId);

            WriteVerbose($"Found {components.Count} root components in the solution.");

            // Output components
            foreach (var component in components)
            {
                OutputComponentAsObject(component);

                // If IncludeSubcomponents is specified, retrieve and output subcomponents
                if (IncludeSubcomponents.IsPresent && component.ComponentType == 1)
                {
                    var componentIdentifier = component.LogicalName ?? component.ObjectId?.ToString() ?? "Unknown";
                    WriteVerbose($"Retrieving subcomponents for {GetComponentTypeName(component.ComponentType)}: {componentIdentifier}");
                    var subcomponents = GetSubcomponentsForComponent(component);
                    foreach (var subcomponent in subcomponents)
                    {
                        OutputComponentAsObject(subcomponent);
                    }
                }
            }
        }

        private string GetComponentTypeName(int componentType)
        {
            switch (componentType)
            {
                case 1: return "Entity";
                case 2: return "Attribute";
                case 3: return "Relationship";
                case 4: return "Attribute Picklist Value";
                case 5: return "Attribute Lookup Value";
                case 6: return "View Query";
                case 7: return "Localized Label";
                case 8: return "Relationship Extra Condition";
                case 9: return "Option Set";
                case 10: return "Entity Relationship";
                case 11: return "Entity Relationship Role";
                case 12: return "Entity Relationship Relationships";
                case 13: return "Managed Property";
                case 14: return "Entity Key";
                case 16: return "Privilege";
                case 17: return "Privilege Object Type Code";
                case 18: return "Index";
                case 20: return "Role";
                case 21: return "Role Privilege";
                case 22: return "Display String";
                case 23: return "Display String Map";
                case 24: return "Form";
                case 25: return "Organization";
                case 26: return "Saved Query";
                case 29: return "Workflow";
                case 31: return "Report";
                case 32: return "Report Entity";
                case 33: return "Report Category";
                case 34: return "Report Visibility";
                case 35: return "Attachment";
                case 36: return "Email Template";
                case 37: return "Contract Template";
                case 38: return "KB Article Template";
                case 39: return "Mail Merge Template";
                case 44: return "Duplicate Rule";
                case 45: return "Duplicate Rule Condition";
                case 46: return "Entity Map";
                case 47: return "Attribute Map";
                case 48: return "Ribbon Command";
                case 49: return "Ribbon Context Group";
                case 50: return "Ribbon Customization";
                case 52: return "Ribbon Rule";
                case 53: return "Ribbon Tab To Command Map";
                case 55: return "Ribbon Diff";
                case 59: return "Saved Query Visualization";
                case 60: return "System Form";
                case 61: return "Web Resource";
                case 62: return "Site Map";
                case 63: return "Connection Role";
                case 64: return "Complex Control";
                case 65: return "Hierarchy Rule";
                case 66: return "Custom Control";
                case 68: return "Custom Control Default Config";
                case 70: return "Field Security Profile";
                case 71: return "Field Permission";
                case 90: return "Plugin Type";
                case 91: return "Plugin Assembly";
                case 92: return "SDK Message Processing Step";
                case 93: return "SDK Message Processing Step Image";
                case 95: return "Service Endpoint";
                case 150: return "Routing Rule";
                case 151: return "Routing Rule Item";
                case 152: return "SLA";
                case 153: return "SLA Item";
                case 154: return "Convert Rule";
                case 155: return "Convert Rule Item";
                case 161: return "Mobile Offline Profile";
                case 162: return "Mobile Offline Profile Item";
                case 165: return "Similarity Rule";
                case 166: return "Data Source Mapping";
                case 201: return "SDKMessage";
                case 202: return "SDKMessageFilter";
                case 203: return "SdkMessagePair";
                case 204: return "SdkMessageRequest";
                case 205: return "SdkMessageRequestField";
                case 206: return "SdkMessageResponse";
                case 207: return "SdkMessageResponseField";
                case 208: return "Import Map";
                case 210: return "WebWizard";
                case 300: return "Canvas App";
                case 371: return "Connector";
                case 372: return "Connector";
                case 380: return "Environment Variable Definition";
                case 381: return "Environment Variable Value";
                case 400: return "AI Project Type";
                case 401: return "AI Project";
                case 402: return "AI Configuration";
                case 430: return "Model-Driven App";
                default: return $"Unknown ({componentType})";
            }
        }

        private string GetBehaviorName(int behavior)
        {
            switch (behavior)
            {
                case 0: return "Include Subcomponents";
                case 1: return "Do Not Include Subcomponents";
                case 2: return "Include As Shell";
                default: return $"Unknown ({behavior})";
            }
        }

        private void OutputComponentAsObject(SolutionComponent component)
        {
            var result = new PSObject();
            var displayIdentifier = component.LogicalName ?? component.ObjectId?.ToString() ?? "Unknown";
            result.Properties.Add(new PSNoteProperty("ObjectId", displayIdentifier));
            result.Properties.Add(new PSNoteProperty("ComponentType", component.ComponentType));
            result.Properties.Add(new PSNoteProperty("ComponentTypeName", GetComponentTypeName(component.ComponentType)));
            result.Properties.Add(new PSNoteProperty("Behavior", GetBehaviorName(component.RootComponentBehavior)));
            result.Properties.Add(new PSNoteProperty("MetadataId", component.MetadataId));
            result.Properties.Add(new PSNoteProperty("IsSubcomponent", component.IsSubcomponent));

            if (component.IsSubcomponent)
            {
                result.Properties.Add(new PSNoteProperty("ParentComponentType", component.ParentComponentType));
                result.Properties.Add(new PSNoteProperty("ParentComponentTypeName", GetComponentTypeName(component.ParentComponentType.GetValueOrDefault())));
                result.Properties.Add(new PSNoteProperty("ParentObjectId", component.ParentObjectId));
            }

            WriteObject(result);
        }

        private List<SolutionComponent> GetSubcomponentsForComponent(SolutionComponent parentComponent)
        {
            var retriever = new SubcomponentRetriever(Connection, this);
            return retriever.GetSubcomponents(parentComponent);
        }
    }
}
