using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands.Model
{
    /// <summary>
    /// Defines the component types that can be included in a Dataverse app module.
    /// </summary>
    public enum AppModuleComponentType
    {
        /// <summary>
        /// Entity component (table).
        /// </summary>
        Entity = 1,

        /// <summary>
        /// View component (saved query).
        /// </summary>
        View = 26,

        /// <summary>
        /// Business Process Flow component.
        /// </summary>
        BusinessProcessFlow = 29,

        /// <summary>
        /// Ribbon Command component for Forms, Grids, sub grids.
        /// </summary>
        RibbonCommand = 48,

        /// <summary>
        /// Chart component (saved query visualization).
        /// </summary>
        Chart = 59,

        /// <summary>
        /// Form component.
        /// </summary>
        Form = 60,

        /// <summary>
        /// Site Map component.
        /// </summary>
        SiteMap = 62
    }

    /// <summary>
    /// Extension methods for the AppModuleComponentType enum.
    /// </summary>
    public static class AppModuleComponentTypeExtensions
    {
        /// <summary>
        /// Converts an integer value to an AppModuleComponentType enum value.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>The corresponding AppModuleComponentType enum value, or null if invalid.</returns>
        public static AppModuleComponentType? FromInt(int? value)
        {
            if (!value.HasValue)
                return null;

            switch (value.Value)
            {
                case 1:
                    return AppModuleComponentType.Entity;
                case 26:
                    return AppModuleComponentType.View;
                case 29:
                    return AppModuleComponentType.BusinessProcessFlow;
                case 48:
                    return AppModuleComponentType.RibbonCommand;
                case 59:
                    return AppModuleComponentType.Chart;
                case 60:
                    return AppModuleComponentType.Form;
                case 62:
                    return AppModuleComponentType.SiteMap;
                default:
                    return null;
            }
        }
    }
}