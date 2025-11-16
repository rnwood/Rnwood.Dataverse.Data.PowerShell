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
        // PowerShell completion service
        private PowerShellCompletionService _completionService;

        // Tab data
        private Dictionary<TabPage, ScriptTabContentControl> tabData = new Dictionary<TabPage, ScriptTabContentControl>();
        private int untitledCounter = 1;
        private string _accessToken;
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Create initial tab
            CreateNewScriptTab();
        }

        private TabPage CreateScriptTab(string title, string path)
        {
            TabPage tabPage = new TabPage(title);

            ScriptTabContentControl content = new ScriptTabContentControl();
            content.Path = path;
            content.ToolbarImages = this.toolbarImages;
            content.CompletionService = _completionService;
            content.RunRequested += (s, e) => RunScriptRequested?.Invoke(this, EventArgs.Empty);
            content.SaveRequested += (s, e) => SaveScriptRequested?.Invoke(this, EventArgs.Empty);
            content.CloseRequested += (s, e) => {
                tabControl.TabPages.Remove(tabPage);
                if (tabData.ContainsKey(tabPage))
                {
                    tabData[tabPage].WebView?.Dispose();
                    tabData.Remove(tabPage);
                }
            };
            tabPage.Controls.Add(content);

            tabData[tabPage] = content;

            return tabPage;
        }

        private void InitializeToolbarImages()
        {
            if (this.toolbarImages == null)
            {
                this.toolbarImages = new ImageList();
            }

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
    }
}
