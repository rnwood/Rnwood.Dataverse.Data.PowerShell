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

        private Guid _resolvedSolutionId;

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

            _resolvedSolutionId = resolvedSolutionId;

            // Extract components from the environment
            var components = SolutionComponentExtractor.ExtractEnvironmentComponents(Connection, _resolvedSolutionId);

            WriteVerbose($"Found {components.Count} root components in the solution.");

            // Output components
            foreach (var component in components)
            {
                OutputComponentAsObject(component);

                // If IncludeSubcomponents is specified, retrieve and output subcomponents
                if (IncludeSubcomponents.IsPresent && component.ComponentType == 1)
                {
                    var componentIdentifier = ComponentTypeResolver.GetComponentIdentifier(component, Connection);
                    WriteVerbose($"Retrieving subcomponents for {ComponentTypeResolver.GetComponentTypeName(Connection, component)}: {componentIdentifier}");
                    var subcomponents = GetSubcomponentsForComponent(component);
                    foreach (var subcomponent in subcomponents)
                    {
                        OutputComponentAsObject(subcomponent);
                    }
                }
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
            var displayIdentifier = ComponentTypeResolver.GetComponentIdentifier(component, Connection);
            result.Properties.Add(new PSNoteProperty("ObjectId", displayIdentifier));
            result.Properties.Add(new PSNoteProperty("ComponentType", component.ComponentType));
            result.Properties.Add(new PSNoteProperty("ComponentTypeName", ComponentTypeResolver.GetComponentTypeName(Connection, component)));
            result.Properties.Add(new PSNoteProperty("Behavior", GetBehaviorName(component.RootComponentBehavior ?? 0)));
            result.Properties.Add(new PSNoteProperty("MetadataId", component.MetadataId));
            result.Properties.Add(new PSNoteProperty("IsSubcomponent", component.IsSubcomponent));

            if (component.IsSubcomponent)
            {
                result.Properties.Add(new PSNoteProperty("ParentComponentType", component.ParentComponentType));
                result.Properties.Add(new PSNoteProperty("ParentComponentTypeName", ComponentTypeResolver.GetComponentTypeName(Connection, new SolutionComponent { ComponentType = component.ParentComponentType.GetValueOrDefault() })));
                result.Properties.Add(new PSNoteProperty("ParentTableName", component.ParentTableName));
            }

            WriteObject(result);
        }

        private List<SolutionComponent> GetSubcomponentsForComponent(SolutionComponent parentComponent)
        {
            var retriever = new SubcomponentRetriever(Connection, this, _resolvedSolutionId);
            return retriever.GetSubcomponents(parentComponent);
        }
    }
}
