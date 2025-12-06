using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class ScriptGalleryControl : UserControl
    {
        private GitHubService _githubService;
        private List<ScriptGalleryItem> _discussions;
        private ScriptGalleryItem _selectedItem;
        private bool _webViewInitialized;

        public event EventHandler<string> LoadScriptRequested;

        public ScriptGalleryControl()
        {
            InitializeComponent();
            _githubService = new GitHubService();
            
            // Initialize WebView2
            InitializeWebViewAsync();
        }
        
        protected override void OnHandleDestroyed(EventArgs e)
        {
            _githubService?.Dispose();
            base.OnHandleDestroyed(e);
        }

        private async void InitializeWebViewAsync()
        {
            try
            {
                await detailWebView.EnsureCoreWebView2Async(null);
                _webViewInitialized = true;
                detailWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                detailWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize WebView2: {ex.Message}\n\nWebView2 Runtime may not be installed.\n\nPlease download and install it from:\nhttps://developer.microsoft.com/microsoft-edge/webview2/",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void SetScriptEditorControl(ScriptEditorControl editorControl)
        {
            // Will be used to enable save to gallery from editor
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            if (_githubService.IsAuthenticated)
            {
                // Logout
                _githubService.Logout();
                loginButton.Text = "Login to GitHub";
                statusLabel.Text = "Not logged in";
                MessageBox.Show("Logged out successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Show token input dialog
                using (var tokenForm = new Form
                {
                    Text = "GitHub Authentication",
                    Width = 500,
                    Height = 250,
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                })
                {
                    var instructionLabel = new Label
                    {
                        Text = "Please enter your GitHub Personal Access Token.\n\nTo create one:\n1. Go to github.com/settings/tokens\n2. Click 'Generate new token' (classic)\n3. Select scopes:\n   - 'public_repo' (for public repositories) OR 'repo' (for private repositories)\n   - 'read:discussion' and 'write:discussion'\n4. Generate and copy the token",
                        AutoSize = false,
                        Location = new System.Drawing.Point(20, 20),
                        Size = new System.Drawing.Size(440, 100)
                    };

                    var tokenLabel = new Label
                    {
                        Text = "Token:",
                        Location = new System.Drawing.Point(20, 130),
                        AutoSize = true
                    };

                    var tokenTextBox = new TextBox
                    {
                        Location = new System.Drawing.Point(80, 127),
                        Width = 380,
                        UseSystemPasswordChar = true
                    };

                    var loginButton2 = new Button
                    {
                        Text = "Login",
                        DialogResult = DialogResult.OK,
                        Location = new System.Drawing.Point(305, 165),
                        Width = 75
                    };

                    var cancelButton = new Button
                    {
                        Text = "Cancel",
                        DialogResult = DialogResult.Cancel,
                        Location = new System.Drawing.Point(385, 165),
                        Width = 75
                    };

                    tokenForm.Controls.Add(instructionLabel);
                    tokenForm.Controls.Add(tokenLabel);
                    tokenForm.Controls.Add(tokenTextBox);
                    tokenForm.Controls.Add(loginButton2);
                    tokenForm.Controls.Add(cancelButton);
                    tokenForm.AcceptButton = loginButton2;
                    tokenForm.CancelButton = cancelButton;

                    if (tokenForm.ShowDialog() == DialogResult.OK)
                    {
                        if (string.IsNullOrWhiteSpace(tokenTextBox.Text))
                        {
                            MessageBox.Show("Please enter a token", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        var (success, message) = await _githubService.AuthenticateWithTokenAsync(tokenTextBox.Text);

                        if (success)
                        {
                            loginButton.Text = $"Logout ({_githubService.CurrentUsername})";
                            statusLabel.Text = $"Logged in as {_githubService.CurrentUsername}";
                            await RefreshDiscussionsAsync();
                            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(message, "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            await RefreshDiscussionsAsync();
        }

        private async Task RefreshDiscussionsAsync()
        {
            try
            {
                statusLabel.Text = "Loading discussions...";
                listView.Items.Clear();

                _discussions = await _githubService.GetDiscussionsAsync();

                foreach (var discussion in _discussions)
                {
                    var item = new ListViewItem(discussion.Title);
                    item.SubItems.Add(discussion.Author);
                    item.SubItems.Add(discussion.UpvoteCount.ToString());
                    item.SubItems.Add(discussion.CommentCount.ToString());
                    item.SubItems.Add(discussion.CreatedAt.ToShortDateString());
                    item.Tag = discussion;
                    listView.Items.Add(item);
                }

                statusLabel.Text = $"Loaded {_discussions.Count} scripts";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load discussions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error loading discussions";
            }
        }

        private async void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
                return;

            var selectedItem = listView.SelectedItems[0];
            var discussion = selectedItem.Tag as ScriptGalleryItem;

            if (discussion == null)
                return;

            try
            {
                // Load full discussion details
                _selectedItem = await _githubService.GetDiscussionAsync(discussion.Number);
                
                // Display in WebView2
                await DisplayDiscussionAsync(_selectedItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load discussion details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DisplayDiscussionAsync(ScriptGalleryItem item)
        {
            if (!_webViewInitialized)
                return;

            // Convert markdown to HTML using Markdig with default pipeline
            var bodyHtml = Markdig.Markdown.ToHtml(item.Body ?? "");
            
            var commentsHtml = "";
            if (item.Comments != null && item.Comments.Any())
            {
                commentsHtml = "<h2>Comments</h2>";
                foreach (var comment in item.Comments)
                {
                    var commentBodyHtml = Markdig.Markdown.ToHtml(comment.Body ?? "");
                    commentsHtml += $@"
                        <div style='border: 1px solid #ddd; padding: 10px; margin: 10px 0; border-radius: 5px;'>
                            <div style='font-weight: bold; color: #0366d6;'>{comment.Author}</div>
                            <div style='font-size: 0.9em; color: #666;'>{comment.CreatedAt.ToString("g")}</div>
                            <div style='margin-top: 10px;'>{commentBodyHtml}</div>
                        </div>";
                }
            }

            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif;
            padding: 20px;
            background: white;
        }}
        h1 {{
            color: #24292e;
            border-bottom: 1px solid #e1e4e8;
            padding-bottom: 10px;
        }}
        .meta {{
            color: #586069;
            font-size: 14px;
            margin-bottom: 20px;
        }}
        pre {{
            background: #f6f8fa;
            padding: 16px;
            border-radius: 6px;
            overflow-x: auto;
        }}
        code {{
            background: #f6f8fa;
            padding: 2px 6px;
            border-radius: 3px;
            font-family: 'Courier New', monospace;
        }}
        pre code {{
            background: transparent;
            padding: 0;
        }}
    </style>
</head>
<body>
    <h1>{item.Title}</h1>
    <div class='meta'>
        By <strong>{item.Author}</strong> • 
        {item.CreatedAt.ToString("g")} • 
        👍 {item.UpvoteCount} • 
        💬 {item.CommentCount}
    </div>
    <div>{bodyHtml}</div>
    {commentsHtml}
</body>
</html>";

            detailWebView.NavigateToString(html);
        }

        private void LoadToEditorButton_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Please select a script first", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var scriptContent = _selectedItem.GetScriptContent();
            if (string.IsNullOrEmpty(scriptContent))
            {
                MessageBox.Show("No PowerShell script found in this discussion", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadScriptRequested?.Invoke(this, scriptContent);
        }

        private async void UpvoteButton_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Please select a script first", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!_githubService.IsAuthenticated)
            {
                MessageBox.Show("You must be logged in to upvote", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                await _githubService.UpvoteDiscussionAsync(_selectedItem.Id);
                MessageBox.Show("Upvoted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Refresh the current discussion
                _selectedItem = await _githubService.GetDiscussionAsync(_selectedItem.Number);
                await DisplayDiscussionAsync(_selectedItem);
                
                // Update list view
                foreach (ListViewItem item in listView.Items)
                {
                    if ((item.Tag as ScriptGalleryItem)?.Number == _selectedItem.Number)
                    {
                        item.SubItems[2].Text = _selectedItem.UpvoteCount.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to upvote: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void AddCommentButton_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Please select a script first", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!_githubService.IsAuthenticated)
            {
                MessageBox.Show("You must be logged in to comment", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(commentTextBox.Text))
            {
                MessageBox.Show("Please enter a comment", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var comment = await _githubService.AddCommentAsync(_selectedItem.Id, commentTextBox.Text);
                commentTextBox.Clear();
                
                MessageBox.Show("Comment added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Refresh the current discussion
                _selectedItem = await _githubService.GetDiscussionAsync(_selectedItem.Number);
                await DisplayDiscussionAsync(_selectedItem);
                
                // Update list view comment count
                foreach (ListViewItem item in listView.Items)
                {
                    if ((item.Tag as ScriptGalleryItem)?.Number == _selectedItem.Number)
                    {
                        item.SubItems[3].Text = _selectedItem.CommentCount.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add comment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task<bool> SaveScriptToGalleryAsync(string title, string scriptContent)
        {
            if (!_githubService.IsAuthenticated)
            {
                MessageBox.Show("You must be logged in to save scripts to the gallery", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            try
            {
                var newItem = await _githubService.CreateDiscussionAsync(title, scriptContent);
                MessageBox.Show($"Script saved to gallery successfully!\n\nDiscussion #{newItem.Number}: {newItem.Title}", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Refresh the list
                await RefreshDiscussionsAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save script to gallery: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}