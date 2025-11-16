using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    partial class ScriptEditorControl
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
            this.components = new System.ComponentModel.Container();
            this.editorToolbar = new System.Windows.Forms.ToolStrip();
            this.toolbarImages = new System.Windows.Forms.ImageList(this.components);
            this.newScriptButton = new System.Windows.Forms.ToolStripButton();
            this.openScriptButton = new System.Windows.Forms.ToolStripButton();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.editorToolbar.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // editorToolbar
            // 
            this.editorToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.editorToolbar.ImageList = this.toolbarImages;
            this.editorToolbar.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.editorToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newScriptButton,
            this.openScriptButton});
            this.editorToolbar.Location = new System.Drawing.Point(0, 0);
            this.editorToolbar.Name = "editorToolbar";
            this.editorToolbar.Size = new System.Drawing.Size(800, 40);
            this.editorToolbar.TabIndex = 0;
            // 
            // toolbarImages
            // 
            this.toolbarImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.toolbarImages.ImageSize = new System.Drawing.Size(16, 16);
            this.toolbarImages.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // newScriptButton
            // 
            this.newScriptButton.Name = "newScriptButton";
            this.newScriptButton.Size = new System.Drawing.Size(59, 34);
            this.newScriptButton.Text = "New";
            this.newScriptButton.Click += new System.EventHandler(this.NewScriptButton_Click);
            // 
            // openScriptButton
            // 
            this.openScriptButton.Name = "openScriptButton";
            this.openScriptButton.Size = new System.Drawing.Size(68, 34);
            this.openScriptButton.Text = "Open";
            this.openScriptButton.Click += new System.EventHandler(this.OpenScriptButton_Click);
            // 
            // tabControl
            // 
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabControl.Location = new System.Drawing.Point(0, 40);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(800, 560);
            this.tabControl.TabIndex = 1;
            // 
            // ScriptEditorControl
            // 
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.editorToolbar);
            this.Name = "ScriptEditorControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.editorToolbar.ResumeLayout(false);
            this.editorToolbar.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

         }

        #endregion

        private ToolStrip editorToolbar;
        private ToolStripButton newScriptButton;
        private ToolStripButton openScriptButton;
        private TabControl tabControl;
        private ImageList toolbarImages;
    }
}