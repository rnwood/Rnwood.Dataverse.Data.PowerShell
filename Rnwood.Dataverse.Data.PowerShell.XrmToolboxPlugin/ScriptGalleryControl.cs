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
        private PasteBinService _pasteService;
        private List<PasteInfo> _currentPastes;
        private List<PasteInfo> _allPastes;

        public event EventHandler<PasteInfo> OpenPasteRequested;

        public ScriptGalleryControl()
        {
            InitializeComponent();
            _pasteService = new PasteBinService();
            _currentPastes = new List<PasteInfo>();
            _allPastes = new List<PasteInfo>();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                UpdateAuthUI();
                LoadPastesAsync();
            }
        }

        public void SetApiKey(string apiKey)
        {
            _pasteService.SetApiKey(apiKey);
        }

        public PasteBinService GetPasteService()
        {
            return _pasteService;
        }

        private void UpdateAuthUI()
        {
            if (_pasteService.IsAuthenticated)
            {
                signInButton.Text = "Sign Out";
                userLabel.Text = $"Signed in as: {_pasteService.CurrentUser}";
                userLabel.Visible = true;
                managePastesLink.Visible = true;
                myScriptsCheckBox.Enabled = true;
            }
            else
            {
                signInButton.Text = "Sign In";
                userLabel.Text = "";
                userLabel.Visible = false;
                managePastesLink.Visible = false;
                myScriptsCheckBox.Enabled = false;
                myScriptsCheckBox.Checked = false;
            }
        }

        private async void LoadPastesAsync()
        {
            try
            {
                statusLabel.Text = "Loading scripts from PasteBin...";
                statusLabel.ForeColor = SystemColors.ControlText;
                refreshButton.Enabled = false;
                scriptListView.Items.Clear();

                var pastes = await _pasteService.SearchScriptPastesAsync();
                _allPastes = pastes;
                
                // Apply filters
                FilterAndDisplayPastes();
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

        private void FilterAndDisplayPastes()
        {
            var filteredPastes = _allPastes.AsEnumerable();

            // Filter by current user if checkbox is checked
            if (myScriptsCheckBox.Checked && _pasteService.IsAuthenticated)
            {
                var currentUser = _pasteService.CurrentUser;
                filteredPastes = filteredPastes.Where(p => 
                    p.Author?.Equals(currentUser, StringComparison.OrdinalIgnoreCase) == true);
            }

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                var searchText = searchTextBox.Text.ToLower();
                filteredPastes = filteredPastes.Where(p =>
                    (p.Title?.ToLower().Contains(searchText) == true) ||
                    (p.Author?.ToLower().Contains(searchText) == true));
            }

            _currentPastes = filteredPastes.ToList();

            if (_currentPastes.Count == 0)
            {
                statusLabel.Text = "No scripts found. Sign in to see your pastes.";
                statusLabel.ForeColor = Color.Gray;
                scriptListView.Items.Clear();
            }
            else
            {
                PopulatePasteList(_currentPastes);
                statusLabel.Text = $"Found {_currentPastes.Count} script(s)";
                if (_currentPastes.Count != _allPastes.Count)
                {
                    statusLabel.Text += $" (filtered from {_allPastes.Count} total)";
                }
                statusLabel.ForeColor = SystemColors.ControlText;
            }
        }

        private void PopulatePasteList(List<PasteInfo> pastes)
        {
            scriptListView.Items.Clear();

            foreach (var paste in pastes)
            {
                var item = new ListViewItem(new[]
                {
                    paste.GetDisplayTitle(),
                    paste.Author ?? "Unknown",
                    paste.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                });
                item.Tag = paste;
                scriptListView.Items.Add(item);
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadPastesAsync();
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            if (scriptListView.SelectedItems.Count > 0)
            {
                var selectedItem = scriptListView.SelectedItems[0];
                var paste = selectedItem.Tag as PasteInfo;
                if (paste != null)
                {
                    OpenPasteRequested?.Invoke(this, paste);
                }
            }
        }

        private void ScriptListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (scriptListView.SelectedItems.Count > 0)
            {
                var selectedItem = scriptListView.SelectedItems[0];
                var paste = selectedItem.Tag as PasteInfo;
                if (paste != null)
                {
                    UpdateDescriptionPanel(paste);
                    openButton.Enabled = true;
                }
            }
            else
            {
                openButton.Enabled = false;
            }
        }

        private void ScriptListView_DoubleClick(object sender, EventArgs e)
        {
            if (scriptListView.SelectedItems.Count > 0)
            {
                var selectedItem = scriptListView.SelectedItems[0];
                var paste = selectedItem.Tag as PasteInfo;
                if (paste != null)
                {
                    OpenPasteRequested?.Invoke(this, paste);
                }
            }
        }

        private void UpdateDescriptionPanel(PasteInfo paste)
        {
            var description = paste.Title ?? "No description available";
            
            descriptionTextBox.Text = $"Description: {description}\r\n\r\n" +
                                     $"Size: {paste.Size} bytes\r\n" +
                                     $"Author: {paste.Author ?? "Unknown"}\r\n" +
                                     $"Created: {paste.CreatedAt.ToLocalTime():yyyy-MM-dd HH:mm}\r\n" +
                                     $"URL: {paste.Url}";
        }

        private async void SignInButton_Click(object sender, EventArgs e)
        {
            if (_pasteService.IsAuthenticated)
            {
                // Sign out
                var result = MessageBox.Show("Are you sure you want to sign out?", 
                    "Sign Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    _pasteService.SignOut();
                    UpdateAuthUI();
                    LoadPastesAsync();
                }
            }
            else
            {
                // Sign in
                await SignInToPasteBinAsync();
            }
        }

        private async Task SignInToPasteBinAsync()
        {
            using (var dialog = new PasteBinAuthDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var success = await _pasteService.AuthenticateAsync(
                            dialog.ApiKey, 
                            dialog.Username, 
                            dialog.Password);

                        if (success)
                        {
                            UpdateAuthUI();
                            MessageBox.Show($"Successfully signed in as {_pasteService.CurrentUser}!",
                                "Sign In Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadPastesAsync();
                        }
                        else
                        {
                            MessageBox.Show("Sign in failed. Please check your credentials and try again.",
                                "Sign In Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Sign in error: {ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void MyScriptsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            FilterAndDisplayPastes();
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
                FilterAndDisplayPastes();
            };
            _searchTimer.Start();
        }

        private Timer _searchTimer;

        private void ManagePastesLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var url = $"https://pastebin.com/u/{_pasteService.CurrentUser}";
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
