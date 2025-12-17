using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Helper class to generate download URLs for icons from different icon sets.
    /// Ensures consistency between Get-DataverseIconSetIcon and Set-DataverseTableIconFromSet cmdlets.
    /// </summary>
    internal static class IconSetUrlHelper
    {
        /// <summary>
        /// Gets the download URL for an icon from a specific icon set.
        /// </summary>
        /// <param name="iconSet">The icon set name (FluentUI, Iconoir, Tabler)</param>
        /// <param name="iconName">The icon name</param>
        /// <returns>The download URL for the icon</returns>
        public static string GetIconDownloadUrl(string iconSet, string iconName)
        {
            switch (iconSet)
            {
                case "Iconoir":
                    // Iconoir icons are at: https://raw.githubusercontent.com/iconoir-icons/iconoir/main/icons/regular/{name}.svg
                    return $"https://raw.githubusercontent.com/iconoir-icons/iconoir/main/icons/regular/{iconName}.svg";

                case "FluentUI":
                    // FluentUI System Icons are at: https://raw.githubusercontent.com/microsoft/fluentui-system-icons/master/assets/{Name}/SVG/ic_fluent_{name}_24_regular.svg
                    // Using master branch, capitalized folder names, and ic_fluent_ prefix
                    // Using 24_regular as the standard size/variant for table icons
                    return $"https://raw.githubusercontent.com/microsoft/fluentui-system-icons/master/assets/{iconName}/SVG/ic_fluent_{iconName.ToLower().Replace(" ", "_")}_24_regular.svg";

                case "Tabler":
                    // Tabler Icons are at: https://raw.githubusercontent.com/tabler/tabler-icons/main/icons/outline/{name}.svg
                    return $"https://raw.githubusercontent.com/tabler/tabler-icons/main/icons/outline/{iconName}.svg";

                default:
                    throw new NotSupportedException($"Icon set '{iconSet}' is not supported");
            }
        }

        /// <summary>
        /// Capitalizes the first letter of a string.
        /// </summary>
        private static string CapitalizeFirst(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        /// <summary>
        /// Gets the GitHub API URL for listing icons in an icon set.
        /// </summary>
        /// <param name="iconSet">The icon set name</param>
        /// <returns>The GitHub API URL for listing icons</returns>
        public static string GetIconListApiUrl(string iconSet)
        {
            switch (iconSet)
            {
                case "Iconoir":
                    return "https://api.github.com/repos/iconoir-icons/iconoir/contents/icons/regular";

                case "FluentUI":
                    // FluentUI uses master branch, not main
                    return "https://api.github.com/repos/microsoft/fluentui-system-icons/contents/assets?ref=master";

                case "Tabler":
                    return "https://api.github.com/repos/tabler/tabler-icons/contents/icons/outline";

                default:
                    throw new NotSupportedException($"Icon set '{iconSet}' is not supported");
            }
        }

        /// <summary>
        /// Gets the raw base URL for constructing download URLs.
        /// </summary>
        /// <param name="iconSet">The icon set name</param>
        /// <returns>The raw base URL</returns>
        public static string GetRawBaseUrl(string iconSet)
        {
            switch (iconSet)
            {
                case "Iconoir":
                    return "https://raw.githubusercontent.com/iconoir-icons/iconoir/main/icons/regular";

                case "FluentUI":
                    // FluentUI uses master branch, not main
                    return "https://raw.githubusercontent.com/microsoft/fluentui-system-icons/master/assets";

                case "Tabler":
                    return "https://raw.githubusercontent.com/tabler/tabler-icons/main/icons/outline";

                default:
                    throw new NotSupportedException($"Icon set '{iconSet}' is not supported");
            }
        }
    }
}
