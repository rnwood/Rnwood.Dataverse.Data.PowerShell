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
            this.newScriptButton = new System.Windows.Forms.ToolStripButton();
            this.openScriptButton = new System.Windows.Forms.ToolStripButton();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.completionStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.editorToolbar.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // editorToolbar
            // 
            this.editorToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.editorToolbar.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.editorToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newScriptButton,
            this.openScriptButton});
            this.editorToolbar.Location = new System.Drawing.Point(0, 0);
            this.editorToolbar.Name = "editorToolbar";
            this.editorToolbar.Size = new System.Drawing.Size(800, 25);
            this.editorToolbar.TabIndex = 0;

            // 
            // newScriptButton
            // 
            this.newScriptButton.Name = "newScriptButton";
            this.newScriptButton.Size = new System.Drawing.Size(35, 22);
            this.newScriptButton.Text = "New";
            this.newScriptButton.Click += new System.EventHandler(this.NewScriptButton_Click);
            // 
            // openScriptButton
            // 
            this.openScriptButton.Name = "openScriptButton";
            this.openScriptButton.Size = new System.Drawing.Size(40, 22);
            this.openScriptButton.Text = "Open";
            this.openScriptButton.Click += new System.EventHandler(this.OpenScriptButton_Click);
            // 
            // tabControl
            // 
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabControl.Location = new System.Drawing.Point(0, 25);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(800, 553);
            this.tabControl.TabIndex = 1;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.completionStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 578);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(800, 22);
            this.statusStrip.TabIndex = 0;
            // 
            // completionStatusLabel
            // 
            this.completionStatusLabel.Name = "completionStatusLabel";
            this.completionStatusLabel.Size = new System.Drawing.Size(149, 17);
            this.completionStatusLabel.Text = "Completion: Not initialized";
            // 
            // ScriptEditorControl
            // 
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.editorToolbar);
            this.Name = "ScriptEditorControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.editorToolbar.ResumeLayout(false);
            this.editorToolbar.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

         }

        #endregion

        private ToolStrip editorToolbar;
        private ToolStripButton newScriptButton;
        private ToolStripButton openScriptButton;
        private TabControl tabControl;
    }
}