using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Compares a solution file with the state of that solution in the target environment.
    /// </summary>
    [Cmdlet(VerbsData.Compare, "DataverseSolutionComponents")]
    [OutputType(typeof(PSObject))]
    public class CompareDataverseSolutionComponentsCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the Dataverse connection.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "FileToEnvironment", HelpMessage = "Dataverse connection for comparing with environment.")]
        [Parameter(Mandatory = false, ParameterSetName = "BytesToEnvironment", HelpMessage = "Dataverse connection for comparing with environment.")]
        public override ServiceClient Connection { get => base.Connection; set => base.Connection = value; }

        /// <summary>
        /// Gets or sets the path to the solution file to compare.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FileToEnvironment", HelpMessage = "Path to the solution file (.zip) to compare with environment.")]
        [ValidateNotNullOrEmpty]
        public string SolutionFile { get; set; }

        /// <summary>
        /// Gets or sets the solution file bytes to compare.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "BytesToEnvironment", HelpMessage = "Solution file bytes to compare with environment.")]
        public byte[] SolutionBytes { get; set; }

        /// <summary>
        /// Switch to compare a solution file with the environment.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FileToEnvironment", HelpMessage = "Compare a solution file with the environment.")]
        public SwitchParameter FileToEnvironment { get; set; }

        /// <summary>
        /// Switch to compare solution bytes with the environment.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "BytesToEnvironment", HelpMessage = "Compare solution bytes with the environment.")]
        public SwitchParameter BytesToEnvironment { get; set; }

        /// <summary>
        /// Gets or sets whether to reverse the comparison direction (compare environment to solution instead of solution to environment).
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "FileToEnvironment", HelpMessage = "Reverse the comparison direction.")]
        [Parameter(Mandatory = false, ParameterSetName = "BytesToEnvironment", HelpMessage = "Reverse the comparison direction.")]
        public SwitchParameter ReverseComparison { get; set; }

        // Private fields to store solution file bytes for subcomponent extraction
        private byte[] _sourceSolutionBytes;

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Load solution file (source)
            byte[] sourceSolutionBytes;
            if (ParameterSetName == "BytesToEnvironment")
            {
                sourceSolutionBytes = SolutionBytes;
            }
            else
            {
                var filePath = GetUnresolvedProviderPathFromPSPath(SolutionFile);
                if (!File.Exists(filePath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                new FileNotFoundException($"Solution file not found: {filePath}"),
                        "FileNotFound",
                       ErrorCategory.ObjectNotFound,
                      filePath));
                    return;
                }

                WriteVerbose($"Loading solution file from: {filePath}");
                sourceSolutionBytes = File.ReadAllBytes(filePath);
            }

            WriteVerbose($"Solution file size: {sourceSolutionBytes.Length} bytes");

            // Store the source bytes for subcomponent extraction
            _sourceSolutionBytes = sourceSolutionBytes;

            // Extract solution info and components from source file using SolutionComponentExtractor
            var (sourceSolutionName, sourceComponents) = SolutionComponentExtractor.ExtractSolutionFileComponents(sourceSolutionBytes);
            WriteVerbose($"Extracted source solution: {sourceSolutionName} with {sourceComponents.Count} root components");

            // Query target environment for the solution
            var solutionQuery = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("solutionid", "uniquename", "friendlyname"),
                Criteria = new FilterExpression
                {
                    Conditions =
 {
new ConditionExpression("uniquename", ConditionOperator.Equal, sourceSolutionName)
   }
                },
                TopCount = 1
            };

            var solutions = Connection.RetrieveMultiple(solutionQuery);
            if (solutions.Entities.Count == 0)
            {
                WriteWarning($"Solution '{sourceSolutionName}' not found in target environment. All components will be marked as 'Added'.");

                // Output all source components as Added
                foreach (var component in sourceComponents)
                {
                    OutputComparisonResult(new SolutionComponentInfo
                    {
                        LogicalName = component.UniqueName,
                        ObjectId = component.ObjectId,
                        ComponentType = component.ComponentType,
                        RootComponentBehavior = component.RootComponentBehavior
                    }, null, SolutionComponentStatus.InSourceOnly, sourceSolutionName, isReversed: ReverseComparison.IsPresent);
                }
                return;
            }

            var solutionId = solutions.Entities[0].Id;
            WriteVerbose($"Found solution in target environment: {solutionId}");

            // Query target environment for solution components using SolutionComponentExtractor
            var targetComponents = SolutionComponentExtractor.ExtractEnvironmentComponents(Connection, solutionId);
            var targetSolutionName = sourceSolutionName;
            WriteVerbose($"Found {targetComponents.Count} components in target environment");

            // Apply reverse comparison if requested
            if (ReverseComparison.IsPresent)
            {
                WriteVerbose("Reversing comparison direction");
                var temp = sourceComponents;
                sourceComponents = targetComponents;
                targetComponents = temp;
            }

            // Compare components
            CompareComponents(sourceComponents, targetComponents, targetSolutionName, solutionId);
        }

        private void CompareComponents(List<SolutionComponent> sourceComponents,
   List<SolutionComponent> targetComponents, string solutionName, Guid? targetSolutionId)
        {
            // Expand components to include subcomponents based on behavior
            var expandedSourceComponents = ExpandComponentsWithSubcomponents(sourceComponents, isSource: true, solutionId: null);
            var expandedTargetComponents = ExpandComponentsWithSubcomponents(targetComponents, isSource: false, solutionId: targetSolutionId);

            WriteVerbose($"Expanded source components: {sourceComponents.Count} root -> {expandedSourceComponents.Count} total");
            WriteVerbose($"Expanded target components: {targetComponents.Count} root -> {expandedTargetComponents.Count} total");

            // Sort components for consistent verbose output
            expandedSourceComponents = expandedSourceComponents.OrderBy(c => c.ComponentType).ThenBy(c => GetComponentKey(c)).ToList();
            expandedTargetComponents = expandedTargetComponents.OrderBy(c => c.ComponentType).ThenBy(c => GetComponentKey(c)).ToList();

            // Output verbose list of source components
            foreach (var comp in expandedSourceComponents)
            {
                var compKey = GetComponentKey(comp);
                var dummyComponent = new SolutionComponent { ComponentType = comp.ComponentType, UniqueName = comp.LogicalName };
                WriteVerbose($"Source component: {ComponentTypeResolver.GetComponentTypeName(Connection, dummyComponent)} ({comp.ComponentType}) - {compKey}");
            }

            // Output verbose list of target components
            foreach (var comp in expandedTargetComponents)
            {
                var compKey = GetComponentKey(comp);
                var dummyComponent = new SolutionComponent { ComponentType = comp.ComponentType, UniqueName = comp.LogicalName };
                WriteVerbose($"Target component: {ComponentTypeResolver.GetComponentTypeName(Connection, dummyComponent)} ({comp.ComponentType}) - {compKey}");
            }

            // Create lookup dictionaries for efficient comparison (compare by logical name first, then object ID)
            var sourceComponentDict = expandedSourceComponents
 .GroupBy(c => new { c.ComponentType, Key = GetComponentKey(c) })
    .ToDictionary(g => g.Key, g => g.First());

            var targetComponentDict = expandedTargetComponents
       .GroupBy(c => new { c.ComponentType, Key = GetComponentKey(c) })
            .ToDictionary(g => g.Key, g => g.First());

            // Find added components (in source but not in target)
            foreach (var sourceComponent in expandedSourceComponents)
            {
                var key = new { sourceComponent.ComponentType, Key = GetComponentKey(sourceComponent) };

                if (!targetComponentDict.ContainsKey(key))
                {
                    OutputComparisonResult(sourceComponent, null, SolutionComponentStatus.InSourceOnly, solutionName, isReversed: false);
                }
                else
                {
                    var targetComponent = targetComponentDict[key];

                    // Check if behavior changed
                    if (sourceComponent.RootComponentBehavior != targetComponent.RootComponentBehavior)
                    {
                        // Determine if this is an inclusion or exclusion based on behavior change
                        SolutionComponentStatus status = DetermineBehaviorChangeStatus(sourceComponent.RootComponentBehavior ?? 0, targetComponent.RootComponentBehavior ?? 0);

                        OutputComparisonResult(sourceComponent, targetComponent, status, solutionName, isReversed: false);
                    }
                    else
                    {
                        // Component exists in both with same behavior - assume modified
                        // (We can't detect actual changes without inspecting the component itself)
                        OutputComparisonResult(sourceComponent, targetComponent, SolutionComponentStatus.InSourceAndTarget, solutionName, isReversed: false);
                    }
                }
            }

            // Find removed components (in target but not in source)
            foreach (var targetComponent in expandedTargetComponents)
            {
                var key = new { targetComponent.ComponentType, Key = GetComponentKey(targetComponent) };

                if (!sourceComponentDict.ContainsKey(key))
                {
                    OutputComparisonResult(null, targetComponent, SolutionComponentStatus.InTargetOnly, solutionName, isReversed: false);
                }
            }
        }

        /// <summary>
        /// Gets a comparison key for a component, preferring LogicalName if available, otherwise ObjectId.
        /// </summary>
        private string GetComponentKey(SolutionComponentInfo component)
        {
            // Prefer logical name (case-insensitive for comparison)
            if (!string.IsNullOrEmpty(component.LogicalName))
            {
                string key = component.LogicalName;
                if (!string.IsNullOrEmpty(component.ParentTableName))
                {
                    key = $"{component.ParentTableName}.{key}";
                }
                return key.ToLowerInvariant();
            }

            // Fall back to ObjectId as string
            return component.ObjectId?.ToString() ?? Guid.Empty.ToString();
        }

        /// <summary>
        /// Gets the display identifier for a component, including parent table name if available.
        /// </summary>
        private string GetDisplayIdentifier(SolutionComponentInfo component)
        {
            if (component == null) return null;

            if (!string.IsNullOrEmpty(component.LogicalName))
            {
                string id = component.LogicalName;
                if (!string.IsNullOrEmpty(component.ParentTableName))
                {
                    id = $"{component.ParentTableName}.{id}";
                }
                return id;
            }

            return component.ObjectId?.ToString();
        }

        private List<SolutionComponentInfo> ExpandComponentsWithSubcomponents(List<SolutionComponent> components, bool isSource, Guid? solutionId)
        {
            var expandedComponents = new List<SolutionComponentInfo>();

            foreach (var component in components)
            {
                // Always add the root component itself
                expandedComponents.Add(new SolutionComponentInfo
                {
                    LogicalName = component.UniqueName,
                    ObjectId = component.ObjectId,
                    MetadataId = component.MetadataId,
                    ComponentType = component.ComponentType,
                    RootComponentBehavior = component.RootComponentBehavior
                });

                var subcomponents = GetSubcomponents(component, isSource, solutionId);
                expandedComponents.AddRange(subcomponents);
            }

            return expandedComponents;
        }

        private List<SolutionComponentInfo> GetSubcomponents(SolutionComponent parentComponent, bool isSource, Guid? solutionId)
        {
            var subcomponents = new List<SolutionComponentInfo>();

            // Use SubcomponentRetriever for retrieval
            SubcomponentRetriever retriever;
            if (isSource)
            {
                retriever = new SubcomponentRetriever(Connection, this, _sourceSolutionBytes, solutionId);
            }
            else
            {
                retriever = new SubcomponentRetriever(Connection, this, solutionId);
            }
            var retrievedSubcomponents = retriever.GetSubcomponents(parentComponent);

            // Convert back to SolutionComponentInfo
            foreach (var subcomponent in retrievedSubcomponents)
            {
                subcomponents.Add(new SolutionComponentInfo
                {
                    LogicalName = subcomponent.UniqueName,
                    ObjectId = subcomponent.ObjectId,
                    MetadataId = subcomponent.MetadataId,
                    ComponentType = subcomponent.ComponentType,
                    RootComponentBehavior = subcomponent.RootComponentBehavior,
                    IsSubcomponent = subcomponent.IsSubcomponent,
                    ParentComponentType = subcomponent.ParentComponentType,
                    ParentTableName = subcomponent.ParentTableName
                });
            }

            return subcomponents;
        }

        private SolutionComponentStatus DetermineBehaviorChangeStatus(int sourceBehavior, int targetBehavior)
        {
            // Behavior levels: 0 (Full/Include Subcomponents) > 1 (Do Not Include Subcomponents) > 2 (Shell)
            // Going from higher number to lower number = including more data (BehaviorIncluded)
            // Going from lower number to higher number = excluding data (BehaviorExcluded)

            if (sourceBehavior < targetBehavior)
            {
                // e.g., 0 (Full) -> 2 (Shell): excluding/removing data
                return SolutionComponentStatus.InSourceAndTarget_BehaviourLessInclusiveInSource;
            }
            else if (sourceBehavior > targetBehavior)
            {
                // e.g., 2 (Shell) -> 0 (Full): including more data
                return SolutionComponentStatus.InSourceAndTarget_BehaviourMoreInclusiveInSource;
            }

            return SolutionComponentStatus.InSourceAndTarget;
        }

        private void OutputComparisonResult(SolutionComponentInfo sourceComponent, SolutionComponentInfo targetComponent,
       SolutionComponentStatus status, string solutionName, bool isReversed)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("SolutionName", solutionName));

            int componentType = sourceComponent?.ComponentType ?? targetComponent?.ComponentType ?? 0;
            var dummyComponent = new SolutionComponent
            {
                ComponentType = componentType,
                UniqueName = sourceComponent?.LogicalName ?? targetComponent?.LogicalName
            };
            result.Properties.Add(new PSNoteProperty("ComponentType", componentType));
            result.Properties.Add(new PSNoteProperty("ComponentTypeName", ComponentTypeResolver.GetComponentTypeName(Connection, dummyComponent)));

            // Display the logical name if available, otherwise the ObjectId
            string displayIdentifier = GetDisplayIdentifier(sourceComponent) ?? GetDisplayIdentifier(targetComponent) ?? "Unknown";
            result.Properties.Add(new PSNoteProperty("DisplayIdentifier", displayIdentifier));

            // Add source and target ObjectIds
            result.Properties.Add(new PSNoteProperty("SourceObjectId", sourceComponent?.ObjectId));
            result.Properties.Add(new PSNoteProperty("TargetObjectId", targetComponent?.ObjectId));

            result.Properties.Add(new PSNoteProperty("Status", status.ToString()));
            result.Properties.Add(new PSNoteProperty("SourceBehavior", sourceComponent?.RootComponentBehavior.HasValue ?? false ? GetBehaviorName(sourceComponent.RootComponentBehavior.Value) : null));
            result.Properties.Add(new PSNoteProperty("TargetBehavior", targetComponent?.RootComponentBehavior.HasValue ?? false ? GetBehaviorName(targetComponent.RootComponentBehavior.Value) : null));
            result.Properties.Add(new PSNoteProperty("IsSubcomponent", sourceComponent?.IsSubcomponent ?? targetComponent?.IsSubcomponent ?? false));

            if ((sourceComponent?.IsSubcomponent ?? targetComponent?.IsSubcomponent ?? false) &&
                (sourceComponent?.ParentComponentType.HasValue ?? targetComponent?.ParentComponentType.HasValue ?? false))
            {
                int? parentComponentType = sourceComponent?.ParentComponentType ?? targetComponent?.ParentComponentType;
                result.Properties.Add(new PSNoteProperty("ParentComponentType", parentComponentType.Value));
                result.Properties.Add(new PSNoteProperty("ParentComponentTypeName", ComponentTypeResolver.GetComponentTypeName(Connection, new SolutionComponent { ComponentType = parentComponentType.Value })));
                result.Properties.Add(new PSNoteProperty("ParentTableName", sourceComponent?.ParentTableName ?? targetComponent?.ParentTableName));
            }

            WriteObject(result);
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

        private class SolutionComponentInfo
        {
            public string LogicalName { get; set; }
            public Guid? ObjectId { get; set; }
            public Guid? MetadataId { get; set; }
            public int ComponentType { get; set; }
            public int? RootComponentBehavior { get; set; }
            public bool IsSubcomponent { get; set; }
            public int? ParentComponentType { get; set; }
            public string ParentTableName { get; set; }
        }

        private enum SolutionComponentStatus
        {
            InSourceOnly,
            InTargetOnly,
            InSourceAndTarget,
            InSourceAndTarget_BehaviourMoreInclusiveInSource,
            InSourceAndTarget_BehaviourLessInclusiveInSource
        }
    }
}
