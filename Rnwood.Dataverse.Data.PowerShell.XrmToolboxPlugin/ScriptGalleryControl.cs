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
        private string _currentSearchText;
        private List<string> _currentFilterTags;

        public event EventHandler<string> LoadScriptRequested;

        public ScriptGalleryControl()
        {
            InitializeComponent();
            _githubService = new GitHubService();
            _currentFilterTags = new List<string>();
            
            // Initialize WebView2
            InitializeWebViewAsync();
            
            // Update UI based on authentication state
        }
        
        private void UpdateUIState()
        {
            bool isAuthenticated = _githubService.IsAuthenticated;
            
            if (isAuthenticated)
            {
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
                addCommentButton.Enabled = true;
                commentTextBox.Enabled = true;
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
                addCommentButton.Enabled = false;
                commentTextBox.Enabled = false;
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
                await detailWebView.EnsureCoreWebView2Async(null);
                _webViewInitialized = true;
                detailWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                detailWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                UpdateUIState();
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

        public async Task<bool> SaveScriptToGalleryAsync(string title, string scriptContent, List<string> tags = null)
        {
            if (!_githubService.IsAuthenticated)
            {
                MessageBox.Show("You must be logged in to save scripts to the gallery", "Authentication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            try
            {
                var newItem = await _githubService.CreateDiscussionAsync(title, scriptContent, tags);
                MessageBox.Show($"Script saved to gallery successfully!\n\nDiscussion #{newItem.Number}: {newItem.Title}", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Refresh the list and tags
                await LoadAvailableTagsAsync();
                await RefreshDiscussionsAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save script to gallery: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
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

            // Show edit dialog
            using (var editForm = new Form
            {
                Text = "Edit Script",
                Width = 600,
                Height = 500,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var titleLabel = new Label
                {
                    Text = "Title:",
                    Location = new System.Drawing.Point(10, 20),
                    AutoSize = true
                };
                
                var titleTextBox = new TextBox
                {
                    Location = new System.Drawing.Point(10, 40),
                    Width = 560,
                    Text = _selectedItem.Title
                };
                
                var bodyLabel = new Label
                {
                    Text = "Body:",
                    Location = new System.Drawing.Point(10, 70),
                    AutoSize = true
                };
                
                var bodyTextBox = new TextBox
                {
                    Location = new System.Drawing.Point(10, 90),
                    Width = 560,
                    Height = 300,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    Text = _selectedItem.Body
                };
                
                var saveButton = new Button
                {
                    Text = "Save",
                    DialogResult = DialogResult.OK,
                    Location = new System.Drawing.Point(415, 410),
                    Width = 75
                };
                
                var cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new System.Drawing.Point(495, 410),
                    Width = 75
                };
                
                editForm.Controls.Add(titleLabel);
                editForm.Controls.Add(titleTextBox);
                editForm.Controls.Add(bodyLabel);
                editForm.Controls.Add(bodyTextBox);
                editForm.Controls.Add(saveButton);
                editForm.Controls.Add(cancelButton);
                editForm.AcceptButton = saveButton;
                editForm.CancelButton = cancelButton;
                
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await _githubService.UpdateDiscussionAsync(_selectedItem.Id, titleTextBox.Text, bodyTextBox.Text);
                        MessageBox.Show("Script updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Refresh
                        await RefreshDiscussionsAsync();
                        _selectedItem = await _githubService.GetDiscussionAsync(_selectedItem.Number);
                        await DisplayDiscussionAsync(_selectedItem);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update script: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
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
    }
}