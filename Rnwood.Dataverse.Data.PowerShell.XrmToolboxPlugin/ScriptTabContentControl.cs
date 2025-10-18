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
    public partial class ScriptTabContentControl : UserControl
    {
        private PowerShellCompletionService _completionService;
        private TaskCompletionSource<bool> _webViewReadyTask = new TaskCompletionSource<bool>();
        private string _path;

        public WebView2 WebView => webView;
        public string Path { get => _path; set => _path = value; }
        public ImageList ToolbarImages { get => toolbarImages; set { toolbarImages = value; UpdateToolbarImages(); } }
        public PowerShellCompletionService CompletionService { get => _completionService; set => _completionService = value; }

        public event EventHandler RunRequested;
        public event EventHandler SaveRequested;
        public event EventHandler CloseRequested;

        public ScriptTabContentControl()
        {
            InitializeComponent();
        }

        private void UpdateToolbarImages()
        {
            if (toolbarImages != null)
            {
                tabToolbar.ImageList = toolbarImages;
            }
        }

        public async Task InitializeWebView()
        {
            _webViewReadyTask = new TaskCompletionSource<bool>();

            try
            {
                await webView.EnsureCoreWebView2Async(null);

                string monacoHtml = GenerateMonacoEditorHtml();
                webView.NavigateToString(monacoHtml);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize script editor: {ex.Message}\n\nWebView2 Runtime may not be installed.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            var readyTask = _webViewReadyTask.Task;
            var timeoutTask = Task.Delay(30000);
            var completedTask = await Task.WhenAny(readyTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                MessageBox.Show("WebView2 initialization timed out. The editor may load later.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public async Task<string> GetScriptContentAsync()
        {
            try
            {
                string script = await webView.ExecuteScriptAsync("getContent()");

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
        window.pendingContent = null;
        function setContent(content) {
            if (window.editor) {
                window.editor.setValue(content);
            } else {
                window.pendingContent = content;
            }
        }
        function getContent() {
            if (window.editor) {
                return window.editor.getValue();
            } else {
                return window.pendingContent || '';
            }
        }
        
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
            
            // Set pending content if any
            if (window.pendingContent) {{
                window.editor.setValue(window.pendingContent);
                window.pendingContent = null;
            }}
            
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
        
    </script>
</body>
</html>";

            return html.Replace("__DEFAULT_SCRIPT_CONTENT__", defaultContent);
        }

        private string GetDefaultScriptContent()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.DefaultScript.ps1"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private async void EditorWebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string message = e.WebMessageAsJson;

                if (message.Contains("\"action\":\"completion\""))
                {
                    await HandleCompletionRequestAsync(message, sender as WebView2);
                }
                else if (message.Contains("\"action\":\"run\""))
                {
                    RunRequested?.Invoke(this, EventArgs.Empty);
                }
                else if (message.Contains("\"action\":\"save\""))
                {
                    SaveRequested?.Invoke(this, EventArgs.Empty);
                }
                else if (message.Contains("\"action\":\"new\""))
                {
                    // Not handled in this control
                }
                else if (message.Contains("\"action\":\"ready\""))
                {
                    webView.Visible = true;
                    _webViewReadyTask.TrySetResult(true);
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
                var jsonDoc = JsonDocument.Parse(message);
                var root = jsonDoc.RootElement;

                string requestId = root.GetProperty("requestId").GetString();
                string script = root.GetProperty("script").GetString();
                int cursorPosition = root.GetProperty("cursorPosition").GetInt32();

                List<CompletionItem> completions = new List<CompletionItem>();
                if (_completionService != null)
                {
                    completions = await _completionService.GetCompletionsAsync(script, cursorPosition);
                }

                var monacoCompletions = completions.Select(c => new
                {
                    label = c.ListItemText ?? c.CompletionText,
                    insertText = c.CompletionText,
                    kind = MapCompletionTypeToMonacoKind(c.ResultType),
                    documentation = c.ToolTip,
                    detail = GetCompletionDetail(c.ResultType)
                }).ToList();

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
    }
}