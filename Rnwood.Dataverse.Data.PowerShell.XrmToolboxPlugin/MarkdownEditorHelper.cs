using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public static class MarkdownEditorHelper
    {
        private static CoreWebView2Environment _env;
        private static Exception _initializationException;

        /// <summary>
        /// Pre-initializes the WebView2 environment. Call this before showing modal dialogs.
        /// </summary>
        public static async Task PreInitializeEnvironmentAsync()
        {
            if (_env != null)
            {
                return;
            }

            try
            {
                var userDataFolder = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                    "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin",
                    "WebView2");

                // Ensure directory exists
                if (!System.IO.Directory.Exists(userDataFolder))
                {
                    System.IO.Directory.CreateDirectory(userDataFolder);
                }

                _env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            }
            catch (Exception ex)
            {
                _initializationException = ex;
                throw new InvalidOperationException(
                    "Failed to initialize WebView2. Please ensure the WebView2 Runtime is installed. " +
                    "You can download it from: https://developer.microsoft.com/microsoft-edge/webview2/", ex);
            }
        }

        // Intentionally simple environment creation: we don't attempt to be smart about
        // available browser version. CreateAsync will surface any errors clearly.

        /// <summary>
        /// Ensures WebView2 is initialized. Safe to call from async methods.
        /// For modal dialogs, call PreInitializeEnvironmentAsync() first.
        /// </summary>
        public static async Task EnsureWebViewInitializedAsync(WebView2 webView)
        {
            if (_initializationException != null)
            {
                throw new InvalidOperationException(
                    "WebView2 initialization previously failed. Please restart the application and ensure the WebView2 Runtime is installed.",
                    _initializationException);
            }
            
            await PreInitializeEnvironmentAsync();
            // Ensure EnsureCoreWebView2Async is invoked on the UI thread for the WebView control
            try
            {
                if (webView.InvokeRequired)
                {
                    // Use Invoke to call the async method on the UI thread and obtain the returned Task to await
                    var returned = webView.Invoke(new Func<Task>(() => webView.EnsureCoreWebView2Async(_env)));
                    if (returned is Task task)
                    {
                        await task;
                    }
                    else
                    {
                        // Fallback
                        await webView.EnsureCoreWebView2Async(_env);
                    }
                }
                else
                {
                    await webView.EnsureCoreWebView2Async(_env);
                }
            }
            catch (Exception ex)
            {
                _initializationException = ex;
                System.Diagnostics.Debug.WriteLine($"EnsureCoreWebView2Async failed: {ex}");
                throw new InvalidOperationException("Failed to initialize WebView2 control. Ensure WebView2 Runtime is installed and accessible.", ex);
            }

        }

        public static string GenerateEditorHtml(string content)
        {
            string encodedContent = JsonSerializer.Serialize(content ?? "");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <link rel=""stylesheet"" href=""https://uicdn.toast.com/editor/latest/toastui-editor.min.css"" />
    <style>
        body {{ margin: 0; padding: 0; overflow: hidden; }}
        #editor {{ height: 100vh; }}
    </style>
</head>
<body>
    <div id=""editor""></div>
    <script src=""https://uicdn.toast.com/editor/latest/toastui-editor-all.min.js""></script>
    <script>
        const Editor = toastui.Editor;
        const editor = new Editor({{
            el: document.querySelector('#editor'),
            height: '100%',
            initialEditType: 'wysiwyg',
            previewStyle: 'vertical',
            initialValue: {encodedContent},
            toolbarItems: [
                ['heading', 'bold', 'italic', 'strike'],
                ['hr', 'quote'],
                ['ul', 'ol', 'task', 'indent', 'outdent'],
                ['table', 'image', 'link'],
                ['code', 'codeblock'],
                ['scrollSync']
            ]
        }});

        // Add custom button for PowerShell code block if needed, 
        // but 'codeblock' item in toolbarItems already supports languages.
        // We can customize it to default to powershell or add a specific button.
        
        // Let's add a specific button for PowerShell
        const toolbar = editor.getUI().getToolbar();
        
        editor.addCommand('markdown', 'insertPowerShell', () => {{
            const cm = editor.getMdEditor();
            const doc = cm.getDoc();
            const cursor = doc.getCursor();
            doc.replaceRange('\n```powershell\n\n```\n', cursor);
            doc.setCursor(cursor.line + 2, 0);
        }});

        editor.addCommand('wysiwyg', 'insertPowerShell', () => {{
            // WYSIWYG command implementation is more complex, 
            // usually relying on the internal model.
            // For simplicity, we might rely on the standard codeblock button
            // or switch to markdown mode temporarily.
            // Toast UI Editor handles code blocks well in WYSIWYG.
            editor.exec('codeBlock', {{ language: 'powershell' }});
        }});

        toolbar.insertItem(20, {{
            type: 'button',
            options: {{
                el: createButton(),
                command: 'insertPowerShell',
                tooltip: 'Insert PowerShell Code Block'
            }}
        }});

        function createButton() {{
            const button = document.createElement('button');
            button.className = 'toastui-editor-toolbar-icons last';
            button.style.backgroundImage = 'none';
            button.style.margin = '0';
            button.innerHTML = 'PS';
            button.style.fontWeight = 'bold';
            button.style.color = '#333';
            return button;
        }}

        // Notify ready
        window.chrome.webview.postMessage({{ action: 'ready' }});

        function getContent() {{
            return editor.getMarkdown();
        }}
    </script>
</body>
</html>";
        }
    }
}
