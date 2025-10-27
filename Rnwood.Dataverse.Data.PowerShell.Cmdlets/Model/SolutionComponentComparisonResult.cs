using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
 /// <summary>
  /// Represents the result of comparing two solution components.
    /// </summary>
 public class SolutionComponentComparisonResult
    {
        /// <summary>
   /// Gets or sets the source component.
     /// </summary>
        public SolutionComponent SourceComponent { get; set; }

     /// <summary>
        /// Gets or sets the target component.
        /// </summary>
   public SolutionComponent TargetComponent { get; set; }

     /// <summary>
    /// Gets or sets the comparison status.
        /// </summary>
public SolutionComponentStatus Status { get; set; }
    }

   /// <summary>
  /// Status of a solution component comparison.
    /// </summary>
    public enum SolutionComponentStatus
    {
        /// <summary>
      /// Component exists only in the source.
 /// </summary>
        InSourceOnly,

    /// <summary>
       /// Component exists only in the target.
      /// </summary>
 InTargetOnly,

   /// <summary>
  /// Component exists in both source and target with the same behavior.
       /// </summary>
    InSourceAndTarget,

 /// <summary>
        /// Component exists in both but source behavior is more inclusive.
        /// </summary>
     InSourceAndTarget_BehaviourMoreInclusiveInSource,

     /// <summary>
     /// Component exists in both but source behavior is less inclusive.
  /// </summary>
        InSourceAndTarget_BehaviourLessInclusiveInSource
  }
}
