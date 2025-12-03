using System;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;

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

        // Tab data
        private Dictionary<TabPage, ScriptTabContentControl> tabData = new Dictionary<TabPage, ScriptTabContentControl>();
        private int untitledCounter = 1;
        private Func<string> _accessTokenProvider;
        private string _url;

        public event EventHandler RunScriptRequested;
        public event EventHandler NewScriptRequested;
        public event EventHandler OpenScriptRequested;
        public event EventHandler SaveScriptRequested;
        public event EventHandler SaveToPasteRequested;
        public event EventHandler<CompletionItem> CompletionResolved;

        public ScriptEditorControl()
        {
            InitializeComponent();
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
            content.SaveRequested += (s, e) => SaveScriptRequested?.Invoke(this, EventArgs.Empty);
            content.CompletionResolved += (s, e) => CompletionResolved?.Invoke(this, e);
            content.CloseRequested += (s, e) => {
                tabControl.TabPages.Remove(tabPage);
                if (tabData.ContainsKey(tabPage))
                {
                    tabData[tabPage].WebView?.Dispose();
                    tabData.Remove(tabPage);
                }
            };
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

        private void SaveToGistButton_Click(object sender, EventArgs e)
        {
            SaveToPaste();
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

        private async Task SaveCurrentScript()
        {
            try
            {
                if (tabControl.SelectedTab == null || !tabData.ContainsKey(tabControl.SelectedTab))
                    return;

                var content = tabData[tabControl.SelectedTab];
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
                tabControl.SelectedTab.Text = Path.GetFileName(scriptPath);

                MessageBox.Show($"Script saved to: {scriptPath}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save script: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CreateNewScript()
        {
            CreateNewScriptTab();
        }

        public async void OpenScript()
        {
            await OpenScriptTab();
        }

        public async void SaveScript()
        {
            await SaveCurrentScript();
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

        /// <summary>
        /// Opens a script from a GitHub Gist in a new editor tab
        /// </summary>
        public async Task OpenFromPasteAsync(PasteInfo paste)
        {
            try
            {
                var content = paste.Content;
                if (string.IsNullOrEmpty(content))
                {
                    MessageBox.Show("No content found in this paste.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var title = paste.GetDisplayTitle();
                
                TabPage tabPage = CreateScriptTab(title, null);
                tabControl.TabPages.Add(tabPage);
                tabControl.SelectedTab = tabPage;

                // Store paste info in tab for later save
                tabPage.Tag = paste;

                // Initialize the webView
                await tabData[tabPage].InitializeWebView();
                tabData[tabPage].SetScriptContentAsync(content);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open paste: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gets the current tab's associated paste (if opened from gallery)
        /// </summary>
        public PasteInfo GetCurrentTabPaste()
        {
            if (tabControl.SelectedTab != null)
            {
                return tabControl.SelectedTab.Tag as PasteInfo;
            }
            return null;
        }

        /// <summary>
        /// Saves the current script to PasteBin
        /// </summary>
        public void SaveToPaste()
        {
            SaveToPasteRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
