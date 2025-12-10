using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    partial class MainControl
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageHelp = new System.Windows.Forms.TabPage();
            this.helpControl = new Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.HelpControl();
            this.tabPageScriptGallery = new System.Windows.Forms.TabPage();
            this.scriptGalleryControl = new Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.ScriptGalleryControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.innerSplitContainer)).BeginInit();
            this.innerSplitContainer.Panel1.SuspendLayout();
            this.innerSplitContainer.Panel2.SuspendLayout();
            this.innerSplitContainer.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageHelp.SuspendLayout();
            this.tabPageScriptGallery.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.innerSplitContainer);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.tabControl);
            this.splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(10, 10, 10, 10);
            this.splitContainer.Size = new System.Drawing.Size(800, 600);
            this.splitContainer.SplitterDistance = 490;
            this.splitContainer.TabIndex = 0;
            // 
            // innerSplitContainer
            // 
            this.innerSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.innerSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.innerSplitContainer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
            this.innerSplitContainer.Size = new System.Drawing.Size(490, 600);
            this.innerSplitContainer.SplitterDistance = 243;
            this.innerSplitContainer.TabIndex = 0;
            // 
            // scriptEditorControl
            // 
            this.scriptEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptEditorControl.Location = new System.Drawing.Point(0, 0);
            this.scriptEditorControl.Name = "scriptEditorControl";
            this.scriptEditorControl.Size = new System.Drawing.Size(490, 243);
            this.scriptEditorControl.TabIndex = 1;
            // 
            // consoleControl
            // 
            this.consoleControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consoleControl.Location = new System.Drawing.Point(0, 0);
            this.consoleControl.Name = "consoleControl";
            this.consoleControl.Size = new System.Drawing.Size(490, 353);
            this.consoleControl.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageHelp);
            this.tabControl.Controls.Add(this.tabPageScriptGallery);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(10, 10);
            this.tabControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(286, 580);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageHelp
            // 
            this.tabPageHelp.Controls.Add(this.helpControl);
            this.tabPageHelp.Location = new System.Drawing.Point(4, 22);
            this.tabPageHelp.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageHelp.Name = "tabPageHelp";
            this.tabPageHelp.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageHelp.Size = new System.Drawing.Size(278, 554);
            this.tabPageHelp.TabIndex = 0;
            this.tabPageHelp.Text = "Help";
            this.tabPageHelp.UseVisualStyleBackColor = true;
            // 
            // helpControl
            // 
            this.helpControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.helpControl.Location = new System.Drawing.Point(2, 2);
            this.helpControl.Name = "helpControl";
            this.helpControl.Size = new System.Drawing.Size(274, 550);
            this.helpControl.TabIndex = 0;
            // 
            // tabPageScriptGallery
            // 
            this.tabPageScriptGallery.Controls.Add(this.scriptGalleryControl);
            this.tabPageScriptGallery.Location = new System.Drawing.Point(4, 22);
            this.tabPageScriptGallery.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageScriptGallery.Name = "tabPageScriptGallery";
            this.tabPageScriptGallery.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageScriptGallery.Size = new System.Drawing.Size(278, 554);
            this.tabPageScriptGallery.TabIndex = 1;
            this.tabPageScriptGallery.Text = "Script Gallery";
            this.tabPageScriptGallery.UseVisualStyleBackColor = true;
            // 
            // scriptGalleryControl
            // 
            this.scriptGalleryControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptGalleryControl.Location = new System.Drawing.Point(2, 2);
            this.scriptGalleryControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.scriptGalleryControl.Name = "scriptGalleryControl";
            this.scriptGalleryControl.Size = new System.Drawing.Size(274, 550);
            this.scriptGalleryControl.TabIndex = 0;
            // 
            // PowerShellConsolePlugin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "PowerShellConsolePlugin";
            this.Size = new System.Drawing.Size(800, 600);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.innerSplitContainer.Panel1.ResumeLayout(false);
            this.innerSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.innerSplitContainer)).EndInit();
            this.innerSplitContainer.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageHelp.ResumeLayout(false);
            this.tabPageScriptGallery.ResumeLayout(false);
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
        private TabPage tabPageHelp;
        private TabPage tabPageScriptGallery;
    }
}