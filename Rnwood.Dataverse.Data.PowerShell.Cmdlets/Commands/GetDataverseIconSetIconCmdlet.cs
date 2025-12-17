using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves available icons from supported online icon sets.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseIconSetIcon")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseIconSetIconCmdlet : PSCmdlet
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Gets or sets the icon set to retrieve icons from.
        /// </summary>
        [Parameter(Position = 0, HelpMessage = "Icon set to retrieve icons from")]
        [ValidateSet("FluentUI", "Iconoir")]
        public string IconSet { get; set; } = "FluentUI";

        /// <summary>
        /// Gets or sets the filter pattern to match icon names (supports wildcards).
        /// </summary>
        [Parameter(Position = 1, HelpMessage = "Filter pattern to match icon names (supports wildcards like 'user*' or '*settings')")]
        [SupportsWildcards]
        public string Name { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose($"Retrieving icons from {IconSet} icon set");

            try
            {
                var icons = GetIconsAsync().GetAwaiter().GetResult();

                // Filter icons by name if specified
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    var pattern = new WildcardPattern(Name, WildcardOptions.IgnoreCase);
                    icons = icons.Where(icon => 
                    {
                        var nameProperty = icon.Properties["Name"];
                        if (nameProperty?.Value != null)
                        {
                            return pattern.IsMatch(nameProperty.Value.ToString());
                        }
                        return false;
                    }).ToList();
                    WriteVerbose($"Filtered to {icons.Count} icons matching '{Name}'");
                }

                WriteVerbose($"Found {icons.Count} icons");
                WriteObject(icons, true);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "IconRetrievalError",
                    ErrorCategory.InvalidOperation,
                    IconSet));
            }
        }

        private async Task<List<PSObject>> GetIconsAsync()
        {
            if (IconSet == "Iconoir")
            {
                return await GetIconoirIconsAsync();
            }
            else if (IconSet == "FluentUI")
            {
                return await GetFluentUIIconsAsync();
            }

            throw new NotSupportedException($"Icon set '{IconSet}' is not supported");
        }

        private async Task<List<PSObject>> GetIconoirIconsAsync()
        {
            // Iconoir repository: https://github.com/iconoir-icons/iconoir
            // Icons are in: icons/regular/*.svg
            // We'll use the GitHub API to list the directory contents
            const string apiUrl = "https://api.github.com/repos/iconoir-icons/iconoir/contents/icons/regular";
            const string rawBaseUrl = "https://raw.githubusercontent.com/iconoir-icons/iconoir/main/icons/regular";

            WriteVerbose($"Fetching icon list from GitHub API: {apiUrl}");

            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Add("User-Agent", "Rnwood.Dataverse.Data.PowerShell");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<List<GitHubFileItem>>(jsonContent);

            var icons = new List<PSObject>();
            foreach (var item in items.Where(i => i.name.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)))
            {
                var iconName = System.IO.Path.GetFileNameWithoutExtension(item.name);
                var icon = new PSObject();
                icon.Properties.Add(new PSNoteProperty("IconSet", "Iconoir"));
                icon.Properties.Add(new PSNoteProperty("Name", iconName));
                icon.Properties.Add(new PSNoteProperty("FileName", item.name));
                icon.Properties.Add(new PSNoteProperty("DownloadUrl", item.download_url ?? $"{rawBaseUrl}/{item.name}"));
                icon.Properties.Add(new PSNoteProperty("Size", item.size));
                icons.Add(icon);
            }

            return icons;
        }

        private async Task<List<PSObject>> GetFluentUIIconsAsync()
        {
            // FluentUI System Icons repository: https://github.com/microsoft/fluentui-system-icons
            // Icons are in: assets/{iconname}/SVG/{iconname}_{size}_{variant}.svg
            // We'll query the assets folder and look for regular/filled variants
            const string apiUrl = "https://api.github.com/repos/microsoft/fluentui-system-icons/contents/assets";
            const string rawBaseUrl = "https://raw.githubusercontent.com/microsoft/fluentui-system-icons/main/assets";

            WriteVerbose($"Fetching icon list from GitHub API: {apiUrl}");

            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Add("User-Agent", "Rnwood.Dataverse.Data.PowerShell");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<List<GitHubFileItem>>(jsonContent);

            var icons = new List<PSObject>();
            // FluentUI has folders for each icon, we'll list the folder names as icon names
            // Each icon typically has multiple sizes (16, 20, 24, 28, 32, 48) and variants (regular, filled)
            // We'll use 24_regular as the default for downloads
            foreach (var item in items.Where(i => i.type == "dir"))
            {
                var iconName = item.name;
                var icon = new PSObject();
                icon.Properties.Add(new PSNoteProperty("IconSet", "FluentUI"));
                icon.Properties.Add(new PSNoteProperty("Name", iconName));
                icon.Properties.Add(new PSNoteProperty("FileName", $"{iconName}_24_regular.svg"));
                icon.Properties.Add(new PSNoteProperty("DownloadUrl", $"{rawBaseUrl}/{iconName}/SVG/{iconName}_24_regular.svg"));
                icon.Properties.Add(new PSNoteProperty("Size", 0L)); // Size unknown without fetching each file
                icons.Add(icon);
            }

            return icons;
        }

        private class GitHubFileItem
        {
            public string name { get; set; }
            public string path { get; set; }
            public string download_url { get; set; }
            public long size { get; set; }
            public string type { get; set; }
        }
    }
}
