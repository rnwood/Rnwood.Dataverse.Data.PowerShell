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
        [ValidateSet("FluentUI", "Iconoir", "Tabler")]
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

                WriteVerbose($"Retrieved {icons.Count} total icons from {IconSet}");

                // Check for truncation warning (FluentUI)
                if (icons.Count > 0)
                {
                    var firstIcon = icons[0];
                    var truncatedProperty = firstIcon.Properties["Truncated"];
                    if (truncatedProperty != null && truncatedProperty.Value is bool truncated && truncated)
                    {
                        WriteWarning($"The {IconSet} repository tree was truncated by GitHub API. Retrieved {icons.Count} icons, but some may be missing. The icon set contains more items than can be returned in a single API call.");
                    }
                }

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

                WriteVerbose($"Returning {icons.Count} icons");
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
            else if (IconSet == "Tabler")
            {
                return await GetTablerIconsAsync();
            }

            throw new NotSupportedException($"Icon set '{IconSet}' is not supported");
        }

        private async Task<List<PSObject>> GetIconoirIconsAsync()
        {
            // Iconoir repository: https://github.com/iconoir-icons/iconoir
            // Icons are in: icons/regular/*.svg
            // Use Git Trees API to get all icons (Contents API limited to 1000 items)
            var tree = await GetGitTreeAsync("iconoir-icons", "iconoir", "main");

            var icons = new List<PSObject>();
            foreach (var item in tree.tree.Where(i => 
                i.path.StartsWith("icons/regular/", StringComparison.OrdinalIgnoreCase) && 
                i.path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)))
            {
                var iconName = System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetFileName(item.path));
                var downloadUrl = IconSetUrlHelper.GetIconDownloadUrl("Iconoir", iconName);
                
                var icon = new PSObject();
                icon.Properties.Add(new PSNoteProperty("IconSet", "Iconoir"));
                icon.Properties.Add(new PSNoteProperty("Name", iconName));
                icon.Properties.Add(new PSNoteProperty("FileName", System.IO.Path.GetFileName(item.path)));
                icon.Properties.Add(new PSNoteProperty("DownloadUrl", downloadUrl));
                icon.Properties.Add(new PSNoteProperty("Size", item.size));
                icon.Properties.Add(new PSNoteProperty("Truncated", tree.truncated));
                icons.Add(icon);
            }

            return icons;
        }

        private async Task<List<PSObject>> GetFluentUIIconsAsync()
        {
            // FluentUI System Icons repository: https://github.com/microsoft/fluentui-system-icons
            // Icons are in: assets/{Capitalized}/SVG/ic_fluent_{lowercase}_24_regular.svg
            // Use Git Trees API to get all icons (Contents API limited to 1000 items)
            // Note: FluentUI tree may be truncated due to large size
            var tree = await GetGitTreeAsync("microsoft", "fluentui-system-icons", "master");

            var icons = new List<PSObject>();
            var iconFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // FluentUI has folders for each icon with Capitalized names
            // Each icon typically has multiple sizes (16, 20, 24, 28, 32, 48) and variants (regular, filled)
            // We'll use 24_regular as the default for downloads
            foreach (var item in tree.tree.Where(i => 
                i.path.StartsWith("assets/", StringComparison.OrdinalIgnoreCase) && 
                i.type == "tree"))
            {
                // Extract icon name from path like "assets/IconName"
                var pathParts = item.path.Split('/');
                if (pathParts.Length == 2)
                {
                    var iconName = pathParts[1];
                    if (iconFolders.Add(iconName))
                    {
                        var downloadUrl = IconSetUrlHelper.GetIconDownloadUrl("FluentUI", iconName);
                        
                        var icon = new PSObject();
                        icon.Properties.Add(new PSNoteProperty("IconSet", "FluentUI"));
                        icon.Properties.Add(new PSNoteProperty("Name", iconName));
                        icon.Properties.Add(new PSNoteProperty("FileName", $"ic_fluent_{iconName.ToLower().Replace(" ", "_")}_24_regular.svg"));
                        icon.Properties.Add(new PSNoteProperty("DownloadUrl", downloadUrl));
                        icon.Properties.Add(new PSNoteProperty("Size", 0L)); // Size unknown without fetching each file
                        icon.Properties.Add(new PSNoteProperty("Truncated", tree.truncated));
                        icons.Add(icon);
                    }
                }
            }

            return icons;
        }

        private async Task<List<PSObject>> GetTablerIconsAsync()
        {
            // Tabler Icons repository: https://github.com/tabler/tabler-icons
            // Icons are in: icons/outline/*.svg
            // Use Git Trees API to get all icons (Contents API limited to 1000 items)
            var tree = await GetGitTreeAsync("tabler", "tabler-icons", "main");

            var icons = new List<PSObject>();
            foreach (var item in tree.tree.Where(i => 
                i.path.StartsWith("icons/outline/", StringComparison.OrdinalIgnoreCase) && 
                i.path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)))
            {
                var iconName = System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetFileName(item.path));
                var downloadUrl = IconSetUrlHelper.GetIconDownloadUrl("Tabler", iconName);
                
                var icon = new PSObject();
                icon.Properties.Add(new PSNoteProperty("IconSet", "Tabler"));
                icon.Properties.Add(new PSNoteProperty("Name", iconName));
                icon.Properties.Add(new PSNoteProperty("FileName", System.IO.Path.GetFileName(item.path)));
                icon.Properties.Add(new PSNoteProperty("DownloadUrl", downloadUrl));
                icon.Properties.Add(new PSNoteProperty("Size", item.size));
                icon.Properties.Add(new PSNoteProperty("Truncated", tree.truncated));
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

        private class GitHubBranchInfo
        {
            public GitHubCommitInfo commit { get; set; }
        }

        private class GitHubCommitInfo
        {
            public string sha { get; set; }
        }

        private class GitHubTreeResponse
        {
            public string sha { get; set; }
            public List<GitHubTreeItem> tree { get; set; }
            public bool truncated { get; set; }
        }

        private class GitHubTreeItem
        {
            public string path { get; set; }
            public string mode { get; set; }
            public string type { get; set; }
            public string sha { get; set; }
            public long size { get; set; }
            public string url { get; set; }
        }

        /// <summary>
        /// Fetches the git tree for a repository using the Git Trees API.
        /// This supports repositories with more than 1000 files (Contents API limit).
        /// </summary>
        private async Task<GitHubTreeResponse> GetGitTreeAsync(string owner, string repo, string branch)
        {
            // Step 1: Get the commit SHA for the branch
            var branchUrl = $"https://api.github.com/repos/{owner}/{repo}/branches/{branch}";
            var branchRequest = new HttpRequestMessage(HttpMethod.Get, branchUrl);
            branchRequest.Headers.Add("User-Agent", "Rnwood.Dataverse.Data.PowerShell");
            branchRequest.Headers.Add("Accept", "application/vnd.github.v3+json");

            var branchResponse = await httpClient.SendAsync(branchRequest);
            branchResponse.EnsureSuccessStatusCode();

            var branchJson = await branchResponse.Content.ReadAsStringAsync();
            var branchInfo = JsonSerializer.Deserialize<GitHubBranchInfo>(branchJson);
            var commitSha = branchInfo.commit.sha;

            // Step 2: Get the git tree recursively
            var treeUrl = $"https://api.github.com/repos/{owner}/{repo}/git/trees/{commitSha}?recursive=1";

            var treeRequest = new HttpRequestMessage(HttpMethod.Get, treeUrl);
            treeRequest.Headers.Add("User-Agent", "Rnwood.Dataverse.Data.PowerShell");
            treeRequest.Headers.Add("Accept", "application/vnd.github.v3+json");

            var treeResponse = await httpClient.SendAsync(treeRequest);
            treeResponse.EnsureSuccessStatusCode();

            var treeJson = await treeResponse.Content.ReadAsStringAsync();
            var tree = JsonSerializer.Deserialize<GitHubTreeResponse>(treeJson);

            return tree;
        }
    }
}
