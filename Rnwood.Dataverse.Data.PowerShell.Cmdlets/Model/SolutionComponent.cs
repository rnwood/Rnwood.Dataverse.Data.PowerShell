using System;
using System.Collections.Generic;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
  /// <summary>
  /// Represents a solution component for comparison or analysis.
    /// </summary>
    public class SolutionComponent
    {
        /// <summary>
 /// Gets or sets the component's object ID (GUID for most component types).
        /// </summary>
        public Guid? ObjectId { get; set; }

      /// <summary>
        /// Gets or sets the component's logical name (for entities and attributes).
        /// </summary>
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the component type.
   /// </summary>
        public int ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the display name of the component type.
        /// </summary>
        public string ComponentTypeName { get; set; }

        /// <summary>
        /// Gets or sets the root component behavior.
        /// </summary>
        public int? RootComponentBehavior { get; set; }

        /// <summary>
      /// Gets or sets whether this is a subcomponent.
        /// </summary>
        public bool IsSubcomponent { get; set; }

     /// <summary>
   /// Gets or sets the parent component type (for subcomponents).
        /// </summary>
        public int? ParentComponentType { get; set; }

        /// <summary>
        /// Gets or sets the parent component's table name (for subcomponents).
   /// </summary>
        public string ParentTableName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a default component.
        /// </summary>
        public bool? IsDefault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a custom component.
        /// </summary>
        public bool? IsCustom { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this component is customized.
        /// </summary>
        public bool? IsCustomized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parent is a custom component.
        /// </summary>
        public bool? ParentIsCustom { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parent component is customized.
        /// </summary>
        public bool? ParentIsCustomized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parent is a default component.
        /// </summary>
        public bool? ParentIsDefault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a managed component.
        /// </summary>
        public bool? IsManaged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parent is a managed component.
        /// </summary>
        public bool? ParentIsManaged { get; set; }
    }
}
