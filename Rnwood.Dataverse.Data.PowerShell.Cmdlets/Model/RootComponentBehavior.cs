using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
    /// <summary>
    /// Defines the behavior values for how a root component is included in a solution.
    /// </summary>
    public enum RootComponentBehavior
    {
        /// <summary>
        /// Include the component with all its subcomponents.
        /// </summary>
        IncludeSubcomponents = 0,

        /// <summary>
        /// Include the component but do not include its subcomponents.
        /// </summary>
        DoNotIncludeSubcomponents = 1,

        /// <summary>
        /// Include the component as a shell only (minimal information).
        /// </summary>
        IncludeAsShell = 2
    }

    /// <summary>
    /// Extension methods for the RootComponentBehavior enum.
    /// </summary>
    public static class RootComponentBehaviorExtensions
    {
        /// <summary>
        /// Gets the display name for a RootComponentBehavior value.
        /// </summary>
        /// <param name="behavior">The behavior value.</param>
        /// <returns>The display name for the behavior.</returns>
        public static string GetDisplayName(this RootComponentBehavior behavior)
        {
            switch (behavior)
            {
                case RootComponentBehavior.IncludeSubcomponents:
                    return "Include Subcomponents";
                case RootComponentBehavior.DoNotIncludeSubcomponents:
                    return "Do Not Include Subcomponents";
                case RootComponentBehavior.IncludeAsShell:
                    return "Include As Shell";
                default:
                    return $"Unknown ({(int)behavior})";
            }
        }

        /// <summary>
        /// Converts an integer value to a RootComponentBehavior enum value.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>The corresponding RootComponentBehavior enum value, or null if invalid.</returns>
        public static RootComponentBehavior? FromInt(int? value)
        {
            if (!value.HasValue)
                return null;

            switch (value.Value)
            {
                case 0:
                    return RootComponentBehavior.IncludeSubcomponents;
                case 1:
                    return RootComponentBehavior.DoNotIncludeSubcomponents;
                case 2:
                    return RootComponentBehavior.IncludeAsShell;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the display name for an integer behavior value.
        /// </summary>
        /// <param name="behavior">The integer behavior value.</param>
        /// <returns>The display name for the behavior.</returns>
        public static string GetDisplayName(int? behavior)
        {
            var enumValue = FromInt(behavior);
            return enumValue?.GetDisplayName() ?? $"Unknown ({behavior ?? 0})";
        }
    }
}