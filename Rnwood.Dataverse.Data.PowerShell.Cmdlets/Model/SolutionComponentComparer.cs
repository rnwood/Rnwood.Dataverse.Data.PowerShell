using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
    /// <summary>
    /// Compares solution components between source and target extractors.
    /// </summary>
    public class SolutionComponentComparer
    {
        private readonly IComponentExtractor _sourceExtractor;
        private readonly IComponentExtractor _targetExtractor;
        private readonly PSCmdlet _cmdlet;

        /// <summary>
        /// Initializes a new instance of the SolutionComponentComparer class.
        /// </summary>
        public SolutionComponentComparer(IComponentExtractor sourceExtractor, IComponentExtractor targetExtractor, PSCmdlet cmdlet)
        {
            _sourceExtractor = sourceExtractor ?? throw new ArgumentNullException(nameof(sourceExtractor));
            _targetExtractor = targetExtractor ?? throw new ArgumentNullException(nameof(targetExtractor));
            _cmdlet = cmdlet ?? throw new ArgumentNullException(nameof(cmdlet));

            // Check that both solutions are managed
            if (_sourceExtractor.IsManagedSolution.HasValue && _targetExtractor.IsManagedSolution.HasValue)
            {
                if (!_sourceExtractor.IsManagedSolution.Value || !_targetExtractor.IsManagedSolution.Value)
                {
                    throw new InvalidOperationException("Both source and target solutions must be managed solutions for comparison. Unmanaged solutions cannot be reliably compared.");
                }
            }
        }

        /// <summary>
        /// Compares the components and returns the comparison results.
        /// </summary>
        public List<SolutionComponentComparisonResult> CompareComponents()
        {
            var results = new List<SolutionComponentComparisonResult>();

            // Get components from extractors
            var sourceComponents = _sourceExtractor.GetComponents(true);
            var targetComponents = _targetExtractor.GetComponents(true);

            _cmdlet.WriteVerbose($"Expanded source components: {sourceComponents.Count} total");
            _cmdlet.WriteVerbose($"Expanded target components: {targetComponents.Count} total");

            // Sort components for consistent comparison
            sourceComponents = sourceComponents.OrderBy(c => c.ComponentType).ThenBy(c => GetComponentKey(c)).ToList();
            targetComponents = targetComponents.OrderBy(c => c.ComponentType).ThenBy(c => GetComponentKey(c)).ToList();

            // Create lookup dictionaries for efficient comparison (compare by logical name first, then object ID)
            var sourceComponentDict = sourceComponents
                .GroupBy(c => new { c.ComponentType, Key = GetComponentKey(c) })
                .ToDictionary(g => g.Key, g => g.First());

            var targetComponentDict = targetComponents
                .GroupBy(c => new { c.ComponentType, Key = GetComponentKey(c) })
                .ToDictionary(g => g.Key, g => g.First());

            // Find added components (in source but not in target)
            foreach (var sourceComponent in sourceComponents)
            {
                var key = new { sourceComponent.ComponentType, Key = GetComponentKey(sourceComponent) };

                if (!targetComponentDict.ContainsKey(key))
                {
                    results.Add(new SolutionComponentComparisonResult
                    {
                        SourceComponent = sourceComponent,
                        TargetComponent = null,
                        Status = SolutionComponentStatus.InSourceOnly
                    });
                }
                else
                {
                    var targetComponent = targetComponentDict[key];

                    // Check if behavior changed
                    if (sourceComponent.RootComponentBehavior != targetComponent.RootComponentBehavior)
                    {
                        // Determine if this is an inclusion or exclusion based on behavior change
                        SolutionComponentStatus status = DetermineBehaviorChangeStatus(sourceComponent.RootComponentBehavior ?? 0, targetComponent.RootComponentBehavior ?? 0);

                        results.Add(new SolutionComponentComparisonResult
                        {
                            SourceComponent = sourceComponent,
                            TargetComponent = targetComponent,
                            Status = status
                        });
                    }
                    else
                    {
                        // Component exists in both with same behavior
                        results.Add(new SolutionComponentComparisonResult
                        {
                            SourceComponent = sourceComponent,
                            TargetComponent = targetComponent,
                            Status = SolutionComponentStatus.InSourceAndTarget
                        });
                    }
                }
            }

            // Find removed components (in target but not in source)
            foreach (var targetComponent in targetComponents)
            {
                var key = new { targetComponent.ComponentType, Key = GetComponentKey(targetComponent) };

                if (!sourceComponentDict.ContainsKey(key))
                {
                    results.Add(new SolutionComponentComparisonResult
                    {
                        SourceComponent = null,
                        TargetComponent = targetComponent,
                        Status = SolutionComponentStatus.InTargetOnly
                    });
                }
            }

            return results;
        }

        private SolutionComponentStatus DetermineBehaviorChangeStatus(int sourceBehavior, int targetBehavior)
        {
            // Behavior levels: 0 (Full/Include Subcomponents) > 1 (Do Not Include Subcomponents) > 2 (Shell)
            // Going from higher number to lower number = including more data (BehaviorIncluded)
            // Going from lower number to higher number = excluding data (BehaviorExcluded)

            var sourceBehaviorEnum = (RootComponentBehavior)sourceBehavior;
            var targetBehaviorEnum = (RootComponentBehavior)targetBehavior;

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

        /// <summary>
        /// Gets a comparison key for a component, preferring LogicalName if available, otherwise ObjectId.
        /// </summary>
        private string GetComponentKey(SolutionComponent component)
        {
            // Prefer logical name (case-insensitive for comparison)
            if (!string.IsNullOrEmpty(component.UniqueName))
            {
                return component.UniqueName.ToLowerInvariant();
            }

            // Fall back to ObjectId as string
            return component.ObjectId?.ToString() ?? Guid.Empty.ToString();
        }
    }
}
