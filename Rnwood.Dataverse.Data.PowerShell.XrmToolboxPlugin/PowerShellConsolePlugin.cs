using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.Management.Automation;
using System.Text.Json;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;
using Microsoft.PowerPlatform.Dataverse.Client;
using ConEmu.WinForms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.Xrm.Tooling.Connector;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class PowerShellConsolePlugin : PluginControlBase, IGitHubPlugin, IPayPalPlugin
    {
        private SplitContainer splitContainer;
        private SplitContainer innerSplitContainer;
        private ConsoleControl consoleControl;
        private HelpControl helpControl;
        private ScriptEditorControl scriptEditorControl;
        private TabControl tabControl;
        private ScriptGalleryControl scriptGalleryControl;

        public PowerShellConsolePlugin()
        {
            InitializeComponent();
            this.Load += PowerShellConsolePlugin_Load;
        }

        private void InitializeComponent()
        {
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.innerSplitContainer = new System.Windows.Forms.SplitContainer();
            this.scriptEditorControl = new Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.ScriptEditorControl();
            this.consoleControl = new Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.ConsoleControl();
            this.helpControl = new Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.HelpControl();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.scriptGalleryControl = new Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.ScriptGalleryControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.innerSplitContainer)).BeginInit();
            this.innerSplitContainer.Panel1.SuspendLayout();
            this.innerSplitContainer.Panel2.SuspendLayout();
            this.innerSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Margin = new System.Windows.Forms.Padding(6);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.innerSplitContainer);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.tabControl);
            this.splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(18);
            this.splitContainer.Size = new System.Drawing.Size(1467, 1108);
            this.splitContainer.SplitterDistance = 900;
            this.splitContainer.SplitterWidth = 7;
            this.splitContainer.TabIndex = 0;
            // 
            // innerSplitContainer
            // 
            this.innerSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.innerSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.innerSplitContainer.Name = "innerSplitContainer";
            this.innerSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // innerSplitContainer.Panel1
            // 
            this.innerSplitContainer.Panel1.Controls.Add(this.scriptEditorControl);
            // 
            // innerSplitContainer.Panel2
            // 
            this.innerSplitContainer.Panel2.Controls.Add(this.consoleControl);
            this.innerSplitContainer.Size = new System.Drawing.Size(900, 1108);
            this.innerSplitContainer.SplitterDistance = 450;
            this.innerSplitContainer.SplitterWidth = 7;
            this.innerSplitContainer.TabIndex = 0;
            // 
            // scriptEditorControl
            // 
            this.scriptEditorControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.scriptEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptEditorControl.Location = new System.Drawing.Point(0, 0);
            this.scriptEditorControl.Margin = new System.Windows.Forms.Padding(6);
            this.scriptEditorControl.Name = "scriptEditorControl";
            this.scriptEditorControl.Size = new System.Drawing.Size(900, 450);
            this.scriptEditorControl.TabIndex = 1;
            // 
            // consoleControl
            // 
            this.consoleControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consoleControl.Location = new System.Drawing.Point(0, 0);
            this.consoleControl.Margin = new System.Windows.Forms.Padding(6);
            this.consoleControl.Name = "consoleControl";
            this.consoleControl.Size = new System.Drawing.Size(900, 651);
            this.consoleControl.TabIndex = 0;
            // 
            // helpControl
            // 
            this.helpControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.helpControl.Location = new System.Drawing.Point(18, 18);
            this.helpControl.Margin = new System.Windows.Forms.Padding(6);
            this.helpControl.Name = "helpControl";
            this.helpControl.Size = new System.Drawing.Size(524, 1072);
            this.helpControl.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(18, 18);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(524, 1072);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageHelp
            // 
            var tabPageHelp = new System.Windows.Forms.TabPage();
            tabPageHelp.Controls.Add(this.helpControl);
            tabPageHelp.Location = new System.Drawing.Point(4, 24);
            tabPageHelp.Name = "tabPageHelp";
            tabPageHelp.Padding = new System.Windows.Forms.Padding(3);
            tabPageHelp.Size = new System.Drawing.Size(516, 1044);
            tabPageHelp.TabIndex = 0;
            tabPageHelp.Text = "Help";
            tabPageHelp.UseVisualStyleBackColor = true;
            // 
            // tabPageScriptGallery
            // 
            var tabPageScriptGallery = new System.Windows.Forms.TabPage();
            tabPageScriptGallery.Location = new System.Drawing.Point(4, 24);
            tabPageScriptGallery.Name = "tabPageScriptGallery";
            tabPageScriptGallery.Padding = new System.Windows.Forms.Padding(3);
            tabPageScriptGallery.Size = new System.Drawing.Size(516, 1044);
            tabPageScriptGallery.TabIndex = 1;
            tabPageScriptGallery.Text = "Script Gallery";
            tabPageScriptGallery.UseVisualStyleBackColor = true;
            tabPageScriptGallery.Controls.Add(this.scriptGalleryControl);
            // 
            this.tabControl.Controls.Add(tabPageHelp);
            this.tabControl.Controls.Add(tabPageScriptGallery);
            this.tabControl.SuspendLayout();
            // 
            // PowerShellConsolePlugin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "PowerShellConsolePlugin";
            this.Size = new System.Drawing.Size(1467, 1108);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.innerSplitContainer.Panel1.ResumeLayout(false);
            this.innerSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.innerSplitContainer)).EndInit();
            this.innerSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        public string RepositoryName => "Rnwood.Dataverse.Data.PowerShell";
        public string UserName => "rnwood";
        public string DonationDescription => "Support development of this PowerShell module";
        public string EmailAccount => "rob@rnwood.co.uk";

        private void PowerShellConsolePlugin_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                // Delay starting the console to ensure the form is fully loaded
                Task.Delay(1000).ContinueWith(_ =>
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        consoleControl.StartEmbeddedPowerShellConsole(Service as CrmServiceClient);
                    });
                });

                scriptEditorControl.InitializeMonacoEditor();
                helpControl.LoadAndShowHelp();

                // Extract connection info for script editor
                var connectionInfo = ConsoleControl.ExtractConnectionInfo(Service as CrmServiceClient);
                if (connectionInfo != null)
                {
                    scriptEditorControl.InitializeMonacoEditor(connectionInfo.Token, connectionInfo.Url);
                }
                else
                {
                    scriptEditorControl.InitializeMonacoEditor();
                }
                helpControl.LoadAndShowHelp();

                // Set splitter to 50/50
                innerSplitContainer.SplitterDistance = innerSplitContainer.Height / 2;

                // Wire up script editor events
                scriptEditorControl.RunScriptRequested += ScriptEditorControl_RunScriptRequested;
                scriptEditorControl.NewScriptRequested += ScriptEditorControl_NewScriptRequested;
                scriptEditorControl.OpenScriptRequested += ScriptEditorControl_OpenScriptRequested;
                scriptEditorControl.SaveScriptRequested += ScriptEditorControl_SaveScriptRequested;
            }
        }

        private async void ScriptEditorControl_RunScriptRequested(object sender, EventArgs e)
        {
            try
            {
                // Get script content from Monaco editor
                string script = await scriptEditorControl.GetScriptContentAsync();

                if (string.IsNullOrWhiteSpace(script))
                {
                    MessageBox.Show("Script is empty. Please enter some PowerShell commands.",
                        "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create temporary script file
                string tempScriptPath = Path.Combine(Path.GetTempPath(), $"xrmtoolbox-script-{Guid.NewGuid()}.ps1");
                File.WriteAllText(tempScriptPath, script);

                // The ConEmu control doesn't expose direct input methods
                // Instead, we'll write the script to a file and have the user run it manually
                // Or we can use a more sophisticated approach with named pipes

                // For now, display message to user
                MessageBox.Show($"Script ready to execute.\n\nRun this command in the console:\n& '{tempScriptPath}'",
                    "Script Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Schedule cleanup after some time
                _ = Task.Delay(30000).ContinueWith(t =>
                {
                    try
                    {
                        if (File.Exists(tempScriptPath))
                        {
                            File.Delete(tempScriptPath);
                        }
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to run script: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ScriptEditorControl_NewScriptRequested(object sender, EventArgs e)
        {
            scriptEditorControl.CreateNewScript();
        }

        private void ScriptEditorControl_OpenScriptRequested(object sender, EventArgs e)
        {
            scriptEditorControl.OpenScript();
        }

        private void ScriptEditorControl_SaveScriptRequested(object sender, EventArgs e)
        {
            scriptEditorControl.SaveScript();
        }

        public override void ClosingPlugin(PluginCloseInfo info)
        {
            // Dispose completion service and other resources
            scriptEditorControl.DisposeResources();
            consoleControl.DisposeResources();

            base.ClosingPlugin(info);
        }
    }
}
