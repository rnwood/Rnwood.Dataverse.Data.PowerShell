using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class ScriptGalleryControl : UserControl
    {
        private GitHubGistService _gistService;
        private GitHubAuthService _authService;
        private List<GistInfo> _currentGists;
        private List<GistInfo> _allGists;

        public event EventHandler<GistInfo> OpenGistRequested;

        public ScriptGalleryControl()
        {
            InitializeComponent();
            _gistService = new GitHubGistService();
            _authService = new GitHubAuthService();
            _currentGists = new List<GistInfo>();
            _allGists = new List<GistInfo>();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                UpdateAuthUI();
                LoadGistsAsync();
            }
        }

        public void SetGitHubToken(string token)
        {
            _gistService.SetAccessToken(token);
        }

        public GitHubAuthService GetAuthService()
        {
            return _authService;
        }

        private void UpdateAuthUI()
        {
            if (_authService.IsAuthenticated)
            {
                signInButton.Text = "Sign Out";
                userLabel.Text = $"Signed in as: {_authService.CurrentUser.Login}";
                userLabel.Visible = true;
                manageGistsLink.Visible = true;
                myScriptsCheckBox.Enabled = true;
            }
            else
            {
                signInButton.Text = "Sign In";
                userLabel.Text = "";
                userLabel.Visible = false;
                manageGistsLink.Visible = false;
                myScriptsCheckBox.Enabled = false;
                myScriptsCheckBox.Checked = false;
            }
        }

        private async void LoadGistsAsync()
        {
            try
            {
                statusLabel.Text = "Loading scripts from GitHub Gists...";
                statusLabel.ForeColor = SystemColors.ControlText;
                refreshButton.Enabled = false;
                scriptListView.Items.Clear();

                var gists = await _gistService.SearchScriptGistsAsync();
                _allGists = gists;
                
                // Apply filters
                FilterAndDisplayGists();
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error loading scripts: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
            }
            finally
            {
                refreshButton.Enabled = true;
            }
        }

        private void FilterAndDisplayGists()
        {
            var filteredGists = _allGists.AsEnumerable();

            // Filter by current user if checkbox is checked
            if (myScriptsCheckBox.Checked && _authService.IsAuthenticated)
            {
                var currentUserLogin = _authService.CurrentUser.Login;
                filteredGists = filteredGists.Where(g => 
                    g.Owner?.Login?.Equals(currentUserLogin, StringComparison.OrdinalIgnoreCase) == true);
            }

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                var searchText = searchTextBox.Text.ToLower();
                filteredGists = filteredGists.Where(g =>
                    (g.Description?.ToLower().Contains(searchText) == true) ||
                    (g.Owner?.Login?.ToLower().Contains(searchText) == true) ||
                    (g.GetFirstPowerShellFile()?.ToLower().Contains(searchText) == true));
            }

            _currentGists = filteredGists.ToList();

            if (_currentGists.Count == 0)
            {
                statusLabel.Text = "No scripts found. Try adjusting your filters or search terms.";
                statusLabel.ForeColor = Color.Gray;
                scriptListView.Items.Clear();
            }
            else
            {
                PopulateGistList(_currentGists);
                statusLabel.Text = $"Found {_currentGists.Count} script(s)";
                if (_currentGists.Count != _allGists.Count)
                {
                    statusLabel.Text += $" (filtered from {_allGists.Count} total)";
                }
                statusLabel.ForeColor = SystemColors.ControlText;
            }
        }

        private void PopulateGistList(List<GistInfo> gists)
        {
            scriptListView.Items.Clear();

            foreach (var gist in gists)
            {
                var item = new ListViewItem(new[]
                {
                    GetGistTitle(gist),
                    gist.Owner?.Login ?? "Unknown",
                    gist.UpdatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                });
                item.Tag = gist;
                scriptListView.Items.Add(item);
            }

            // Auto-resize columns
            if (scriptListView.Items.Count > 0)
            {
                scriptListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                scriptListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
        }

        private string GetGistTitle(GistInfo gist)
        {
            if (!string.IsNullOrEmpty(gist.Description))
            {
                // Remove the hashtag from display
                var title = gist.Description.Replace("#rnwdataversepowershell", "").Trim();
                if (!string.IsNullOrWhiteSpace(title))
                {
                    return title;
                }
            }

            // Fallback to first PowerShell file name
            var fileName = gist.GetFirstPowerShellFile();
            return !string.IsNullOrEmpty(fileName) ? fileName : "Untitled Script";
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadGistsAsync();
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            OpenSelectedGist();
        }

        private void ScriptListView_DoubleClick(object sender, EventArgs e)
        {
            OpenSelectedGist();
        }

        private void OpenSelectedGist()
        {
            if (scriptListView.SelectedItems.Count > 0)
            {
                var gist = scriptListView.SelectedItems[0].Tag as GistInfo;
                if (gist != null)
                {
                    OpenGistRequested?.Invoke(this, gist);
                }
            }
        }

        private void ScriptListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            openButton.Enabled = scriptListView.SelectedItems.Count > 0;
            
            if (scriptListView.SelectedItems.Count > 0)
            {
                var gist = scriptListView.SelectedItems[0].Tag as GistInfo;
                if (gist != null)
                {
                    UpdateDescriptionPanel(gist);
                }
            }
            else
            {
                descriptionTextBox.Clear();
            }
        }

        private void UpdateDescriptionPanel(GistInfo gist)
        {
            var description = gist.Description ?? "No description available";
            var fileName = gist.GetFirstPowerShellFile() ?? "N/A";
            var fileSize = gist.Files?.Values.FirstOrDefault()?.Size ?? 0;
            
            descriptionTextBox.Text = $"Description: {description}\r\n\r\n" +
                                     $"File: {fileName}\r\n" +
                                     $"Size: {fileSize} bytes\r\n" +
                                     $"Owner: {gist.Owner?.Login ?? "Unknown"}\r\n" +
                                     $"Created: {gist.CreatedAt.ToLocalTime():yyyy-MM-dd HH:mm}\r\n" +
                                     $"Updated: {gist.UpdatedAt.ToLocalTime():yyyy-MM-dd HH:mm}\r\n" +
                                     $"URL: {gist.HtmlUrl}";
        }

        private async void SignInButton_Click(object sender, EventArgs e)
        {
            if (_authService.IsAuthenticated)
            {
                // Sign out
                var result = MessageBox.Show("Are you sure you want to sign out?", 
                    "Sign Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    _authService.SignOut();
                    _gistService.SetAccessToken(null);
                    UpdateAuthUI();
                }
            }
            else
            {
                // Sign in - show options
                await ShowSignInOptionsAsync();
            }
        }

        private async Task ShowSignInOptionsAsync()
        {
            using (var optionsDialog = new Form())
            {
                optionsDialog.Text = "Sign In to GitHub";
                optionsDialog.Size = new System.Drawing.Size(400, 200);
                optionsDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                optionsDialog.StartPosition = FormStartPosition.CenterParent;
                optionsDialog.MaximizeBox = false;
                optionsDialog.MinimizeBox = false;

                var label = new Label
                {
                    Text = "Choose your sign-in method:",
                    Location = new System.Drawing.Point(20, 20),
                    Size = new System.Drawing.Size(350, 20)
                };

                var browserButton = new Button
                {
                    Text = "Sign in with Browser (Recommended)",
                    Location = new System.Drawing.Point(20, 50),
                    Size = new System.Drawing.Size(340, 35)
                };
                browserButton.Click += async (s, e) =>
                {
                    optionsDialog.DialogResult = DialogResult.OK;
                    optionsDialog.Tag = "browser";
                    optionsDialog.Close();
                };

                var tokenButton = new Button
                {
                    Text = "Sign in with Personal Access Token",
                    Location = new System.Drawing.Point(20, 90),
                    Size = new System.Drawing.Size(340, 35)
                };
                tokenButton.Click += (s, e) =>
                {
                    optionsDialog.DialogResult = DialogResult.OK;
                    optionsDialog.Tag = "token";
                    optionsDialog.Close();
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    Location = new System.Drawing.Point(250, 130),
                    Size = new System.Drawing.Size(110, 30),
                    DialogResult = DialogResult.Cancel
                };

                optionsDialog.Controls.AddRange(new Control[] { label, browserButton, tokenButton, cancelButton });
                optionsDialog.CancelButton = cancelButton;

                var result = optionsDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var method = optionsDialog.Tag as string;
                    if (method == "browser")
                    {
                        await SignInWithBrowserAsync();
                    }
                    else if (method == "token")
                    {
                        await SignInWithTokenAsync();
                    }
                }
            }
        }

        private async Task SignInWithBrowserAsync()
        {
            try
            {
                if (await _authService.AuthenticateWithDeviceFlowAsync())
                {
                    _gistService.SetAccessToken(_authService.AccessToken);
                    UpdateAuthUI();
                    MessageBox.Show($"Successfully signed in as {_authService.CurrentUser.Login}!",
                        "Sign In Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sign in failed: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SignInWithTokenAsync()
        {
            using (var tokenDialog = new Form())
            {
                tokenDialog.Text = "Sign in with Personal Access Token";
                tokenDialog.Size = new System.Drawing.Size(500, 250);
                tokenDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                tokenDialog.StartPosition = FormStartPosition.CenterParent;
                tokenDialog.MaximizeBox = false;
                tokenDialog.MinimizeBox = false;

                var label = new Label
                {
                    Text = "Enter your GitHub Personal Access Token:",
                    Location = new System.Drawing.Point(20, 20),
                    Size = new System.Drawing.Size(450, 20)
                };

                var tokenTextBox = new TextBox
                {
                    Location = new System.Drawing.Point(20, 50),
                    Size = new System.Drawing.Size(440, 20),
                    UseSystemPasswordChar = true
                };

                var linkLabel = new LinkLabel
                {
                    Text = "Create token at: https://github.com/settings/tokens (scope: gist)",
                    Location = new System.Drawing.Point(20, 80),
                    Size = new System.Drawing.Size(450, 20)
                };
                linkLabel.LinkClicked += (s, e) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "https://github.com/settings/tokens",
                            UseShellExecute = true
                        });
                    }
                    catch { }
                };

                var infoLabel = new Label
                {
                    Text = "The token will be validated when you click OK.",
                    Location = new System.Drawing.Point(20, 110),
                    Size = new System.Drawing.Size(450, 40),
                    ForeColor = Color.Gray
                };

                var okButton = new Button
                {
                    Text = "OK",
                    Location = new System.Drawing.Point(250, 160),
                    Size = new System.Drawing.Size(100, 30),
                    DialogResult = DialogResult.OK
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    Location = new System.Drawing.Point(360, 160),
                    Size = new System.Drawing.Size(100, 30),
                    DialogResult = DialogResult.Cancel
                };

                tokenDialog.Controls.AddRange(new Control[] { label, tokenTextBox, linkLabel, infoLabel, okButton, cancelButton });
                tokenDialog.AcceptButton = okButton;
                tokenDialog.CancelButton = cancelButton;

                if (tokenDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(tokenTextBox.Text))
                {
                    if (await _authService.AuthenticateWithTokenAsync(tokenTextBox.Text))
                    {
                        _gistService.SetAccessToken(_authService.AccessToken);
                        UpdateAuthUI();
                        MessageBox.Show($"Successfully signed in as {_authService.CurrentUser.Login}!",
                            "Sign In Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void MyScriptsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            FilterAndDisplayGists();
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            // Debounce search - wait 500ms after user stops typing
            if (_searchTimer != null)
            {
                _searchTimer.Stop();
                _searchTimer.Dispose();
            }

            _searchTimer = new Timer();
            _searchTimer.Interval = 500;
            _searchTimer.Tick += (s, args) =>
            {
                _searchTimer.Stop();
                FilterAndDisplayGists();
            };
            _searchTimer.Start();
        }

        private Timer _searchTimer;

        private void ManageGistsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var url = $"https://gist.github.com/{_authService.CurrentUser.Login}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open browser: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}