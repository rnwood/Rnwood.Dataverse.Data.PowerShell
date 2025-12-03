using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    /// <summary>
    /// Service for interacting with GitHub Gists API to manage script gallery
    /// </summary>
    public class GitHubGistService
    {
        private const string SCRIPT_TAG = "#rnwdataversepowershell";
        private readonly GitHubClient _client;

        public GitHubGistService()
        {
            _client = new GitHubClient(new ProductHeaderValue("Rnwood-Dataverse-PowerShell-XrmToolbox-Plugin"));
        }

        public void SetAccessToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _client.Credentials = new Credentials(token);
            }
            else
            {
                _client.Credentials = Credentials.Anonymous;
            }
        }

        /// <summary>
        /// Search for public gists containing the script tag
        /// Note: GitHub API doesn't support searching gists by description directly.
        /// This method uses the code search API to find PowerShell files containing the tag.
        /// </summary>
        public async Task<List<GistInfo>> SearchScriptGistsAsync()
        {
            try
            {
                // Search for the hashtag in PowerShell code files
                var searchRequest = new SearchCodeRequest(SCRIPT_TAG)
                {
                    Language = Language.PowerShell,
                    PerPage = 100
                };

                var searchResult = await _client.Search.SearchCode(searchRequest);

                // Extract unique gist IDs from search results
                var gistIds = new HashSet<string>();
                
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

                // Fetch full gist details using Octokit
                var gists = new List<GistInfo>();
                foreach (var gistId in gistIds.Take(50)) // Limit to prevent too many API calls
                {
                    try
                    {
                        var gist = await _client.Gist.Get(gistId);
                        var gistInfo = ConvertToGistInfo(gist);
                        
                        if (gistInfo != null)
                        {
                            // Verify the gist contains the tag
                            bool hasTag = (gistInfo.Description != null && gistInfo.Description.Contains(SCRIPT_TAG)) ||
                                         (gistInfo.GetFirstPowerShellContent()?.Contains(SCRIPT_TAG) == true);
                            
                            if (hasTag)
                            {
                                gists.Add(gistInfo);
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
            try
            {
                var uri = new Uri(htmlUrl);
                var segments = uri.Segments;
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

        /// <summary>
        /// Get a specific gist by ID
        /// </summary>
        public async Task<GistInfo> GetGistAsync(string gistId)
        {
            var gist = await _client.Gist.Get(gistId);
            return ConvertToGistInfo(gist);
        }

        /// <summary>
        /// Create a new gist
        /// </summary>
        public async Task<GistInfo> CreateGistAsync(string description, string filename, string content, bool isPublic = true)
        {
            var newGist = new NewGist
            {
                Description = description,
                Public = isPublic
            };
            newGist.Files.Add(filename, content);

            var gist = await _client.Gist.Create(newGist);
            return ConvertToGistInfo(gist);
        }

        /// <summary>
        /// Update an existing gist
        /// </summary>
        public async Task<GistInfo> UpdateGistAsync(string gistId, string description, string filename, string content)
        {
            var gistUpdate = new GistUpdate
            {
                Description = description
            };
            gistUpdate.Files.Add(filename, new GistFileUpdate { Content = content });

            var gist = await _client.Gist.Edit(gistId, gistUpdate);
            return ConvertToGistInfo(gist);
        }

        /// <summary>
        /// Get the authenticated user's gists
        /// </summary>
        public async Task<List<GistInfo>> GetUserGistsAsync()
        {
            try
            {
                var gists = await _client.Gist.GetAll();
                
                // Filter to only include gists with the script tag
                return gists
                    .Select(ConvertToGistInfo)
                    .Where(g => g.Description != null && g.Description.Contains(SCRIPT_TAG))
                    .OrderByDescending(g => g.UpdatedAt)
                    .ToList();
            }
            catch
            {
                return new List<GistInfo>();
            }
        }

        private GistInfo ConvertToGistInfo(Gist gist)
        {
            if (gist == null) return null;

            var gistInfo = new GistInfo
            {
                Id = gist.Id,
                Description = gist.Description,
                HtmlUrl = gist.HtmlUrl,
                IsPublic = gist.Public,
                CreatedAt = gist.CreatedAt.DateTime,
                UpdatedAt = gist.UpdatedAt.DateTime,
                Files = new Dictionary<string, GistFile>(),
                Owner = gist.Owner != null ? new GistOwner
                {
                    Login = gist.Owner.Login,
                    AvatarUrl = gist.Owner.AvatarUrl
                } : null
            };

            foreach (var file in gist.Files)
            {
                gistInfo.Files[file.Key] = new GistFile
                {
                    Filename = file.Value.Filename,
                    Type = file.Value.Type,
                    Language = file.Value.Language,
                    Size = file.Value.Size,
                    Content = file.Value.Content
                };
            }

            return gistInfo;
        }
    }

    #region JSON Models (kept for compatibility)

    public class GistInfo
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string HtmlUrl { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Dictionary<string, GistFile> Files { get; set; }
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
        public string Filename { get; set; }
        public string Type { get; set; }
        public string Language { get; set; }
        public int Size { get; set; }
        public string Content { get; set; }
    }

    public class GistOwner
    {
        public string Login { get; set; }
        public string AvatarUrl { get; set; }
    }

    #endregion
}
