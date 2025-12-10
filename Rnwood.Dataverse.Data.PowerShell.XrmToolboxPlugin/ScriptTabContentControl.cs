using System;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.Collections.Generic;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
        {
    public partial class ScriptTabContentControl : UserControl
            {
        private PowerShellCompletionService _completionService;
        private TaskCompletionSource<bool> _webViewReadyTask = new TaskCompletionSource<bool>();
        private System.Threading.SynchronizationContext _syncContext;
        private string _path;
        private PowerShellVersion _powerShellVersion = PowerShellDetector.GetDefaultVersion();
        private ScriptGalleryItem _galleryItem;

        public WebView2 WebView => webView;
        public string Path         { get => _path; set => _path = value; }
        public ScriptGalleryItem GalleryItem
        {
            get => _galleryItem;
            set
            {
                _galleryItem = value;
                // Update toolbar button text based on whether this tab has an associated gallery item
                if (saveToGalleryButton != null)
                {
                    saveToGalleryButton.Text = _galleryItem != null ? "Update in Gallery" : "Save to Gallery";
                }
            }
        }

        public PowerShellCompletionService CompletionService         { get => _completionService; set => _completionService = value; }

        /// <summary>
        /// Gets or sets the PowerShell version used for this script tab.
        /// </summary>
        public PowerShellVersion PowerShellVersion
                {
            get => _powerShellVersion;
            set
                    {
                if (_powerShellVersion != value)
                        {
                    _powerShellVersion = value;
                    UpdatePowerShellVersionLabel();
                }
            }
        }

        public event EventHandler RunRequested;
        public event EventHandler SaveRequested;
        public event EventHandler CloseRequested;
        public event EventHandler SaveToGalleryRequested;
        public event EventHandler<CompletionItem> CompletionResolved;


        public ScriptTabContentControl()
        {
            _syncContext = System.Threading.SynchronizationContext.Current;
            InitializeComponent();
            closeButton.BringToFront();
            UpdatePowerShellVersionLabel();
        }

        private void UpdatePowerShellVersionLabel()
                {
            if (powerShellVersionButton != null)
                    {
                powerShellVersionButton.Text = PowerShellDetector.GetDisplayName(_powerShellVersion);
            }

        }

        // Named event handlers referenced by designer
        private void RunButton_Click(object sender, EventArgs e)
                {
            RunRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SaveButton_Click(object sender, EventArgs e)
                {
            SaveRequested?.Invoke(this, EventArgs.Empty);
        }

        private void CloseButton_Click(object sender, EventArgs e)
                {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SaveToGalleryButton_Click(object sender, EventArgs e)
                {
            SaveToGalleryRequested?.Invoke(this, EventArgs.Empty);
        }

        public async Task InitializeWebView()
                {
            _webViewReadyTask = new TaskCompletionSource<bool>();

            try
                    {
                // Initialize WebView2 directly (previously via MarkdownEditorHelper)
                await webView.EnsureCoreWebView2Async();

                string monacoPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "monaco-editor");
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "monaco.editor",
                    monacoPath,
                    CoreWebView2HostResourceAccessKind.Allow
                );

                webView.WebMessageReceived += EditorWebView_WebMessageReceived;

                string monacoHtml = GenerateMonacoEditorHtml();
                webView.NavigateToString(monacoHtml);
            }
            catch (Exception ex)
                    {
                MessageBox.Show($"Failed to initialize script editor:         {ex.Message}\n\nWebView2 Runtime may not be installed.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public async Task<string> GetScriptContentAsync()
                {
            try
                    {
                string script = await webView.ExecuteScriptAsync("getContent()");
                return JsonSerializer.Deserialize<string>(script);
            }
            catch (Exception ex)
                    {
                MessageBox.Show($"Failed to get script content:         {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        public async Task SetScriptContentAsync(string content)
                {
            try
                    {
                await _webViewReadyTask.Task;
                string encodedContent = JsonSerializer.Serialize(content);
                await webView.ExecuteScriptAsync($"setContent(        {encodedContent})");
            }
            catch (Exception ex)
                    {
                MessageBox.Show($"Failed to set script content:         {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateMonacoEditorHtml()
                {
            string defaultContent = GetDefaultScriptContent();
            string encodedDefaultContent = JsonSerializer.Serialize(defaultContent);

            string html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <style>
        body         { margin: 0; padding: 0; overflow: hidden; }
        #container         { width: 100%; height: 100vh; }

        /* make the suggest widget taller and wider so multiple items show */
        .monaco-editor .suggest-widget .monaco-list,
        .monaco-editor .suggest-widget .monaco-tree         {
          max-height: 240px !important;   /* show ~8�10 items depending on row height */
          min-width: 320px !important;    /* optional: widen the widget */
        }

        /* reduce row height to fit more items if desired */
        .monaco-editor .suggest-widget .monaco-list .monaco-list-row         {
          height: 24px !important;
          line-height: 24px !important;
        }

    </style>
</head>
<body>
    <div id=""container""></div>
    
    <script src=""https://monaco.editor/min/vs/loader.js""></script>
    <script>
        window.pendingContent = null;
        function setContent(content)         {
            if (window.editor)         {
                window.editor.setValue(content);
            } else         {
                window.pendingContent = content;
            }
        }
        function getContent()         {
            if (window.editor)         {
                return window.editor.getValue();
            } else         {
                return window.pendingContent || '';
            }
        }
        
        require.config(        { paths:         { vs: 'https://monaco.editor/min/vs' } });
        
        require(['vs/editor/editor.main'], function()         {
            // Create Monaco editor
            window.editor = monaco.editor.create(document.getElementById('container'),         {
                value: __DEFAULT_SCRIPT_CONTENT__,
                language: 'powershell',
                theme: 'vs-dark',
                automaticLayout: true,
                fontSize: 14,
                minimap:         { enabled: true },
                scrollBeyondLastLine: false,
                wordWrap: 'on',
                lineNumbers: 'on',
                folding: true,
                renderWhitespace: 'selection'
            });
            
            // Set pending content if any
            if (window.pendingContent)         {
                window.editor.setValue(window.pendingContent);
                window.pendingContent = null;
            }
            
            // Store for pending completion requests
            var pendingCompletionRequests =         {};
            
            // Handler for completion responses from C#
            window.handleCompletionResponse = function(response)         {
                if (response.requestId && pendingCompletionRequests[response.requestId])         {
                    var resolve = pendingCompletionRequests[response.requestId];
                    delete pendingCompletionRequests[response.requestId];
                    resolve(response.completions || []);
                }
            };
            
            // Register dynamic PowerShell completion provider using LSP
            monaco.languages.registerCompletionItemProvider('powershell',         {
                provideCompletionItems: async function(model, position)         {
                    var word = model.getWordUntilPosition(position);
                    var range =         {
                        startLineNumber: position.lineNumber,
                        endLineNumber: position.lineNumber,
                        startColumn: word.startColumn,
                        endColumn: word.endColumn
                    };
                    
                    // Get the full script text and cursor position
                    var script = model.getValue();
                    var cursorOffset = model.getOffsetAt(position);
                    
                    // Create a unique request ID
                    var requestId = 'completion_' + Date.now() + '_' + Math.random();
                    
                    // Send completion request to C#
                    var completionPromise = new Promise(function(resolve)         {
                        pendingCompletionRequests[requestId] = resolve;
                        window.chrome.webview.postMessage(        {
                            action: 'completion',
                            requestId: requestId,
                            script: script,
                            cursorPosition: cursorOffset
                        });
                    });
                    
                    // Wait for response with timeout
                    var timeoutPromise = new Promise(function(resolve)         {
                        setTimeout(function()         { resolve([]); }, 15000);
                    });
                    
                    var completions = await Promise.race([completionPromise, timeoutPromise]);
                    
                    // Map completions to Monaco format
                    return         {
                        suggestions: completions.map(function(c)         {
                            return         {
                                label: c.label,
                                kind: c.kind,
                                documentation: c.documentation,
                                detail: c.detail,
                                insertText: c.insertText,
                                filterText: c.filterText,
                                range: range,
                                data: c.data
                            };
                        })
                    };
                },
                resolveCompletionItem: async function(item, token)         {
                    if (!item.data) return item;
                    
                    var requestId = 'resolve_' + Date.now() + '_' + Math.random();
                    
                    var resolvePromise = new Promise(function(resolve)         {
                        pendingCompletionRequests[requestId] = resolve;
                        window.chrome.webview.postMessage(        {
                            action: 'resolveCompletion',
                            requestId: requestId,
                            item: item.data
                        });
                    });
                    
                    var timeoutPromise = new Promise(function(resolve)         {
                        setTimeout(function()         { resolve(null); }, 5000);
                    });
                    
                    var resolvedData = await Promise.race([resolvePromise, timeoutPromise]);
                    
                    if (resolvedData)         {
                        item.documentation = resolvedData.ToolTip;
                    }
                    
                    return item;
                },
                triggerCharacters: ['-', '$', '.', '::']
            });
            
            // Add keyboard shortcuts
            editor.addCommand(monaco.KeyCode.F5, function()         {
                window.chrome.webview.postMessage(        { action: 'run' });
            });
            
            editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, function()         {
                window.chrome.webview.postMessage(        { action: 'save' });
            });
            
            editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyN, function()         {
                window.chrome.webview.postMessage(        { action: 'new' });
            });
            
            // Notify ready
            window.chrome.webview.postMessage(        { action: 'ready' });
        });
        
    </script>
</body>
</html>";

            return html.Replace("__DEFAULT_SCRIPT_CONTENT__", encodedDefaultContent);
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
                else if (message.Contains("\"action\":\"resolveCompletion\""))
                        {
                    await HandleResolveCompletionRequestAsync(message, sender as WebView2);
                }
                else if (message.Contains("\"action\":\"run\""))
                        {
                    if (_syncContext != null)
                            {
                        _syncContext.Post(_ => RunRequested?.Invoke(this, EventArgs.Empty), null);
                    }
                    else
                            {
                        RunRequested?.Invoke(this, EventArgs.Empty);
                    }
                }
                else if (message.Contains("\"action\":\"save\""))
                        {
                    if (_syncContext != null)
                            {
                        _syncContext.Post(_ => SaveRequested?.Invoke(this, EventArgs.Empty), null);
                    }
                    else
                            {
                        SaveRequested?.Invoke(this, EventArgs.Empty);
                    }
                }
                else if (message.Contains("\"action\":\"new\""))
                        {
                    // Not handled in this control
                }
                else if (message.Contains("\"action\":\"ready\""))
                        {
                    if (_syncContext != null)
                            {
                        _syncContext.Post(_ =>
                                {
                            webView.Visible = true;
                            _webViewReadyTask.TrySetResult(true);
                        }, null);
                    }
                    else
                            {
                        webView.Visible = true;
                        _webViewReadyTask.TrySetResult(true);
                    }
                }
            }
            catch (Exception ex)
                    {
                System.Diagnostics.Debug.WriteLine($"Error handling web message:         {ex.Message}");
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

                var monacoCompletions = completions.Select(c =>         {
                    return new
                            {
                        label = c.ListItemText ?? c.CompletionText,
                        insertText = c.CompletionText,
                        filterText = c.CompletionText,
                        kind = MapCompletionTypeToMonacoKind(c.ResultType),
                        documentation = c.ToolTip,
                        detail = GetCompletionDetail(c.ResultType),
                        data = c
                    };
                }).ToList();

                var response = new
                        {
                    action = "completionResponse",
                    requestId = requestId,
                    completions = monacoCompletions
                };

                string responseJson = JsonSerializer.Serialize(response);
                await senderWebView.CoreWebView2.ExecuteScriptAsync(
                    $"window.handleCompletionResponse(        {responseJson})");
            }
            catch (Exception ex)
                    {
                System.Diagnostics.Debug.WriteLine($"Error handling completion request:         {ex.Message}");
            }
        }

        private async Task HandleResolveCompletionRequestAsync(string message, WebView2 senderWebView)
                {
            try
                    {
                var jsonDoc = JsonDocument.Parse(message);
                var root = jsonDoc.RootElement;

                string requestId = root.GetProperty("requestId").GetString();
                var itemElement = root.GetProperty("item");
                
                var item = JsonSerializer.Deserialize<CompletionItem>(itemElement.GetRawText());

                CompletionItem resolvedItem = item;
                if (_completionService != null)
                        {
                    resolvedItem = await _completionService.GetCompletionDetailsAsync(item);
                }

                CompletionResolved?.Invoke(this, resolvedItem);

                var response = new
                        {
                    action = "completionResponse",
                    requestId = requestId,
                    completions = resolvedItem
                };

                string responseJson = JsonSerializer.Serialize(response);
                await senderWebView.CoreWebView2.ExecuteScriptAsync(
                    $"window.handleCompletionResponse(        {responseJson})");
            }
            catch (Exception ex)
                    {
                System.Diagnostics.Debug.WriteLine($"Error handling resolve request:         {ex.Message}");
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

        private void PowerShellVersionButton_Click(object sender, EventArgs e)
                {
            // Toggle between versions
            if (_powerShellVersion == PowerShellVersion.Desktop)
                    {
                if (PowerShellDetector.IsCoreAvailable())
                        {
                    PowerShellVersion = PowerShellVersion.Core;
                }
                else
                        {
                    MessageBox.Show(PowerShellDetector.GetInstallInstructions(),
                        "PowerShell 7+ Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
                    {
                PowerShellVersion = PowerShellVersion.Desktop;
            }
        }
    }
}

