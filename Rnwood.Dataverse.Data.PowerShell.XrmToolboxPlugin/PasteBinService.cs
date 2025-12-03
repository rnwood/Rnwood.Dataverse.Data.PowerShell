using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    /// <summary>
    /// Service for interacting with PasteBin API to manage script gallery
    /// </summary>
    public class PasteBinService
    {
        private const string PASTEBIN_API_BASE = "https://pastebin.com/api";
        private const string SCRIPT_TAG = "#rnwdataversepowershell";
        private readonly HttpClient _httpClient;
        private string _apiKey;
        private string _userKey;

        public PasteBinService()
        {
            _httpClient = new HttpClient();
        }

        public void SetApiKey(string apiKey)
        {
            _apiKey = apiKey;
        }

        public void SetUserKey(string userKey)
        {
            _userKey = userKey;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_userKey);
        public string CurrentUser { get; private set; }

        /// <summary>
        /// Authenticate with username and password to get user key
        /// </summary>
        public async Task<bool> AuthenticateAsync(string apiKey, string username, string password)
        {
            try
            {
                _apiKey = apiKey;
                
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("api_dev_key", apiKey),
                    new KeyValuePair<string, string>("api_user_name", username),
                    new KeyValuePair<string, string>("api_user_password", password)
                });

                var response = await _httpClient.PostAsync($"{PASTEBIN_API_BASE}/api_login.php", content);
                var result = await response.Content.ReadAsStringAsync();

                if (result.StartsWith("Bad API request"))
                {
                    return false;
                }

                _userKey = result;
                CurrentUser = username;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sign out and clear authentication
        /// </summary>
        public void SignOut()
        {
            _userKey = null;
            CurrentUser = null;
        }

        /// <summary>
        /// Search for pastes containing the script tag
        /// Note: PasteBin doesn't provide a search API for free accounts.
        /// We'll need to list user pastes and filter them.
        /// </summary>
        public async Task<List<PasteInfo>> SearchScriptPastesAsync()
        {
            // PasteBin free API doesn't support searching all public pastes
            // We can only list pastes from authenticated user
            if (!IsAuthenticated)
            {
                return new List<PasteInfo>();
            }

            return await GetUserPastesAsync();
        }

        /// <summary>
        /// Get authenticated user's pastes
        /// </summary>
        public async Task<List<PasteInfo>> GetUserPastesAsync()
        {
            if (!IsAuthenticated)
            {
                return new List<PasteInfo>();
            }

            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("api_dev_key", _apiKey),
                    new KeyValuePair<string, string>("api_user_key", _userKey),
                    new KeyValuePair<string, string>("api_results_limit", "100"),
                    new KeyValuePair<string, string>("api_option", "list")
                });

                var response = await _httpClient.PostAsync($"{PASTEBIN_API_BASE}/api_post.php", content);
                var result = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(result) || result.StartsWith("Bad API request") || result == "No pastes found.")
                {
                    return new List<PasteInfo>();
                }

                // Parse XML response
                var pastes = ParsePastesXml(result);
                
                // Filter to only include pastes with the script tag
                return pastes.Where(p => p.Title != null && p.Title.Contains(SCRIPT_TAG)).ToList();
            }
            catch
            {
                return new List<PasteInfo>();
            }
        }

        /// <summary>
        /// Get a specific paste by key
        /// </summary>
        public async Task<PasteInfo> GetPasteAsync(string pasteKey)
        {
            try
            {
                var url = $"https://pastebin.com/raw/{pasteKey}";
                var content = await _httpClient.GetStringAsync(url);

                return new PasteInfo
                {
                    Key = pasteKey,
                    Content = content,
                    Url = $"https://pastebin.com/{pasteKey}"
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Create a new paste
        /// </summary>
        public async Task<PasteInfo> CreatePasteAsync(string title, string content, bool isPublic = true, string apiKey = null)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = _apiKey;
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("API key is required to create pastes");
            }

            // Ensure title contains the tag
            if (!title.Contains(SCRIPT_TAG))
            {
                title = $"{title} {SCRIPT_TAG}";
            }

            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("api_dev_key", apiKey),
                new KeyValuePair<string, string>("api_option", "paste"),
                new KeyValuePair<string, string>("api_paste_code", content),
                new KeyValuePair<string, string>("api_paste_name", title),
                new KeyValuePair<string, string>("api_paste_format", "powershell"),
                new KeyValuePair<string, string>("api_paste_private", isPublic ? "0" : "1") // 0=public, 1=unlisted, 2=private
            };

            if (!string.IsNullOrEmpty(_userKey))
            {
                postData.Add(new KeyValuePair<string, string>("api_user_key", _userKey));
            }

            var postContent = new FormUrlEncodedContent(postData);
            var response = await _httpClient.PostAsync($"{PASTEBIN_API_BASE}/api_post.php", postContent);
            var result = await response.Content.ReadAsStringAsync();

            if (result.StartsWith("Bad API request") || result.StartsWith("Post limit"))
            {
                throw new Exception($"Failed to create paste: {result}");
            }

            // Result is the paste URL
            var pasteKey = result.Replace("https://pastebin.com/", "");
            
            return new PasteInfo
            {
                Key = pasteKey,
                Title = title,
                Content = content,
                Url = result,
                IsPublic = isPublic,
                Author = CurrentUser
            };
        }

        /// <summary>
        /// Delete a paste
        /// </summary>
        public async Task<bool> DeletePasteAsync(string pasteKey)
        {
            if (!IsAuthenticated)
            {
                return false;
            }

            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("api_dev_key", _apiKey),
                    new KeyValuePair<string, string>("api_user_key", _userKey),
                    new KeyValuePair<string, string>("api_paste_key", pasteKey),
                    new KeyValuePair<string, string>("api_option", "delete")
                });

                var response = await _httpClient.PostAsync($"{PASTEBIN_API_BASE}/api_post.php", content);
                var result = await response.Content.ReadAsStringAsync();

                return result.Contains("Paste Removed");
            }
            catch
            {
                return false;
            }
        }

        private List<PasteInfo> ParsePastesXml(string xml)
        {
            var pastes = new List<PasteInfo>();
            
            try
            {
                // Simple XML parsing for PasteBin API response
                var pasteElements = xml.Split(new[] { "<paste>" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var pasteXml in pasteElements.Skip(1)) // Skip first empty element
                {
                    var paste = new PasteInfo();
                    
                    paste.Key = ExtractXmlValue(pasteXml, "paste_key");
                    paste.Title = HttpUtility.HtmlDecode(ExtractXmlValue(pasteXml, "paste_title"));
                    paste.Url = ExtractXmlValue(pasteXml, "paste_url");
                    
                    var sizeStr = ExtractXmlValue(pasteXml, "paste_size");
                    if (int.TryParse(sizeStr, out int size))
                    {
                        paste.Size = size;
                    }
                    
                    var dateStr = ExtractXmlValue(pasteXml, "paste_date");
                    if (long.TryParse(dateStr, out long timestamp))
                    {
                        paste.CreatedAt = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
                    }
                    
                    var privateStr = ExtractXmlValue(pasteXml, "paste_private");
                    paste.IsPublic = privateStr == "0";
                    
                    pastes.Add(paste);
                }
            }
            catch
            {
                // Return empty list on parse error
            }
            
            return pastes;
        }

        private string ExtractXmlValue(string xml, string tagName)
        {
            var startTag = $"<{tagName}>";
            var endTag = $"</{tagName}>";
            
            var startIndex = xml.IndexOf(startTag);
            if (startIndex == -1) return "";
            
            startIndex += startTag.Length;
            var endIndex = xml.IndexOf(endTag, startIndex);
            if (endIndex == -1) return "";
            
            return xml.Substring(startIndex, endIndex - startIndex);
        }
    }

    #region Data Models

    public class PasteInfo
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Size { get; set; }
        public string Author { get; set; }

        public string GetDisplayTitle()
        {
            if (string.IsNullOrEmpty(Title))
            {
                return "Untitled";
            }
            
            // Remove the tag from display
            return Title.Replace("#rnwdataversepowershell", "").Trim();
        }
    }

    #endregion
}
