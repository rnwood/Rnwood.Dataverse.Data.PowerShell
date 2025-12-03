using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    /// <summary>
    /// Service for interacting with GitHub Gists API to manage script gallery
    /// </summary>
    public class GitHubGistService
    {
        private const string GITHUB_API_BASE = "https://api.github.com";
        private const string SCRIPT_TAG = "#rnwdataversepowershell";
        private readonly HttpClient _httpClient;
        private string _accessToken;

        public GitHubGistService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Rnwood-Dataverse-PowerShell-XrmToolbox-Plugin");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        }

        public void SetAccessToken(string token)
        {
            _accessToken = token;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"token {token}");
            }
        }

        /// <summary>
        /// Search for public gists containing the script tag in their description
        /// Note: GitHub API doesn't support searching gists by description.
        /// This method uses the code search API to find PowerShell files containing the tag,
        /// then filters to gists only.
        /// </summary>
        public async Task<List<GistInfo>> SearchScriptGistsAsync()
        {
            try
            {
                // Strategy: Search for the hashtag in PowerShell code files
                // The code search will find files in both repos and gists
                var searchUrl = $"{GITHUB_API_BASE}/search/code?q={Uri.EscapeDataString(SCRIPT_TAG)}+language:PowerShell&per_page=100";
                
                var response = await _httpClient.GetAsync(searchUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    // If search fails, return empty list
                    return new List<GistInfo>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var searchResult = JsonSerializer.Deserialize<GitHubSearchResult>(content);

                // Extract gist IDs from search results
                // GitHub URLs for gists look like: https://gist.github.com/{user}/{gist_id}
                var gistIds = new HashSet<string>();
                
                if (searchResult?.Items != null)
                {
                    foreach (var item in searchResult.Items)
                    {
                        // Check if the HTML URL contains gist.github.com
                        if (item.HtmlUrl != null && item.HtmlUrl.Contains("gist.github.com"))
                        {
                            var gistId = ExtractGistIdFromGistUrl(item.HtmlUrl);
                            if (!string.IsNullOrEmpty(gistId))
                            {
                                gistIds.Add(gistId);
                            }
                        }
                    }
                }

                // Fetch full gist details
                var gists = new List<GistInfo>();
                foreach (var gistId in gistIds.Take(50)) // Limit to prevent too many API calls
                {
                    try
                    {
                        var gist = await GetGistAsync(gistId);
                        if (gist != null)
                        {
                            // Verify the gist contains the tag (in description or content)
                            bool hasTag = (gist.Description != null && gist.Description.Contains(SCRIPT_TAG)) ||
                                         (gist.GetFirstPowerShellContent()?.Contains(SCRIPT_TAG) == true);
                            
                            if (hasTag)
                            {
                                gists.Add(gist);
                            }
                        }
                    }
                    catch
                    {
                        // Skip individual gist errors
                    }
                }

                return gists.OrderByDescending(g => g.UpdatedAt).ToList();
            }
            catch
            {
                return new List<GistInfo>();
            }
        }

        private string ExtractGistIdFromGistUrl(string htmlUrl)
        {
            // Extract gist ID from URLs like: https://gist.github.com/username/1234567890abcdef
            // or https://gist.github.com/1234567890abcdef
            try
            {
                var uri = new Uri(htmlUrl);
                var segments = uri.Segments;
                // The gist ID is typically the last segment
                var lastSegment = segments[segments.Length - 1].TrimEnd('/');
                
                // Gist IDs are typically 32 character hex strings or shorter alphanumeric
                if (lastSegment.Length >= 20 && lastSegment.All(c => char.IsLetterOrDigit(c)))
                {
                    return lastSegment;
                }
            }
            catch
            {
                // Return null on error
            }
            
            return null;
        }


        private string ExtractGistIdFromUrl(string url)
        {
            // Extract gist ID from URLs like: https://api.github.com/gists/1234567890abcdef
            var parts = url?.Split('/');
            return parts?.LastOrDefault();
        }

        /// <summary>
        /// Get a specific gist by ID
        /// </summary>
        public async Task<GistInfo> GetGistAsync(string gistId)
        {
            var response = await _httpClient.GetAsync($"{GITHUB_API_BASE}/gists/{gistId}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GistInfo>(content);
        }

        /// <summary>
        /// Create a new gist
        /// </summary>
        public async Task<GistInfo> CreateGistAsync(string description, string filename, string content, bool isPublic = true)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                throw new InvalidOperationException("GitHub access token is required to create gists");
            }

            var gist = new
            {
                description = description,
                @public = isPublic,
                files = new Dictionary<string, object>
                {
                    [filename] = new { content = content }
                }
            };

            var json = JsonSerializer.Serialize(gist);
            var response = await _httpClient.PostAsync(
                $"{GITHUB_API_BASE}/gists",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GistInfo>(responseContent);
        }

        /// <summary>
        /// Update an existing gist
        /// </summary>
        public async Task<GistInfo> UpdateGistAsync(string gistId, string description, string filename, string content)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                throw new InvalidOperationException("GitHub access token is required to update gists");
            }

            var gist = new
            {
                description = description,
                files = new Dictionary<string, object>
                {
                    [filename] = new { content = content }
                }
            };

            var json = JsonSerializer.Serialize(gist);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{GITHUB_API_BASE}/gists/{gistId}")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GistInfo>(responseContent);
        }

        /// <summary>
        /// Get the authenticated user's gists
        /// </summary>
        public async Task<List<GistInfo>> GetUserGistsAsync()
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                return new List<GistInfo>();
            }

            var response = await _httpClient.GetAsync($"{GITHUB_API_BASE}/gists?per_page=100");
            
            if (!response.IsSuccessStatusCode)
            {
                return new List<GistInfo>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var gists = JsonSerializer.Deserialize<List<GistInfo>>(content);
            
            // Filter to only include gists with the script tag
            return gists?.Where(g => g.Description != null && g.Description.Contains(SCRIPT_TAG))
                        .OrderByDescending(g => g.UpdatedAt)
                        .ToList() ?? new List<GistInfo>();
        }
    }

    #region JSON Models

    public class GistInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        [JsonPropertyName("public")]
        public bool IsPublic { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("files")]
        public Dictionary<string, GistFile> Files { get; set; }

        [JsonPropertyName("owner")]
        public GistOwner Owner { get; set; }

        public string GetFirstPowerShellFile()
        {
            var psFile = Files?.Values.FirstOrDefault(f => 
                f.Filename?.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) == true);
            return psFile?.Filename;
        }

        public string GetFirstPowerShellContent()
        {
            var psFile = Files?.Values.FirstOrDefault(f => 
                f.Filename?.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) == true);
            return psFile?.Content;
        }
    }

    public class GistFile
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class GistOwner
    {
        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }
    }

    public class GitHubSearchResult
    {
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }

        [JsonPropertyName("items")]
        public List<GitHubSearchItem> Items { get; set; }
    }

    public class GitHubSearchItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        [JsonPropertyName("repository")]
        public GitHubRepository Repository { get; set; }
    }

    public class GitHubRepository
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }
    }

    #endregion
}
