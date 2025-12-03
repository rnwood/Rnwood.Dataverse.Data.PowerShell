using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Octokit;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class GitHubAuthDialog : Form
    {
        private readonly OauthDeviceFlowResponse _deviceFlowResponse;
        private CancellationTokenSource _cancellationTokenSource;
        private GitHubClient _client;
        
        public string AccessToken { get; private set; }

        public GitHubAuthDialog(OauthDeviceFlowResponse deviceFlowResponse)
        {
            InitializeComponent();
            _deviceFlowResponse = deviceFlowResponse;
            _cancellationTokenSource = new CancellationTokenSource();
            _client = new GitHubClient(new ProductHeaderValue("Rnwood-Dataverse-PowerShell-XrmToolbox"));

            // Display the user code and URL
            userCodeLabel.Text = deviceFlowResponse.UserCode;
            instructionsLabel.Text = $"1. Click 'Open Browser' or visit:\n   {deviceFlowResponse.VerificationUri}\n\n" +
                                   $"2. Enter code: {deviceFlowResponse.UserCode}\n\n" +
                                   "3. Click 'Authorize' on GitHub\n\n" +
                                   "This dialog will close automatically once authorized.";
        }

        private void OpenBrowserButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _deviceFlowResponse.VerificationUri.ToString(),
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open browser: {ex.Message}\n\nPlease visit the URL manually.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CopyCodeButton_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(_deviceFlowResponse.UserCode);
                MessageBox.Show("Code copied to clipboard!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy code: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            // Start polling for authorization
            await PollForAuthorizationAsync();
        }

        private async Task PollForAuthorizationAsync()
        {
            try
            {
                statusLabel.Text = "Waiting for authorization...";

                // Poll every interval seconds until authorized or timeout
                var timeout = DateTime.Now.AddSeconds(_deviceFlowResponse.ExpiresIn);
                
                while (DateTime.Now < timeout && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var token = await _client.Oauth.CreateAccessTokenForDeviceFlow(
                            "Ov23liJBvO7HvfUQq7L8", // CLIENT_ID
                            _deviceFlowResponse);

                        if (!string.IsNullOrEmpty(token.AccessToken))
                        {
                            AccessToken = token.AccessToken;
                            statusLabel.Text = "Authorization successful!";
                            DialogResult = DialogResult.OK;
                            Close();
                            return;
                        }
                    }
                    catch (AuthorizationException)
                    {
                        // Authorization pending - continue polling
                    }
                    catch (Exception ex)
                    {
                        statusLabel.Text = $"Error: {ex.Message}";
                        break;
                    }

                    await Task.Delay(_deviceFlowResponse.Interval * 1000, _cancellationTokenSource.Token);
                }

                if (DateTime.Now >= timeout)
                {
                    statusLabel.Text = "Authorization timed out. Please try again.";
                    MessageBox.Show("Authorization timed out. Please try again.",
                        "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error: {ex.Message}";
                MessageBox.Show($"Authorization failed: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelAuthButton_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            base.OnFormClosing(e);
        }
    }
}
