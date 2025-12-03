using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class ScriptGalleryControl : UserControl
    {
        private GitHubGistService _gistService;
        private List<GistInfo> _currentGists;

        public event EventHandler<GistInfo> OpenGistRequested;

        public ScriptGalleryControl()
        {
            InitializeComponent();
            _gistService = new GitHubGistService();
            _currentGists = new List<GistInfo>();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                LoadGistsAsync();
            }
        }

        public void SetGitHubToken(string token)
        {
            _gistService.SetAccessToken(token);
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
                _currentGists = gists;

                if (gists.Count == 0)
                {
                    statusLabel.Text = "No scripts found. Scripts must contain #rnwdataversepowershell in the description.";
                    statusLabel.ForeColor = Color.Gray;
                }
                else
                {
                    PopulateGistList(gists);
                    statusLabel.Text = $"Found {gists.Count} script(s)";
                    statusLabel.ForeColor = SystemColors.ControlText;
                }
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
    }
}