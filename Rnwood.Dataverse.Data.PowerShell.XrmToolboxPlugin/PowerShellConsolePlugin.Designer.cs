using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    partial class PowerShellConsolePlugin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
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
            scriptGalleryControl.Dock = DockStyle.Fill;
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

        #endregion

        private SplitContainer splitContainer;
        private SplitContainer innerSplitContainer;
        private ConsoleControl consoleControl;
        private HelpControl helpControl;
        private ScriptEditorControl scriptEditorControl;
        private TabControl tabControl;
        private ScriptGalleryControl scriptGalleryControl;
    }
}