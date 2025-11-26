using System;
using System.IO;
using System.Windows.Forms;
using System.Text;
using Markdig;
using Microsoft.Web.WebView2.WinForms;
using System.Text.RegularExpressions;
using Markdig.Extensions;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class HelpControl : UserControl
    {
        private bool comboPopulated = false;
        private string pendingScrollId = null;

        private class ComboItem
        {
            public string Text { get; set; }
            public string Url { get; set; }
            public string ScrollId { get; set; }
            public override string ToString() => Text;
        }

        public HelpControl()
        {
            InitializeComponent();
            this.Load += new EventHandler(this.HelpControl_Load);
        }

        private void HomeButton_Click(object sender, EventArgs e)
        {
            LoadAndShowHelp("gettingstarted");
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            if (helpWebView2.CoreWebView2?.CanGoBack == true)
            {
                helpWebView2.CoreWebView2.GoBack();
            }
        }

        private void ForwardButton_Click(object sender, EventArgs e)
        {
            if (helpWebView2.CoreWebView2?.CanGoForward == true)
            {
                helpWebView2.CoreWebView2.GoForward();
            }
        }

        private void HelpWebView2_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            backButton.Enabled = helpWebView2.CoreWebView2?.CanGoBack == true;
            forwardButton.Enabled = helpWebView2.CoreWebView2?.CanGoForward == true;
            if (pendingScrollId != null)
            {
                helpWebView2.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{pendingScrollId}').scrollIntoView();");
                pendingScrollId = null;
            }
        }

        private void HelpControl_Load(object sender, EventArgs e)
        {
            LoadAndShowHelp("gettingstarted");
        }

        public async void LoadAndShowHelp(string contentUrl = null)
        {
            try
            {
                string helpContent;
                if (!comboPopulated)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string readmeContent = await client.GetStringAsync("https://raw.githubusercontent.com/rnwood/Rnwood.Dataverse.Data.PowerShell/main/README.md");
                        PopulateSearchCombo(readmeContent);
                        await AddCmdletsDocumentationItems();
                        comboPopulated = true;
                    }
                }

                using (HttpClient client = new HttpClient())
                {
                    if (contentUrl == "gettingstarted" || string.IsNullOrEmpty(contentUrl))
                    {
                        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                        var resourceName = "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.GettingStarted.md";
                        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            helpContent = reader.ReadToEnd();
                        }
                    }
                    else
                    {
                        string url = contentUrl ?? "https://raw.githubusercontent.com/rnwood/Rnwood.Dataverse.Data.PowerShell/main/README.md";
                        helpContent = await client.GetStringAsync(url);
                    }
                }

                // Convert markdown to HTML using Markdig with auto identifiers and table support
                var pipeline = new MarkdownPipelineBuilder().UseAutoIdentifiers().UsePipeTables().Build();
                string htmlContent = Markdown.ToHtml(helpContent, pipeline);

                // Wrap in basic HTML structure
                string html = $@"<html>
