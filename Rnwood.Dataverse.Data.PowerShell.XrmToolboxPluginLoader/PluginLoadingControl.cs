using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.IO.Compression;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPluginLoader
{
    public partial class PluginLoadingControl : PluginControlBase, IGitHubPlugin, IPayPalPlugin
    {
        private Panel mainPanel;
        private ProgressBar progressBar;
        private Label statusLabel;
        private PluginControlBase realControl;
        private PictureBox pictureBox1;
        private string pluginSubdir;

        public PluginLoadingControl()
        {
            InitializeComponent();

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartLoading();
        }

        private void InitializeComponent()
        {
            this.mainPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.statusLabel = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.pictureBox1);
            this.mainPanel.Controls.Add(this.statusLabel);
            this.mainPanel.Controls.Add(this.progressBar);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(428, 198);
            this.mainPanel.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBox1.Image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Rnwood.Dataverse.Data.PowerShell.XrmToolboxPluginLoader.logo.png"));
            this.pictureBox1.Location = new System.Drawing.Point(12, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(397, 103);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(12, 122);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(212, 13);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "Loading PowerShell Scripting Workspace...";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.progressBar.Location = new System.Drawing.Point(12, 152);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(400, 22);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 1;
            // 
            // PluginLoadingControl
            // 
            this.Controls.Add(this.mainPanel);
            this.Name = "PluginLoadingControl";
            this.Size = new System.Drawing.Size(428, 198);
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        private void StartLoading()
        {
            Task.Run(() =>
            {
                try
                {
                    string pluginsDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    pluginSubdir = Path.Combine(pluginsDir, "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPluginLoader");


                    // Look for an o.zip in the plugin folder; if present, extract to per-user LocalAppData and use that path to avoid long path name issues
                    var zipPath = Path.Combine(pluginSubdir, "o.zip");

                    // In debug builds, allow running directly from build output without needing the zip

#if !DEBUG
                    pluginSubdir = EnsureUnzipped(zipPath, "net48");       
#endif


                    // Set up assembly resolve
                    AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                    {
                        string assemblyName = new AssemblyName(args.Name).Name;
                        string dllPath = Path.Combine(pluginSubdir, assemblyName + ".dll");

                        if (File.Exists(dllPath))
                        {
                            return Assembly.LoadFrom(dllPath);
                        }

                        return null;
                    };

                    // Load the real control assembly and type
                    var assembly = Assembly.LoadFrom(Path.Combine(pluginSubdir, "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.dll"));
                    var type = assembly.GetType("Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.MainControl");

                    //Ensure we were visible for at least 2 seconds
                    System.Threading.Thread.Sleep(2000);

                    // Update UI on UI thread
                    this.Invoke(new Action(() =>
                    {
                        realControl = (PluginControlBase)Activator.CreateInstance(type);

                        mainPanel.Controls.Clear();
                        realControl.Dock = DockStyle.Fill;
                        mainPanel.Controls.Add(realControl);

                        // Forward any pending connection updates
                        if (Service != null)
                        {
                            var method = realControl.GetType().GetMethod("OnConnectionUpdated", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (method != null)
                            {
                                method.Invoke(realControl, new object[] { new ConnectionUpdatedEventArgs(Service, null) });
                            }
                        }
                    }));
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"Failed to load plugin: {ex.Message}", "Plugin Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            });
        }

        private static string EnsureUnzipped(string zipPath, string targetFramework)
        {
            var localBase = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Rnwood.Dataverse.Data.PowerShell", "Plugins", targetFramework);
            var stampFilePath = Path.Combine(localBase, "assemblyversion.stamp");

            // Compute expected stamp from zip entry (prefer an 'assemblyversion.stamp' file inside the zip)
            string expectedStamp = null;
            try
            {
                using (var z = ZipFile.OpenRead(zipPath))
                {
                    var stampEntry = z.Entries.FirstOrDefault(e => e.Name.Equals("assemblyversion.stamp", StringComparison.OrdinalIgnoreCase));

                    using (var sr = new StreamReader(stampEntry.Open()))
                    {
                        expectedStamp = sr.ReadToEnd().Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read version from plugin zip: {ex.Message}", "Plugin Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            if (string.IsNullOrEmpty(expectedStamp))
            {
                MessageBox.Show("Could not determine plugin version from zip file.", "Plugin Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new InvalidOperationException("Could not determine plugin version from zip file.");
            }

            // Check if already extracted and stamp matches; treat absence of stamp as non-matching
            bool isValid = false;
            if (Directory.Exists(localBase) && File.Exists(stampFilePath))
            {
                try
                {
                    var existing = File.ReadAllText(stampFilePath).Trim();
                    if (existing == expectedStamp)
                    {
                        isValid = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to read existing plugin stamp: {ex.Message}", "Plugin Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }

            if (!isValid)
            {
                // Stamp mismatch or absent - delete folder if exists
                if (Directory.Exists(localBase))
                {
                    try { Directory.Delete(localBase, true); } catch { }
                }

                // Extract to temp dir atomically
                var tmpDir = Path.Combine(Path.GetTempPath(), "Rnwood.Dataverse.Data.PowerShell", Guid.NewGuid().ToString());
                try
                {
                    Directory.CreateDirectory(tmpDir);
                    ZipFile.ExtractToDirectory(zipPath, tmpDir);
                    // Ensure parent directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(localBase));
                    // Move temp to final (atomic rename)
                    Directory.Move(tmpDir, localBase);
                    File.WriteAllText(stampFilePath, expectedStamp);
                }
                catch (Exception ex)
                {
                    try { Directory.Delete(tmpDir, true); } catch { }
                    MessageBox.Show($"Failed to extract plugin files: {ex.Message}", "Plugin Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }

            return localBase;
        }

        // Forward interface properties
        public string RepositoryName => "Rnwood.Dataverse.Data.PowerShell";
        public string UserName => "rnwood";
        public string DonationDescription => "Support development of this PowerShell module";
        public string EmailAccount => "rob@rnwood.co.uk";

        // Forward virtual methods
        protected override void OnConnectionUpdated(ConnectionUpdatedEventArgs e)
        {
            base.OnConnectionUpdated(e);
            if (realControl != null)
            {
                var method = realControl.GetType().GetMethod("OnConnectionUpdated", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(realControl, new object[] { e });
                }
            }
        }

        public override void ClosingPlugin(PluginCloseInfo info)
        {
            if (realControl != null)
            {
                realControl.ClosingPlugin(info);
            }
            base.ClosingPlugin(info);
        }
    }
}