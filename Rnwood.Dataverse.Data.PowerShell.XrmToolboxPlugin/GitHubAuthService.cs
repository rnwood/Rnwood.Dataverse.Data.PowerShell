using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Octokit;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    /// <summary>
    /// Service for handling GitHub OAuth authentication
    /// </summary>
    public class GitHubAuthService
    {
        private const string CLIENT_ID = "Ov23liJBvO7HvfUQq7L8"; // GitHub OAuth App for XrmToolbox plugin
        private GitHubClient _client;
        private string _accessToken;
        private User _currentUser;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken) && _currentUser != null;
        public string AccessToken => _accessToken;
        public User CurrentUser => _currentUser;

        public GitHubAuthService()
        {
            _client = new GitHubClient(new ProductHeaderValue("Rnwood-Dataverse-PowerShell-XrmToolbox"));
        }

        /// <summary>
        /// Authenticate using OAuth Device Flow (browser-based)
        /// </summary>
        public async Task<bool> AuthenticateWithDeviceFlowAsync()
        {
            try
            {
                // Request device and user codes
                var deviceFlowRequest = new OauthDeviceFlowRequest(CLIENT_ID)
                {
                    Scopes = { "gist" }
                };

                var deviceFlowResponse = await _client.Oauth.InitiateDeviceFlow(deviceFlowRequest);

                // Show dialog with instructions
                using (var dialog = new GitHubAuthDialog(deviceFlowResponse))
                {
                    var dialogResult = dialog.ShowDialog();
                    
                    if (dialogResult != DialogResult.OK)
                    {
                        return false;
                    }

                    // The dialog handles waiting for user authorization
                    if (!string.IsNullOrEmpty(dialog.AccessToken))
                    {
                        _accessToken = dialog.AccessToken;
                        _client.Credentials = new Credentials(_accessToken);
                        
                        // Get current user info
                        _currentUser = await _client.User.Current();
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Authentication failed: {ex.Message}", 
                    "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Authenticate using Personal Access Token (fallback method)
        /// </summary>
        public async Task<bool> AuthenticateWithTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return false;
                }

                _accessToken = token;
                _client.Credentials = new Credentials(token);
                
                // Verify token by getting current user
                _currentUser = await _client.User.Current();
                return true;
            }
            catch (Exception ex)
            {
                _accessToken = null;
                _currentUser = null;
                MessageBox.Show($"Token validation failed: {ex.Message}\n\nPlease check your token and try again.", 
                    "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Sign out and clear authentication
        /// </summary>
        public void SignOut()
        {
            _accessToken = null;
            _currentUser = null;
            _client.Credentials = Credentials.Anonymous;
        }

        /// <summary>
        /// Get the Octokit client for direct API access
        /// </summary>
        public GitHubClient GetClient()
        {
            return _client;
        }
    }
}