<head>
<style>
body {{
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
    font-size: 14px;
    line-height: 1.6;
    margin: 20px;
    color: #333;
    background-color: #fff;
}}
h1, h2, h3, h4, h5, h6 {{
    margin-top: 24px;
    margin-bottom: 16px;
    font-weight: 600;
    line-height: 1.25;
}}
h1 {{
    font-size: 2em;
    border-bottom: 1px solid #eaecef;
    padding-bottom: 0.3em;
}}
h2 {{
    font-size: 1.5em;
    border-bottom: 1px solid #eaecef;
    padding-bottom: 0.3em;
}}
h3 {{
    font-size: 1.25em;
}}
h4 {{
    font-size: 1em;
}}
h5 {{
    font-size: 0.875em;
}}
h6 {{
    font-size: 0.85em;
    color: #6a737d;
}}
p {{
    margin-bottom: 16px;
}}
code {{
    font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
    background-color: #f6f8fa;
    border-radius: 3px;
    padding: 0.2em 0.4em;
    font-size: 85%;
    color: #d73a49;
}}
pre {{
    background-color: #f6f8fa;
    border-radius: 3px;
    padding: 16px;
    overflow-x: auto;
    font-size: 85%;
    line-height: 1.45;
}}
pre code {{
    background: none;
    padding: 0;
    color: inherit;
}}
blockquote {{
    border-left: 4px solid #dfe2e5;
    padding: 0 16px;
    color: #6a737d;
    margin: 0 0 16px 0;
}}
table {{
    border-collapse: collapse;
    width: 100%;
    margin-bottom: 16px;
}}
th, td {{
    border: 1px solid #dfe2e5;
    padding: 6px 13px;
}}
th {{
    background-color: #f6f8fa;
    font-weight: 600;
}}
tr:nth-child(even) {{
    background-color: #f6f8fa;
}}
ul, ol {{
    padding-left: 2em;
    margin-bottom: 16px;
}}
li {{
    margin-bottom: 4px;
}}
a {{
    color: #0366d6;
    text-decoration: none;
}}
a:hover {{
    text-decoration: underline;
}}
img {{
    max-width: 100%;
    height: auto;
}}
hr {{
    border: none;
    border-top: 1px solid #eaecef;
    margin: 24px 0;
}}
</style>
</head>
<body>
{htmlContent}
</body>
</html>";

                // Ensure WebView2 is initialized
                if (helpWebView2.CoreWebView2 == null)
                {
                    await helpWebView2.EnsureCoreWebView2Async();
                }

                helpWebView2.NavigateToString(html);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load help: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (searchCombo.SelectedItem is ComboItem item && helpWebView2.CoreWebView2 != null)
            {
                pendingScrollId = item.ScrollId;
                LoadAndShowHelp(item.Url);
            }
        }

        private string GenerateId(string heading)
        {
            return Regex.Replace(heading.ToLower(), @"[^a-z0-9]+", "-").Trim('-');
        }

        private string GetPlainText(ContainerInline inline)
        {
            if (inline == null) return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (var child in inline)
            {
                if (child is LiteralInline literal)
                {
                    sb.Append(literal.Content);
                }
                else if (child is ContainerInline container)
                {
                    sb.Append(GetPlainText(container));
                }
                // Ignore other inlines for plain text extraction
            }
            return sb.ToString();
        }

        private void PopulateSearchCombo(string markdown)
        {
            searchCombo.Items.Clear();
            if (string.IsNullOrEmpty(markdown))
            {
                return;
            }
            var document = Markdown.Parse(markdown);
            foreach (var headingBlock in document.Descendants<Markdig.Syntax.HeadingBlock>())
            {
                string heading = GetPlainText(headingBlock.Inline);
                if (!string.IsNullOrEmpty(heading))
                {
                    string id = GenerateId(heading);
                    searchCombo.Items.Add(new ComboItem { Text = new string(' ', (headingBlock.Level - 1) * 2) + heading, Url = "https://raw.githubusercontent.com/rnwood/Rnwood.Dataverse.Data.PowerShell/main/README.md", ScrollId = id });
                }
            }
        }

        private async Task AddCmdletsDocumentationItems()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin");
                    var apiUrl = "https://api.github.com/repos/rnwood/Rnwood.Dataverse.Data.PowerShell/contents/Rnwood.Dataverse.Data.PowerShell/docs";
                    var response = await client.GetStringAsync(apiUrl);
                    var json = JsonDocument.Parse(response);
                    var docs = json.RootElement.EnumerateArray()
                        .Where(e => e.GetProperty("type").GetString() == "file" && e.GetProperty("name").GetString().EndsWith(".md"))
                        .Select(e => e.GetProperty("name").GetString())
                        .ToList();

                    searchCombo.Items.Add(new ComboItem { Text = "Cmdlets documentation", Url = "https://raw.githubusercontent.com/rnwood/Rnwood.Dataverse.Data.PowerShell/main/README.md", ScrollId = null });
                    foreach (var doc in docs)
                    {
                        var baseName = Path.GetFileNameWithoutExtension(doc);
                        var contentUrl = $"https://raw.githubusercontent.com/rnwood/Rnwood.Dataverse.Data.PowerShell/main/Rnwood.Dataverse.Data.PowerShell/docs/{doc}";
                        searchCombo.Items.Add(new ComboItem { Text = baseName, Url = contentUrl, ScrollId = null });
                    }
                }
            }
            catch
            {
                // Ignore errors when fetching docs list
            }
        }
    }
}