using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System.Net;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class ScriptGalleryControl : UserControl
    {
        private GitHubService _githubService;
        private List<ScriptGalleryItem> _discussions;
        private ScriptGalleryItem _selectedItem;
        private bool _webViewInitialized;
        private string _currentSearchText;
        private List<string> _currentFilterTags;

        public event EventHandler<ScriptGalleryItem> LoadScriptRequested;

        public ScriptGalleryControl()
        {
            InitializeComponent();
            _githubService = new GitHubService();
            _currentFilterTags = new List<string>();
            
            // Update UI based on authentication state
            UpdateUIState();
            
            // Initialize WebView2
            InitializeWebViewAsync();

            commentPanel.Visible = false;
        }
        
        private void UpdateUIState()
        {
            bool isAuthenticated = _githubService.IsAuthenticated;
            
            if (isAuthenticated)
            {
                // Update login button and status
                loginButton.Text = $"Logout ({_githubService.CurrentUsername})";
                statusLabel.Text = $"Logged in as {_githubService.CurrentUsername}";
                
                // Enable controls
                refreshButton.Enabled = true;
                searchTextBox.Enabled = true;
                tagFilterComboBox.Enabled = true;
                applyFilterButton.Enabled = true;
                clearFilterButton.Enabled = true;
                mySubmissionsCheckBox.Enabled = true;
                listView.Enabled = true;
                loadToEditorButton.Enabled = true;
                upvoteButton.Enabled = true;
                thumbsDownButton.Enabled = true;
                editButton.Enabled = true;
                closeButton.Enabled = true;
                
                // Load data if not already loaded
                if (tagFilterComboBox.Items.Count <= 1) // Only "(All)" or empty
                {
                    LoadAvailableTagsAsync();
                }
            }
            else
            {
                // Update login button and status
                loginButton.Text = "Login to GitHub";
                statusLabel.Text = "Not logged in";
                
                // Disable controls
                refreshButton.Enabled = false;
                searchTextBox.Enabled = false;
                tagFilterComboBox.Enabled = false;
                applyFilterButton.Enabled = false;
                clearFilterButton.Enabled = false;
                mySubmissionsCheckBox.Enabled = false;
                listView.Enabled = false;
                loadToEditorButton.Enabled = false;
                upvoteButton.Enabled = false;
                thumbsDownButton.Enabled = false;
                editButton.Enabled = false;
                closeButton.Enabled = false;
                
                // Clear data
                listView.Items.Clear();
                if (_webViewInitialized)
                {
                    detailWebView.NavigateToString("<html><body><p>Please log in to GitHub to access the script gallery.</p></body></html>");
                }
                
                // Update status
                statusLabel.Text = "Please log in to GitHub to access the script gallery.";
            }
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
                // Initialize WebView2 directly instead of using MarkdownEditorHelper
                await detailWebView.EnsureCoreWebView2Async();
                _webViewInitialized = true;
                detailWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                detailWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                UpdateUIState();
                if (_githubService.IsAuthenticated)
                {
                    await RefreshDiscussionsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize WebView2: {ex.Message}\n\nWebView2 Runtime may not be installed.\n\nPlease download and install it from:\nhttps://developer.microsoft.com/microsoft-edge/webview2/",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpdateUIState();
            }
        }

        public void SetScriptEditorControl(ScriptEditorControl editorControl)
        {
            // Will be used to enable save to gallery from editor
        }
        
        private async Task LoadAvailableTagsAsync()
        {
            try
            {
                var tags = await _githubService.GetAvailableTagsAsync();
                tagFilterComboBox.Items.Clear();
                tagFilterComboBox.Items.Add("(All)");
                foreach (var tag in tags)
                {
                    tagFilterComboBox.Items.Add(tag);
                }
                if (tagFilterComboBox.Items.Count > 0)
                {
                    tagFilterComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load tags: {ex.Message}");
            }
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
                UpdateUIState();
            }
            else
            {
                // Use Device Flow authentication
                var (success, message, userCode, verificationUri) = await _githubService.InitiateDeviceFlowAsync();
                
                if (!success)
                {
                    MessageBox.Show(message, "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Show device flow dialog
                using (var deviceFlowForm = new Form
                {
                    Text = "GitHub Device Flow Authentication",
                    Width = 550,
                    Height = 250,
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                })
                {
                    var instructionLabel = new Label
                    {
                        Text = $"Please visit the following URL and enter the code:\n\n{verificationUri}\n\nCode: {userCode}\n\nThis window will close automatically when you authorize.",
                        AutoSize = false,
                        Location = new System.Drawing.Point(20, 20),
                        Size = new System.Drawing.Size(500, 100)
                    };

                    var copyCodeButton = new Button
                    {
                        Text = "Copy Code",
                        Location = new System.Drawing.Point(20, 130),
                        Size = new System.Drawing.Size(100, 30)
                    };
                    copyCodeButton.Click += (s, ev) =>
                    {
                        Clipboard.SetText(userCode);
                        MessageBox.Show("Code copied to clipboard", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    };

                    var openBrowserButton = new Button
                    {
                        Text = "Open Browser",
                        Location = new System.Drawing.Point(130, 130),
                        Size = new System.Drawing.Size(120, 30)
                    };
                    openBrowserButton.Click += (s, ev) =>
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = verificationUri,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to open browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    var statusLabelForm = new Label
                    {
                        Text = "Waiting for authorization...",
                        Location = new System.Drawing.Point(20, 170),
                        AutoSize = true
                    };

                    var cancelButton = new Button
                    {
                        Text = "Cancel",
                        Location = new System.Drawing.Point(430, 130),
                        Size = new System.Drawing.Size(90, 30)
                    };
                    cancelButton.Click += (s, ev) =>
                    {
                        deviceFlowForm.Close();
                    };

                    deviceFlowForm.Controls.Add(instructionLabel);
                    deviceFlowForm.Controls.Add(copyCodeButton);
                    deviceFlowForm.Controls.Add(openBrowserButton);
                    deviceFlowForm.Controls.Add(statusLabelForm);
                    deviceFlowForm.Controls.Add(cancelButton);

                    // Poll for authentication in background
                    var pollingTask = Task.Run(async () =>
                    {
                        // Wait a bit before starting to poll
                        await Task.Delay(5000);

                        for (int i = 0; i < 120; i++) // Poll for up to 10 minutes
                        {
                            try
                            {
                                var (authSuccess, authMessage, pending) = await _githubService.PollDeviceFlowAsync();

                                if (authSuccess)
                                {
                                    deviceFlowForm.Invoke((MethodInvoker)delegate
                                    {
                                        statusLabelForm.Text = authMessage;
                                        deviceFlowForm.Close();
                                    });
                                    break;
                                }
                                else if (!pending)
                                {
                                    // Error occurred
                                    deviceFlowForm.Invoke((MethodInvoker)delegate
                                    {
                                        statusLabelForm.Text = authMessage;
                                    });
                                    break;
                                }

                                await Task.Delay(5000); // Poll every 5 seconds
                            }
                            catch (Exception ex)
                            {
                                deviceFlowForm.Invoke((MethodInvoker)delegate
                                {
                                    statusLabelForm.Text = $"Error: {ex.Message}";
                                });
                                break;
                            }
                        }

                        // Timeout
                        if (!_githubService.IsAuthenticated)
                        {
                            try
                            {
                                deviceFlowForm.Invoke((MethodInvoker)delegate
                                {
                                    statusLabelForm.Text = "Authentication timed out";
                                });
                            }
                            catch { }
                        }
                    });

                    deviceFlowForm.ShowDialog();

                    if (_githubService.IsAuthenticated)
                    {
                        loginButton.Text = $"Logout ({_githubService.CurrentUsername})";
                        statusLabel.Text = $"Logged in as {_githubService.CurrentUsername}";
                        await RefreshDiscussionsAsync();
                        MessageBox.Show($"Successfully authenticated as {_githubService.CurrentUsername}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UpdateUIState();
                    }
                }
            }
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            await RefreshDiscussionsAsync();
        }
        
        private async void ApplyFilterButton_Click(object sender, EventArgs e)
        {
            _currentSearchText = searchTextBox.Text;
            _currentFilterTags.Clear();
            
            if (tagFilterComboBox.SelectedIndex > 0) // 0 is "(All)"
            {
                _currentFilterTags.Add(tagFilterComboBox.SelectedItem.ToString());
            }
            
            await RefreshDiscussionsAsync();
        }
        
        private async void ClearFilterButton_Click(object sender, EventArgs e)
        {
            searchTextBox.Text = "";
            tagFilterComboBox.SelectedIndex = 0;
            mySubmissionsCheckBox.Checked = false;
            _currentSearchText = null;
            _currentFilterTags.Clear();
            await RefreshDiscussionsAsync();
        }

        private async Task RefreshDiscussionsAsync()
        {
            try
            {
                statusLabel.Text = "Loading discussions...";
                listView.Items.Clear();

                bool onlyMySubmissions = mySubmissionsCheckBox.Checked;
                _discussions = await _githubService.GetDiscussionsAsync(_currentSearchText, _currentFilterTags, onlyMySubmissions);

                foreach (var discussion in _discussions)
                {
                    var item = new ListViewItem(discussion.Title);
                    item.SubItems.Add(discussion.Author);
                    item.SubItems.Add(string.Join(", ", discussion.Tags));
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
                
                // Show/hide edit and close buttons based on ownership
                bool isOwner = _githubService.IsAuthenticated && 
                              _selectedItem.Author == _githubService.CurrentUsername;
                editButton.Visible = isOwner;
                closeButton.Visible = isOwner;
                
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

            detailWebView.CoreWebView2.AddHostObjectToScript("gallery", new GalleryHostObject(this, System.Threading.SynchronizationContext.Current));

            // Convert markdown to HTML using Markdig with default pipeline
            var bodyHtml = Markdig.Markdown.ToHtml(item.Body ?? "");
            
            var commentsHtml = "";
            if (item.Comments != null && item.Comments.Any())
            {
                commentsHtml = "<h2>Comments</h2>" + GenerateCommentsHtml(item.Comments, 0);
            }

            var script = @"
    <script>
        var currentReplyToId = null;

        function toggleReplies(button, count) {
            var repliesDiv = button.parentElement.querySelector('.replies');
            if (repliesDiv.style.display === 'none') {
                repliesDiv.style.display = 'block';
                button.textContent = '▼ ' + count + ' replies';
            } else {
                repliesDiv.style.display = 'none';
                button.textContent = '▶ ' + count + ' replies';
            }
        }

        function showCommentForm() {
            currentReplyToId = null;
            if (window.chrome && window.chrome.webview && window.chrome.webview.hostObjects && window.chrome.webview.hostObjects.gallery) {
                window.chrome.webview.hostObjects.gallery.ShowCommentDialog(null);
            }
        }

        function showReplyForm(replyToId) {
            currentReplyToId = replyToId;
            if (window.chrome && window.chrome.webview && window.chrome.webview.hostObjects && window.chrome.webview.hostObjects.gallery) {
                window.chrome.webview.hostObjects.gallery.ShowCommentDialog(replyToId);
            }
        }
    </script>";

            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/vs.min.css"">
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js""></script>
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/powershell.min.js""></script>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Helvetica, Arial, sans-serif;
            font-size: 14px;
            padding: 10px;
            background: white;
        }}
        h1 {{
            color: #24292e;
            border-bottom: 1px solid #e1e4e8;
            padding-bottom: 5px;
            font-size: 1.2em;
        }}
        h2 {{
            font-size: 1.1em;
            margin-top: 15px;
            margin-bottom: 10px;
        }}
        .meta {{
            color: #586069;
            font-size: 12px;
            margin-bottom: 15px;
        }}
        pre {{
            background: #f6f8fa;
            padding: 12px;
            border-radius: 6px;
            overflow-x: auto;
        }}
        code {{
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
    <button onclick='showCommentForm()' style='margin-top: 10px;'>Leave a comment</button>
    {commentsHtml}
    {script}
    <script>hljs.highlightAll();</script>
</body>
</html>";

            detailWebView.NavigateToString(html);
        }

        private string GenerateCommentsHtml(List<DiscussionComment> comments, int depth)
        {
            if (comments == null || !comments.Any())
                return "";

            var html = "";
            foreach (var comment in comments)
            {
                var indentStyle = depth > 0 ? $"margin-left: {depth * 20}px; border-left: 2px solid #ddd; padding-left: 10px;" : "";
                var repliesHtml = comment.Replies.Any() ? GenerateCommentsHtml(comment.Replies, depth + 1) : "";
                var repliesStyle = comment.Replies.Any() ? "display: none;" : "";

                // Inline author and date
                var commentBodyHtml = Markdig.Markdown.ToHtml(comment.Body ?? "");
                html += $@"
                    <div style='border: 1px solid #ddd; padding: 10px; margin: 10px 0; border-radius: 5px; {indentStyle}'>
                        
                        <div style='font-weight: bold; color: #0366d6; display: inline;'>{WebUtility.HtmlEncode(comment.Author)}</div>
                        <span style='font-size: 0.9em; color: #666; margin-left: 10px;'>{WebUtility.HtmlEncode(comment.CreatedAt.ToString("g"))}</span>
                        <div style='margin-top: 10px;'>{commentBodyHtml}</div>
                        <button onclick='showReplyForm(""{WebUtility.HtmlEncode(comment.Id)}"")' style='margin-top: 5px; font-size: 0.8em; padding: 2px 8px; background: #28a745; color: white; border: none; border-radius: 3px; cursor: pointer;'>Reply</button>
                        <button class='toggleRepliesButton' onclick='toggleReplies(this, {CountTotalReplies(comment)})' style='margin-top: 5px; font-size: 0.8em; padding: 2px 8px; background: #007bff; color: white; border: none; border-radius: 3px; cursor: pointer;'>
                            ▶ {CountTotalReplies(comment)} replies
                        </button>
                        <div class='replies' style='{repliesStyle}'>
                            {repliesHtml}
                        </div>
                    </div>";
            }
            return html;
        }

        private int CountTotalReplies(DiscussionComment comment)
        {
            int count = comment.Replies.Count;
            foreach (var reply in comment.Replies)
            {
                count += CountTotalReplies(reply);
            }
            return count;
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

            LoadScriptRequested?.Invoke(this, _selectedItem);
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

        public async Task ShowSaveScriptDialog(string scriptContent, ScriptGalleryItem existingItem = null)
        {
            if (!_githubService.IsAuthenticated)
            {
                MessageBox.Show("You must be logged in to save scripts to the gallery", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Prepare initial values
            string title = existingItem?.Title ?? "Untitled Script";
            string description = "";
            List<string> selectedTags = existingItem?.Tags ?? new List<string>();

            if (existingItem != null)
            {
                // Extract description from body (remove script block)
                var parts = existingItem.Body.Split(new[] { "```powershell", "```ps1" }, StringSplitOptions.None);
                if (parts.Length > 0)
                {
                    description = parts[0].Trim();
                }
            }

            // Get available tags
            var availableTags = new List<string>();
            foreach (var item in tagFilterComboBox.Items)
            {
                availableTags.Add(item.ToString());
            }



            using (var dialog = new ScriptSaveDialog(title, description, availableTags, selectedTags))
            {

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string newTitle = dialog.ScriptTitle;
                        string newDescription = await dialog.GetDescriptionAsync();
                        List<string> newTags = dialog.SelectedTags;

                        // Combine description and script
                        string newBody = $"{newDescription}\n\n```powershell\n{scriptContent}\n```";

                        if (existingItem != null)
                        {
                            await _githubService.UpdateDiscussionAsync(existingItem.Id, newTitle, newBody);
                            MessageBox.Show("Script updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            var newItem = await _githubService.CreateDiscussionAsync(newTitle, newBody, newTags);
                            MessageBox.Show($"Script saved to gallery successfully!\n\nDiscussion #{newItem.Number}: {newItem.Title}",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        // Refresh
                        await LoadAvailableTagsAsync();
                        await RefreshDiscussionsAsync();

                        if (existingItem != null && _selectedItem?.Id == existingItem.Id)
                        {
                            _selectedItem = await _githubService.GetDiscussionAsync(_selectedItem.Number);
                            await DisplayDiscussionAsync(_selectedItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save script: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    
        

        private async void ThumbsDownButton_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Please select a script first", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!_githubService.IsAuthenticated)
            {
                MessageBox.Show("You must be logged in to react", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                await _githubService.AddReactionAsync(_selectedItem.Id, "THUMBS_DOWN");
                MessageBox.Show("Reaction added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Refresh discussion to update reaction counts
                await RefreshDiscussionsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add reaction: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private async void EditButton_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Please select a script first", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!_githubService.IsAuthenticated)
            {
                MessageBox.Show("You must be logged in to edit", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var scriptContent = _selectedItem.GetScriptContent();
            await ShowSaveScriptDialog(scriptContent, _selectedItem);
        }
        
        private async void CloseButton_Click(object sender, EventArgs e)
        {
            if (_selectedItem == null)
            {
                MessageBox.Show("Please select a script first", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!_githubService.IsAuthenticated)
            {
                MessageBox.Show("You must be logged in to close", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to close this discussion?\n\nClosed discussions will be hidden from the gallery.",
                "Confirm Close", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    await _githubService.CloseDiscussionAsync(_selectedItem.Id);
                    MessageBox.Show("Discussion closed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Refresh to remove from list
                    await RefreshDiscussionsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to close discussion: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        [System.Runtime.InteropServices.ComVisible(true)]
        public class GalleryHostObject
        {
            private readonly ScriptGalleryControl _control;
            private readonly System.Threading.SynchronizationContext _syncContext;

            public GalleryHostObject(ScriptGalleryControl control)
            {
                _control = control;
            }

            public GalleryHostObject(ScriptGalleryControl control, System.Threading.SynchronizationContext syncContext)
            {
                _control = control;
                _syncContext = syncContext;
            }

            public void ShowCommentDialog(string replyToId)
            {
                if (_syncContext != null)
                {
                    _syncContext.Post(_ => _control.ShowCommentDialog(replyToId), null);
                }
                else if (_control.InvokeRequired)
                {
                    _control.Invoke(new Action(() => { var _ = _control.ShowCommentDialog(replyToId); }));
                }
                else
                {
                    var _ = _control.ShowCommentDialog(replyToId);
                }
            }
        }

        public async Task ShowCommentDialog(string replyToId)
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

            using (var dialog = new CommentDialog())
            {
                // No async initialization required for dialog
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string body = await dialog.GetCommentAsync();
                    if (string.IsNullOrWhiteSpace(body))
                    {
                        MessageBox.Show("Please enter a comment", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    try
                    {
                        var comment = await _githubService.AddCommentAsync(_selectedItem.Id, body, replyToId);
                        
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
            }
        }

        private async void AddCommentButton_Click(object sender, EventArgs e)
        {
            // Not used, using HTML version
        }
    }
}