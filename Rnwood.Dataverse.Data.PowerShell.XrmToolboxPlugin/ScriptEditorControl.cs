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
        private ToolStripButton runScriptButton;
        private ToolStripButton newScriptButton;
        private ToolStripButton openScriptButton;
        private ToolStripButton saveScriptButton;
        private WebView2 editorWebView;
        private bool isEditorView = true;
        private string currentScriptPath = null;
        private ImageList toolbarImages;

        // PowerShell completion service
        private PowerShellCompletionService _completionService;

        public event EventHandler RunScriptRequested;
        public event EventHandler NewScriptRequested;
        public event EventHandler OpenScriptRequested;
        public event EventHandler SaveScriptRequested;

        public ScriptEditorControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.editorToolbar = new ToolStrip();
            this.runScriptButton = new ToolStripButton();
            this.newScriptButton = new ToolStripButton();
            this.openScriptButton = new ToolStripButton();
            this.saveScriptButton = new ToolStripButton();
            this.editorWebView = new WebView2();
            this.toolbarImages = new ImageList();
            this.SuspendLayout();

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

            // editorToolbar
            this.editorToolbar.Dock = DockStyle.Top;
            this.editorToolbar.GripStyle = ToolStripGripStyle.Hidden;
            this.editorToolbar.ImageList = this.toolbarImages;

            // runScriptButton
            this.runScriptButton.Text = "Run (F5)";
            this.runScriptButton.ImageIndex = 0;
            this.runScriptButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.runScriptButton.Click += RunScriptButton_Click;

            // newScriptButton
            this.newScriptButton.Text = "New";
            this.newScriptButton.ImageIndex = 1;
            this.newScriptButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.newScriptButton.Click += NewScriptButton_Click;

            // openScriptButton
            this.openScriptButton.Text = "Open";
            this.openScriptButton.ImageIndex = 2;
            this.openScriptButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.openScriptButton.Click += OpenScriptButton_Click;

            // saveScriptButton
            this.saveScriptButton.Text = "Save";
            this.saveScriptButton.ImageIndex = 3;
            this.saveScriptButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.saveScriptButton.Click += SaveScriptButton_Click;

            this.editorToolbar.Items.AddRange(new ToolStripItem[] {
                this.runScriptButton,
                this.newScriptButton,
                this.openScriptButton,
                this.saveScriptButton
            });

            // editorWebView
            this.editorWebView.Dock = DockStyle.Fill;

            this.Controls.Add(this.editorWebView);
            this.Controls.Add(this.editorToolbar);

            this.Name = "ScriptEditorControl";
            this.Size = new System.Drawing.Size(800, 600);

            this.ResumeLayout(false);
        }

        public async void InitializeMonacoEditor(string accessToken = null, string url = null)
        {
            try
            {
                await editorWebView.EnsureCoreWebView2Async(null);

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

                // Load Monaco editor HTML
                string monacoHtml = GenerateMonacoEditorHtml();
                editorWebView.NavigateToString(monacoHtml);

                // Setup message handler for script operations
                editorWebView.WebMessageReceived += EditorWebView_WebMessageReceived;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize script editor: {ex.Message}\n\nWebView2 Runtime may not be installed.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void RunScriptButton_Click(object sender, EventArgs e)
        {
            RunScriptRequested?.Invoke(this, EventArgs.Empty);
        }

        private void NewScriptButton_Click(object sender, EventArgs e)
        {
            NewScriptRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OpenScriptButton_Click(object sender, EventArgs e)
        {
            OpenScriptRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SaveScriptButton_Click(object sender, EventArgs e)
        {
            SaveScriptRequested?.Invoke(this, EventArgs.Empty);
        }

        public async Task<string> GetScriptContentAsync()
        {
            try
            {
                // Get script content from Monaco editor
                string script = await editorWebView.ExecuteScriptAsync("getContent()");

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
            try
            {
                // Escape for JavaScript
                content = content.Replace("\\", "\\\\")
                               .Replace("'", "\\'")
                               .Replace("\n", "\\n")
                               .Replace("\r", "\\r")
                               .Replace("\"", "\\\"");

                await editorWebView.ExecuteScriptAsync($"setContent('{content}')");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set script content: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CreateNewScript()
        {
            try
            {
                if (MessageBox.Show("Create a new script? Any unsaved changes will be lost.",
                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SetScriptContentAsync("# PowerShell Script\\n# Type your PowerShell commands here\\n# Press F5 or click Run to execute\\n\\n");
                    currentScriptPath = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create new script: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async void OpenScript()
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
                        SetScriptContentAsync(content);
                        currentScriptPath = dialog.FileName;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open script: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async void SaveScript()
        {
            try
            {
                string scriptPath = currentScriptPath;

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
                currentScriptPath = scriptPath;

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
            // Get PowerShell cmdlets for IntelliSense
            string cmdletCompletions = GetPowerShellCmdletCompletions();

            return $@"
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
                value: '# PowerShell Script\\n# Type your PowerShell commands here\\n# Press F5 or click Run to execute\\n\\n',
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
            
            // Fallback static completions for common cmdlets (for immediate feedback)
            var staticCmdlets = {cmdletCompletions};
            monaco.languages.registerCompletionItemProvider('powershell', {{
                provideCompletionItems: function(model, position) {{
                    var word = model.getWordUntilPosition(position);
                    var range = {{
                        startLineNumber: position.lineNumber,
                        endLineNumber: position.lineNumber,
                        startColumn: word.startColumn,
                        endColumn: word.endColumn
                    }};
                    
                    return {{
                        suggestions: staticCmdlets.map(c => ({{
                            label: c.label,
                            kind: monaco.languages.CompletionItemKind.Function,
                            documentation: c.documentation,
                            insertText: c.insertText,
                            range: range,
                            sortText: '1_' + c.label  // Lower priority than LSP completions
                        }}))
                    }};
                }}
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
        }

        private string GetPowerShellCmdletCompletions()
        {
            try
            {
                StringBuilder completions = new StringBuilder("[");

                // Get cmdlets from the Cmdlets assembly
                var cmdletsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Rnwood.Dataverse.Data.PowerShell.Cmdlets");

                if (cmdletsAssembly != null)
                {
                    var cmdletTypes = cmdletsAssembly.GetTypes()
                        .Where(t => t.Name.EndsWith("Cmdlet") && !t.IsAbstract && t.IsPublic);

                    bool first = true;
                    foreach (var type in cmdletTypes)
                    {
                        try
                        {
                            var cmdletAttr = type.GetCustomAttributes(typeof(CmdletAttribute), false)
                                .FirstOrDefault() as CmdletAttribute;

                            if (cmdletAttr != null)
                            {
                                if (!first) completions.Append(",");
                                first = false;

                                string cmdletName = $"{cmdletAttr.VerbName}-{cmdletAttr.NounName}";
                                var parameters = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(p => p.GetCustomAttributes(typeof(ParameterAttribute), false).Any())
                                    .Select(p => $"-{p.Name}")
                                    .Take(5); // Limit to first 5 parameters

                                string paramList = string.Join(", ", parameters);
                                completions.Append($"{{label:'{cmdletName}',insertText:'{cmdletName} ',documentation:'Parameters: {paramList}'}}");
                            }
                        }
                        catch
                        {
                            // Skip cmdlets that cause issues
                        }
                    }
                }

                // Add common PowerShell cmdlets
                string[] commonCmdlets = new string[] {
                    "Write-host", "Write-output", "Write-verbose", "Write-warning", "Write-error",
                    "Get-variable", "Set-variable", "ForEach-object", "Where-object",
                    "Select-object", "Sort-object", "Group-object", "Measure-object",
                    "Import-module", "Get-module", "Get-command", "Get-help"
                };

                foreach (var cmdlet in commonCmdlets)
                {
                    completions.Append($",{{label:'{cmdlet}',insertText:'{cmdlet} ',documentation:'PowerShell built-in cmdlet'}}");
                }

                completions.Append("]");
                return completions.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting cmdlet completions: {ex.Message}");
                return "[]";
            }
        }

        private async void EditorWebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string message = e.WebMessageAsJson;

                // Parse JSON message for completion requests
                if (message.Contains("\"action\":\"completion\""))
                {
                    await HandleCompletionRequestAsync(message);
                }
                // Parse JSON message (simple parsing for action)
                else if (message.Contains("\"action\":\"run\""))
                {
                    RunScriptRequested?.Invoke(this, EventArgs.Empty);
                }
                else if (message.Contains("\"action\":\"save\""))
                {
                    SaveScriptRequested?.Invoke(this, EventArgs.Empty);
                }
                else if (message.Contains("\"action\":\"new\""))
                {
                    NewScriptRequested?.Invoke(this, EventArgs.Empty);
                }
                else if (message.Contains("\"action\":\"ready\""))
                {
                    this.Visible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling web message: {ex.Message}");
            }
        }

        private async Task HandleCompletionRequestAsync(string message)
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
                await editorWebView.CoreWebView2.ExecuteScriptAsync(
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
        }
    }
}