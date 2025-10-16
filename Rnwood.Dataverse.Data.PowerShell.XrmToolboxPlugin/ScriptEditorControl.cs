using System;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.Management.Automation;
using System.Text.Json;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class ScriptEditorControl : UserControl
    {
        private ToolStrip editorToolbar;
        private ToolStripButton newScriptButton;
        private ToolStripButton openScriptButton;
        private TabControl tabControl;
        private ImageList toolbarImages;

        // PowerShell completion service
        private PowerShellCompletionService _completionService;

        // Tab data
        private Dictionary<TabPage, (WebView2 webView, string path)> tabData = new Dictionary<TabPage, (WebView2, string)>();
        private Dictionary<WebView2, TaskCompletionSource<bool>> _webViewReadyTasks = new Dictionary<WebView2, TaskCompletionSource<bool>>();
        private int untitledCounter = 1;
        private string _accessToken;
        private System.ComponentModel.IContainer components;
        private TabPage tabPage1;
        private string _url;

        public event EventHandler RunScriptRequested;
        public event EventHandler NewScriptRequested;
        public event EventHandler OpenScriptRequested;
        public event EventHandler SaveScriptRequested;

        public ScriptEditorControl()
        {
            InitializeComponent();
            InitializeToolbarImages();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.editorToolbar = new System.Windows.Forms.ToolStrip();
            this.toolbarImages = new System.Windows.Forms.ImageList(this.components);
            this.newScriptButton = new System.Windows.Forms.ToolStripButton();
            this.openScriptButton = new System.Windows.Forms.ToolStripButton();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.editorToolbar.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // editorToolbar
            // 
            this.editorToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.editorToolbar.ImageList = this.toolbarImages;
            this.editorToolbar.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.editorToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newScriptButton,
            this.openScriptButton});
            this.editorToolbar.Location = new System.Drawing.Point(0, 0);
            this.editorToolbar.Name = "editorToolbar";
            this.editorToolbar.Size = new System.Drawing.Size(800, 40);
            this.editorToolbar.TabIndex = 0;
            // 
            // toolbarImages
            // 
            this.toolbarImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.toolbarImages.ImageSize = new System.Drawing.Size(16, 16);
            this.toolbarImages.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // newScriptButton
            // 
            this.newScriptButton.Name = "newScriptButton";
            this.newScriptButton.Size = new System.Drawing.Size(59, 34);
            this.newScriptButton.Text = "New";
            this.newScriptButton.Click += new System.EventHandler(this.NewScriptButton_Click);
            // 
            // openScriptButton
            // 
            this.openScriptButton.Name = "openScriptButton";
            this.openScriptButton.Size = new System.Drawing.Size(68, 34);
            this.openScriptButton.Text = "Open";
            this.openScriptButton.Click += new System.EventHandler(this.OpenScriptButton_Click);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabControl.Location = new System.Drawing.Point(0, 40);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(800, 560);
            this.tabControl.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 33);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(792, 523);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // ScriptEditorControl
            // 
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.editorToolbar);
            this.Name = "ScriptEditorControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.editorToolbar.ResumeLayout(false);
            this.editorToolbar.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

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

            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            tabPage.Controls.Add(panel);


            WebView2 webView = new WebView2();
            webView.Dock = DockStyle.Fill;
            panel.Controls.Add(webView);

            // Add tab-specific toolbar
            ToolStrip tabToolbar = new ToolStrip();
            tabToolbar.ImageList = this.toolbarImages;
            tabToolbar.GripStyle = ToolStripGripStyle.Hidden;
            tabToolbar.Dock = DockStyle.Top;

            ToolStripButton tabRunButton = new ToolStripButton();
            tabRunButton.Text = "Run (F5)";
            tabRunButton.ImageIndex = 0;
            tabRunButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            tabRunButton.Click += (s, e) => RunScriptRequested?.Invoke(this, EventArgs.Empty);

            ToolStripButton tabSaveButton = new ToolStripButton();
            tabSaveButton.Text = "Save";
            tabSaveButton.ImageIndex = 3;
            tabSaveButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            tabSaveButton.Click += (s, e) => SaveCurrentScript();

            tabToolbar.Items.AddRange(new ToolStripItem[] { tabRunButton, tabSaveButton });

            panel.Controls.Add(tabToolbar);


            // Add close button
            Button closeButton = new Button();
            closeButton.Text = "X";
            closeButton.Size = new Size(20, 20);
            closeButton.Location = new Point(panel.Width - 25, tabToolbar.Height + 5);
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.Click += (s, e) => {
                tabControl.TabPages.Remove(tabPage);
                if (tabData.ContainsKey(tabPage))
                {
                    var data = tabData[tabPage];
                    data.webView?.Dispose();
                    tabData.Remove(tabPage);
                }
            };
            panel.Controls.Add(closeButton);
            closeButton.BringToFront();

            tabData[tabPage] = (webView, path);

            return tabPage;
        }

        private void InitializeToolbarImages()
        {
            // toolbarImages
            this.toolbarImages.ImageSize = new Size(16, 16);
            this.toolbarImages.ColorDepth = ColorDepth.Depth32Bit;

            // Run icon: play triangle
            Bitmap runBmp = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(runBmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillPolygon(Brushes.Green, new Point[] { new Point(3, 3), new Point(3, 13), new Point(13, 8) });
            }
            this.toolbarImages.Images.Add("Run", runBmp);

            // New icon: plus
            Bitmap newBmp = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(newBmp))
            {
                g.DrawLine(Pens.Black, 8, 2, 8, 14);
                g.DrawLine(Pens.Black, 2, 8, 14, 8);
            }
            this.toolbarImages.Images.Add("New", newBmp);

            // Open icon: folder
            Bitmap openBmp = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(openBmp))
            {
                g.FillRectangle(Brushes.Yellow, 2, 4, 12, 10);
                g.FillRectangle(Brushes.Yellow, 2, 2, 8, 4);
                g.DrawRectangle(Pens.Black, 2, 4, 12, 10);
                g.DrawRectangle(Pens.Black, 2, 2, 8, 4);
            }
            this.toolbarImages.Images.Add("Open", openBmp);

            // Save icon: disk
            Bitmap saveBmp = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(saveBmp))
            {
                g.FillRectangle(Brushes.Gray, 3, 3, 10, 10);
                g.DrawRectangle(Pens.Black, 3, 3, 10, 10);
                g.FillRectangle(Brushes.White, 5, 5, 6, 6);
                g.DrawRectangle(Pens.Black, 5, 5, 6, 6);
            }
            this.toolbarImages.Images.Add("Save", saveBmp);
        }

        private async Task InitializeWebView(WebView2 webView)
        {
            _webViewReadyTasks[webView] = new TaskCompletionSource<bool>();

            try
            {
                await webView.EnsureCoreWebView2Async(null);

                // Load Monaco editor HTML
                string monacoHtml = GenerateMonacoEditorHtml();
                webView.NavigateToString(monacoHtml);

                // Setup message handler for script operations
                webView.WebMessageReceived += EditorWebView_WebMessageReceived;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize script editor: {ex.Message}\n\nWebView2 Runtime may not be installed.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            await _webViewReadyTasks[webView].Task;
        }

        public async void InitializeMonacoEditor(string accessToken = null, string url = null)
        {
            this._accessToken = accessToken;
            this._url = url;

            try
            {
                // Initialize PowerShell completion service
                string modulePath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "PSModule",
                    "Rnwood.Dataverse.Data.PowerShell.psd1"
                );

                _completionService = new PowerShellCompletionService(modulePath, accessToken, url);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _completionService.InitializeAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to initialize completion service: {ex.Message}");
                    }
                });

                // Initialize all existing webViews
                foreach (var data in tabData.Values)
                {
                    await InitializeWebView(data.webView);
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

            var webView = tabData[tabControl.SelectedTab].webView;
            try
            {
                // Get script content from Monaco editor
                string script = await webView.ExecuteScriptAsync("getContent()");

                // Remove JSON string quotes
                script = script.Trim('"').Replace("\\n", "\n").Replace("\\r", "\r")
                    .Replace("\\\"", "\"").Replace("\\\\", "\\");

                return script;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to get script content: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        public async void SetScriptContentAsync(string content)
        {
            if (tabControl.SelectedTab == null || !tabData.ContainsKey(tabControl.SelectedTab))
                return;

            var webView = tabData[tabControl.SelectedTab].webView;
            try
            {
                // Escape for JavaScript
                content = content.Replace("\\", "\\\\")
                               .Replace("'", "\\'")
                               .Replace("\n", "\\n")
                               .Replace("\r", "\\r")
                               .Replace("\"", "\\\"");

                await webView.ExecuteScriptAsync($"setContent('{content}')");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set script content: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                await InitializeWebView(tabData[tabPage].webView);

                // Set default content
                SetScriptContentAsync(GetDefaultScriptContent());
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
                        await InitializeWebView(tabData[tabPage].webView);

                        SetScriptContentAsync(content);
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

                var (webView, path) = tabData[tabControl.SelectedTab];
                string scriptPath = path;

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
                string script = await GetScriptContentAsync();

                File.WriteAllText(scriptPath, script);
                tabData[tabControl.SelectedTab] = (webView, scriptPath);
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

        private string GenerateMonacoEditorHtml()
        {
            string defaultContent = GetDefaultScriptContent()
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\"", "\\\"");

            string html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <style>
        body {{ margin: 0; padding: 0; overflow: hidden; }}
        #container {{ width: 100%; height: 100vh; }}
    </style>
</head>
<body>
    <div id=""container""></div>
    
    <script src=""https://unpkg.com/monaco-editor@0.45.0/min/vs/loader.js""></script>
    <script>
        require.config({{ paths: {{ vs: 'https://unpkg.com/monaco-editor@0.45.0/min/vs' }} }});
        
        require(['vs/editor/editor.main'], function() {{
            // Create Monaco editor
            window.editor = monaco.editor.create(document.getElementById('container'), {{
                value: __DEFAULT_SCRIPT_CONTENT__,
                language: 'powershell',
                theme: 'vs-dark',
                automaticLayout: true,
                fontSize: 14,
                minimap: {{ enabled: true }},
                scrollBeyondLastLine: false,
                wordWrap: 'on',
                lineNumbers: 'on',
                folding: true,
                renderWhitespace: 'selection'
            }});
            
            // Store for pending completion requests
            var pendingCompletionRequests = {{}};
            
            // Handler for completion responses from C#
            window.handleCompletionResponse = function(response) {{
                if (response.requestId && pendingCompletionRequests[response.requestId]) {{
                    var resolve = pendingCompletionRequests[response.requestId];
                    delete pendingCompletionRequests[response.requestId];
                    resolve(response.completions || []);
                }}
            }};
            
            // Register dynamic PowerShell completion provider using LSP
            monaco.languages.registerCompletionItemProvider('powershell', {{
                provideCompletionItems: async function(model, position) {{
                    var word = model.getWordUntilPosition(position);
                    var range = {{
                        startLineNumber: position.lineNumber,
                        endLineNumber: position.lineNumber,
                        startColumn: word.startColumn,
                        endColumn: word.endColumn
                    }};
                    
                    // Get the full script text and cursor position
                    var script = model.getValue();
                    var cursorOffset = model.getOffsetAt(position);
                    
                    // Create a unique request ID
                    var requestId = 'completion_' + Date.now() + '_' + Math.random();
                    
                    // Send completion request to C#
                    var completionPromise = new Promise(function(resolve) {{
                        pendingCompletionRequests[requestId] = resolve;
                        window.chrome.webview.postMessage({{
                            action: 'completion',
                            requestId: requestId,
                            script: script,
                            cursorPosition: cursorOffset
                        }});
                    }});
                    
                    // Wait for response with timeout
                    var timeoutPromise = new Promise(function(resolve) {{
                        setTimeout(function() {{ resolve([]); }}, 5000);
                    }});
                    
                    var completions = await Promise.race([completionPromise, timeoutPromise]);
                    
                    // Map completions to Monaco format
                    return {{
                        suggestions: completions.map(function(c) {{
                            return {{
                                label: c.label,
                                kind: c.kind,
                                documentation: c.documentation,
                                detail: c.detail,
                                insertText: c.insertText,
                                range: range
                            }};
                        }})
                    }};
                }},
                triggerCharacters: ['-', '$', '.', '::']
            }});
            
            // Add keyboard shortcuts
            editor.addCommand(monaco.KeyCode.F5, function() {{
                window.chrome.webview.postMessage({{ action: 'run' }});
            }});
            
            editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, function() {{
                window.chrome.webview.postMessage({{ action: 'save' }});
            }});
            
            editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyN, function() {{
                window.chrome.webview.postMessage({{ action: 'new' }});
            }});
            
            // Notify ready
            window.chrome.webview.postMessage({{ action: 'ready' }});
        }});
        
        // Handle get content requests
        function getContent() {{
            return editor.getValue();
        }}
        
        function setContent(content) {{
            editor.setValue(content);
        }}
    </script>
</body>
</html>";

            return html.Replace("__DEFAULT_SCRIPT_CONTENT__", defaultContent);
        }

        private async void EditorWebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string message = e.WebMessageAsJson;

                // Parse JSON message for completion requests
                if (message.Contains("\"action\":\"completion\""))
                {
                    await HandleCompletionRequestAsync(message, sender as WebView2);
                }
                // Parse JSON message (simple parsing for action)
                else if (message.Contains("\"action\":\"run\""))
                {
                    RunScriptRequested?.Invoke(this, EventArgs.Empty);
                }
                else if (message.Contains("\"action\":\"save\""))
                {
                    _ = SaveCurrentScript();
                }
                else if (message.Contains("\"action\":\"new\""))
                {
                    CreateNewScriptTab();
                }
                else if (message.Contains("\"action\":\"ready\""))
                {
                    // Find the webView and set visible if needed
                    foreach (var kvp in tabData)
                    {
                        if (kvp.Value.webView == sender)
                        {
                            kvp.Value.webView.Visible = true;
                            break;
                        }
                    }

                    // Signal that the WebView is ready
                    WebView2 senderWebView = sender as WebView2;
                    if (_webViewReadyTasks.ContainsKey(senderWebView))
                    {
                        _webViewReadyTasks[senderWebView].TrySetResult(true);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling web message: {ex.Message}");
            }
        }

        private async Task HandleCompletionRequestAsync(string message, WebView2 senderWebView)
        {
            try
            {
                // Parse the JSON message to extract script and cursor position
                var jsonDoc = JsonDocument.Parse(message);
                var root = jsonDoc.RootElement;

                string requestId = root.GetProperty("requestId").GetString();
                string script = root.GetProperty("script").GetString();
                int cursorPosition = root.GetProperty("cursorPosition").GetInt32();

                // Get completions from the service
                List<CompletionItem> completions = new List<CompletionItem>();
                if (_completionService != null)
                {
                    completions = await _completionService.GetCompletionsAsync(script, cursorPosition);
                }

                // Convert to Monaco format
                var monacoCompletions = completions.Select(c => new
                {
                    label = c.ListItemText ?? c.CompletionText,
                    insertText = c.CompletionText,
                    kind = MapCompletionTypeToMonacoKind(c.ResultType),
                    documentation = c.ToolTip,
                    detail = GetCompletionDetail(c.ResultType)
                }).ToList();

                // Send response back to Monaco
                var response = new
                {
                    action = "completionResponse",
                    requestId = requestId,
                    completions = monacoCompletions
                };

                string responseJson = JsonSerializer.Serialize(response);
                await senderWebView.CoreWebView2.ExecuteScriptAsync(
                    $"window.handleCompletionResponse({responseJson})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling completion request: {ex.Message}");
            }
        }

        private int MapCompletionTypeToMonacoKind(CompletionResultType resultType)
        {
            // Map PowerShell CompletionResultType to Monaco CompletionItemKind
            // Monaco kinds: Method=0, Function=1, Constructor=2, Field=3, Variable=4, 
            //               Class=5, Struct=6, Interface=7, Module=8, Property=9, 
            //               Event=10, Operator=11, Unit=12, Value=13, Constant=14, 
            //               Enum=15, EnumMember=16, Keyword=17, Text=18, Color=19, 
            //               File=20, Reference=21, Customcolor=22, Folder=23, 
            //               TypeParameter=24, User=25, Issue=26, Snippet=27

            switch (resultType)
            {
                case CompletionResultType.Command:
                    return 1; // Function
                case CompletionResultType.Method:
                    return 0; // Method
                case CompletionResultType.Property:
                    return 9; // Property
                case CompletionResultType.Variable:
                    return 4; // Variable
                case CompletionResultType.ParameterName:
                    return 9; // Property
                case CompletionResultType.ParameterValue:
                    return 13; // Value
                case CompletionResultType.Type:
                    return 5; // Class
                case CompletionResultType.Namespace:
                    return 8; // Module
                case CompletionResultType.Keyword:
                    return 17; // Keyword
                case CompletionResultType.Text:
                default:
                    return 18; // Text
            }
        }

        private string GetCompletionDetail(CompletionResultType resultType)
        {
            switch (resultType)
            {
                case CompletionResultType.Command:
                    return "Command";
                case CompletionResultType.Method:
                    return "Method";
                case CompletionResultType.Property:
                    return "Property";
                case CompletionResultType.Variable:
                    return "Variable";
                case CompletionResultType.ParameterName:
                    return "Parameter";
                case CompletionResultType.ParameterValue:
                    return "Value";
                case CompletionResultType.Type:
                    return "Type";
                case CompletionResultType.Namespace:
                    return "Namespace";
                case CompletionResultType.Keyword:
                    return "Keyword";
                default:
                    return "";
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

            foreach (var data in tabData.Values)
            {
                try
                {
                    data.webView?.Dispose();
                }
                catch
                {
                    // Ignore errors when disposing
                }
            }
            tabData.Clear();
            _webViewReadyTasks.Clear();
        }

        private string GetDefaultScriptContent()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.DefaultScript.ps1"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
