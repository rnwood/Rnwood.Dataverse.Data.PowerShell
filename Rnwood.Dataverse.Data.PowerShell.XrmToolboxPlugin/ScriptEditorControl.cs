using System;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class ScriptEditorControl : UserControl
    {
        // PowerShell completion service
        private PowerShellCompletionService _completionService;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel completionStatusLabel;
        private int _activeCompletionRequests = 0;
        private string _lastCompletionError;

        private Func<string> _accessTokenProvider;
        private string _url;
        
        // Gallery control reference
        private ScriptGalleryControl _galleryControl;

        // Tab data
        private Dictionary<TabPage, ScriptTabContentControl> tabData = new Dictionary<TabPage, ScriptTabContentControl>();
        private int untitledCounter = 1;

        public event EventHandler RunScriptRequested;
        public event EventHandler<CompletionItem> CompletionResolved;

        public ScriptEditorControl()
        {
            InitializeComponent();
        }
        
        public void SetGalleryControl(ScriptGalleryControl galleryControl)
        {
            _galleryControl = galleryControl;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Create initial tab
            CreateNewScriptTab();
        }

        private TabPage CreateScriptTab(string title, string path)
        {
            TabPage tabPage = new TabPage(title);
            tabPage.UseVisualStyleBackColor = true;

            ScriptTabContentControl content = new ScriptTabContentControl();
            content.Path = path;
 
            content.CompletionService = _completionService;
            content.RunRequested += (s, e) => RunScriptRequested?.Invoke(this, EventArgs.Empty);
            content.CompletionResolved += (s, e) => CompletionResolved?.Invoke(this, e);
            content.CloseRequested += (s, e) => {
                tabControl.TabPages.Remove(tabPage);
                if (tabData.ContainsKey(tabPage))
                {
                    tabData[tabPage].WebView?.Dispose();
                    tabData.Remove(tabPage);
                }
            };
            content.SaveRequested += async (s, e) => await SaveScriptForContent(s as ScriptTabContentControl);
            tabPage.Controls.Add(content);
            content.Dock = DockStyle.Fill;

            tabData[tabPage] = content;

            return tabPage;
        }

        public async void InitializeMonacoEditor(Func<string> accessTokenProvider = null, string url = null)
        {
            this._accessTokenProvider = accessTokenProvider;
            this._url = url;

            try
            {
                // Initialize PowerShell completion service
                string modulePath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "PSModule",
                    "Rnwood.Dataverse.Data.PowerShell.psd1"
                );

                _completionService = new PowerShellCompletionService(modulePath);

                // Subscribe to completion events
                _completionService.CompletionRequestStarted += OnCompletionRequestStarted;
                _completionService.CompletionRequestCompleted += OnCompletionRequestCompleted;
                _completionService.CompletionRequestFailed += OnCompletionRequestFailed;

                // Initialize the service asynchronously
                await _completionService.InitializeAsync();

                // Set completion service for existing tabs
                foreach (var content in tabData.Values)
                {
                    content.CompletionService = _completionService;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize script editor: {ex.Message}\n\nWebView2 Runtime may not be installed.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void NewScriptButton_Click(object sender, EventArgs e)
        {
            CreateNewScriptTab();
        }

        private void OpenScriptButton_Click(object sender, EventArgs e)
        {
            OpenScriptTab();
        }

        public async Task<string> GetScriptContentAsync()
        {
            if (tabControl.SelectedTab == null || !tabData.ContainsKey(tabControl.SelectedTab))
                return string.Empty;

            var content = tabData[tabControl.SelectedTab];
            return await content.GetScriptContentAsync();
        }

        public async void SetScriptContentAsync(string content)
        {
            if (tabControl.SelectedTab == null || !tabData.ContainsKey(tabControl.SelectedTab))
                return;

            var control = tabData[tabControl.SelectedTab];
            control.SetScriptContentAsync(content);
        }

        public PowerShellVersion GetCurrentPowerShellVersion()
        {
            if (tabControl.SelectedTab == null || !tabData.ContainsKey(tabControl.SelectedTab))
                return PowerShellDetector.GetDefaultVersion();

            var content = tabData[tabControl.SelectedTab];
            return content.PowerShellVersion;
        }

        public string GetCurrentFileName()
        {
            if (tabControl.SelectedTab == null || !tabData.ContainsKey(tabControl.SelectedTab))
                return "Script";

            var content = tabData[tabControl.SelectedTab];
            if (!string.IsNullOrEmpty(content.Path))
                return Path.GetFileName(content.Path);
            return tabControl.SelectedTab.Text;
        }

        public async void CreateNewScriptTab()
        {
            try
            {
                string title = $"Untitled-{untitledCounter++}";
                TabPage tabPage = CreateScriptTab(title, null);
                tabControl.TabPages.Add(tabPage);
                tabControl.SelectedTab = tabPage;

                // Initialize the webView
                await tabData[tabPage].InitializeWebView();

                // Default content is already set in the HTML
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create new script: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task OpenScriptTab()
        {
            try
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Filter = "PowerShell Scripts (*.ps1)|*.ps1|All Files (*.*)|*.*";
                    dialog.Title = "Open PowerShell Script";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string content = File.ReadAllText(dialog.FileName);
                        string title = Path.GetFileName(dialog.FileName);
                        TabPage tabPage = CreateScriptTab(title, dialog.FileName);
                        tabControl.TabPages.Add(tabPage);
                        tabControl.SelectedTab = tabPage;

                        // Initialize the webView
                        await tabData[tabPage].InitializeWebView();

                        tabData[tabPage].SetScriptContentAsync(content);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open script: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SaveScriptForContent(ScriptTabContentControl content)
        {
            try
            {
                string scriptPath = content.Path;
                if (string.IsNullOrEmpty(scriptPath))
                {
                    using (SaveFileDialog dialog = new SaveFileDialog())
                    {
                        dialog.Filter = "PowerShell Scripts (*.ps1)|*.ps1|All Files (*.*)|*.*";
                        dialog.Title = "Save PowerShell Script";
                        dialog.DefaultExt = "ps1";

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            scriptPath = dialog.FileName;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                // Get script content
                string script = await content.GetScriptContentAsync();

                File.WriteAllText(scriptPath, script);
                content.Path = scriptPath;

                // Update tab text
                var tab = tabData.FirstOrDefault(kvp => kvp.Value == content).Key;
                if (tab != null)
                {
                    tab.Text = Path.GetFileName(scriptPath);
                }

                MessageBox.Show($"Script saved to: {scriptPath}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save script: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DisposeResources()
        {
            // Unsubscribe from completion events
            if (_completionService != null)
            {
                _completionService.CompletionRequestStarted -= OnCompletionRequestStarted;
                _completionService.CompletionRequestCompleted -= OnCompletionRequestCompleted;
                _completionService.CompletionRequestFailed -= OnCompletionRequestFailed;
            }

            if (_completionService != null)
            {
                try
                {
                    _completionService.Dispose();
                }
                catch
                {
                    // Ignore errors when disposing
                }
            }

            foreach (var content in tabData.Values)
            {
                try
                {
                    content.WebView?.Dispose();
                }
                catch
                {
                    // Ignore errors when disposing
                }
            }
            tabData.Clear();
        }

        private void OnCompletionRequestStarted(object sender, EventArgs e)
        {
            _activeCompletionRequests++;
            UpdateCompletionStatus(null, null); // Update with current status
        }

        private void OnCompletionRequestCompleted(object sender, EventArgs e)
        {
            _activeCompletionRequests--;
            UpdateCompletionStatus(null, null); // Update with current status
        }

        private void OnCompletionRequestFailed(object sender, CompletionErrorEventArgs e)
        {
            _activeCompletionRequests--;
            _lastCompletionError = e.ErrorMessage;
            UpdateCompletionStatus(null, null); // Update with current status
        }

        private void UpdateCompletionStatus(string status, string errorMessage)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateCompletionStatus(status, errorMessage)));
                return;
            }

            string currentStatus = status ?? GetCurrentCompletionStatus();
            string tooltip = errorMessage ?? _lastCompletionError;

            completionStatusLabel.Text = $"Completion: {currentStatus}";
            completionStatusLabel.ToolTipText = string.IsNullOrEmpty(tooltip) ? "" : tooltip;
        }

        private string GetCurrentCompletionStatus()
        {
            if (_completionService == null || !_completionService.IsInitialized)
            {
                return "Not initialized";
            }

            string baseStatus = "Initialized";
            if (!string.IsNullOrEmpty(_lastCompletionError))
            {
                baseStatus = "Error";
            }

            return $"{baseStatus} ({_activeCompletionRequests} active)";
        }
        
        private async void SaveToGalleryButton_Click(object sender, EventArgs e)
        {
            if (_galleryControl == null)
            {
                MessageBox.Show("Gallery control not available", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // Get current script content
            string scriptContent = await GetScriptContentAsync();
            
            if (string.IsNullOrWhiteSpace(scriptContent))
            {
                MessageBox.Show("Script is empty. Please write some PowerShell code first.", 
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // Prompt for title and tags
            using (var titleForm = new Form
            {
                Text = "Save to Gallery",
                Width = 400,
                Height = 220,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var titleLabel = new Label
                {
                    Text = "Script Title:",
                    Location = new System.Drawing.Point(10, 20),
                    AutoSize = true
                };
                
                var titleTextBox = new TextBox
                {
                    Location = new System.Drawing.Point(10, 40),
                    Width = 360,
                    Text = tabControl.SelectedTab?.Text ?? "Untitled Script"
                };
                
                var tagsLabel = new Label
                {
                    Text = "Tags (comma-separated, e.g. sql, data-migration):",
                    Location = new System.Drawing.Point(10, 70),
                    AutoSize = true,
                    Width = 360
                };
                
                var tagsTextBox = new TextBox
                {
                    Location = new System.Drawing.Point(10, 90),
                    Width = 360
                };
                
                var saveButton = new Button
                {
                    Text = "Save",
                    DialogResult = DialogResult.OK,
                    Location = new System.Drawing.Point(210, 145),
                    Width = 75
                };
                
                var cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new System.Drawing.Point(295, 145),
                    Width = 75
                };
                
                titleForm.Controls.Add(titleLabel);
                titleForm.Controls.Add(titleTextBox);
                titleForm.Controls.Add(tagsLabel);
                titleForm.Controls.Add(tagsTextBox);
                titleForm.Controls.Add(saveButton);
                titleForm.Controls.Add(cancelButton);
                titleForm.AcceptButton = saveButton;
                titleForm.CancelButton = cancelButton;
                
                if (titleForm.ShowDialog() == DialogResult.OK)
                {
                    string title = titleTextBox.Text;
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        MessageBox.Show("Please enter a title", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    
                    // Parse tags
                    var tags = new List<string>();
                    if (!string.IsNullOrWhiteSpace(tagsTextBox.Text))
                    {
                        tags = tagsTextBox.Text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .Where(t => !string.IsNullOrEmpty(t))
                            .ToList();
                    }
                    
                    await _galleryControl.SaveScriptToGalleryAsync(title, scriptContent, tags);
                }
            }
        }
    }
}
