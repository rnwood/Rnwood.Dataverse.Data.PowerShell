using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    partial class ScriptTabContentControl
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
            this.panel = new System.Windows.Forms.Panel();
            this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.tabToolbar = new System.Windows.Forms.ToolStrip();
            this.saveToGalleryButton = new System.Windows.Forms.ToolStripButton();
            this.closeButton = new System.Windows.Forms.Button();
            this.runButton = new System.Windows.Forms.ToolStripButton();
            this.saveButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.powerShellVersionButton = new System.Windows.Forms.ToolStripButton();
            this.saveToGalleryButton = new System.Windows.Forms.ToolStripButton();
            this.panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
            this.tabToolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel
            // 
            this.panel.Controls.Add(this.closeButton);
            this.panel.Controls.Add(this.webView);
            this.panel.Controls.Add(this.tabToolbar);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(632, 437);
            this.panel.TabIndex = 0;
            // 
            // webView
            // 
            this.webView.AllowExternalDrop = true;
            this.webView.CreationProperties = null;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView.Location = new System.Drawing.Point(0, 25);
            this.webView.Name = "webView";
            this.webView.Size = new System.Drawing.Size(632, 412);
            this.webView.TabIndex = 0;
            this.webView.ZoomFactor = 1D;
            // 
            // tabToolbar
            // 
            this.tabToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tabToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runButton,
            this.saveButton,
            this.toolStripSeparator1,
            this.powerShellVersionButton,
            this.saveToGalleryButton});
            this.tabToolbar.Location = new System.Drawing.Point(0, 0);
            this.tabToolbar.Name = "tabToolbar";
            this.tabToolbar.Size = new System.Drawing.Size(632, 25);
            this.tabToolbar.TabIndex = 1;
            // 
            // saveToGalleryButton
            // 
            this.saveToGalleryButton.Image = global::Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.Properties.Resources.share_variant_custom;
            this.saveToGalleryButton.Name = "saveToGalleryButton";
            this.saveToGalleryButton.Size = new System.Drawing.Size(104, 22);
            this.saveToGalleryButton.Text = "Save to Gallery";
            this.saveToGalleryButton.Click += new System.EventHandler(this.SaveToGalleryButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Image = global::Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.Properties.Resources.mdi__close;
            this.closeButton.Location = new System.Drawing.Point(594, 46);
            this.closeButton.Name = "closeButton";
            this.closeButton.Padding = new System.Windows.Forms.Padding(3);
            this.closeButton.Size = new System.Drawing.Size(24, 24);
            this.closeButton.TabIndex = 2;
            this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // runButton
            // 
            this.runButton.Image = global::Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.Properties.Resources.mdi__play;
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(48, 22);
            this.runButton.Text = "Run";
            this.runButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Image = global::Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.Properties.Resources.mdi__content_save;
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(51, 22);
            this.saveButton.Text = "Save";
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 35);
            // 
            // powerShellVersionButton
            // 
            this.powerShellVersionButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.powerShellVersionButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.powerShellVersionButton.Name = "powerShellVersionButton";
            this.powerShellVersionButton.Size = new System.Drawing.Size(95, 32);
            this.powerShellVersionButton.Text = "PowerShell 5.1";
            this.powerShellVersionButton.ToolTipText = "Click to switch PowerShell version for script execution (completion always uses PowerShell 5.1)";
            this.powerShellVersionButton.Click += new System.EventHandler(this.PowerShellVersionButton_Click);
            // saveToGalleryButton
            // 
            this.saveToGalleryButton.Image = global::Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.Properties.Resources.mdi__people_group;
            this.saveToGalleryButton.Name = "saveToGalleryButton";
            this.saveToGalleryButton.Size = new System.Drawing.Size(112, 32);
            this.saveToGalleryButton.Text = "Save to Gallery";
            this.saveToGalleryButton.Click += new System.EventHandler(this.SaveToGalleryButton_Click);
            // 
            // ScriptTabContentControl
            // 
            this.Controls.Add(this.panel);
            this.Name = "ScriptTabContentControl";
            this.Size = new System.Drawing.Size(632, 437);
            this.panel.ResumeLayout(false);
            this.panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
            this.tabToolbar.ResumeLayout(false);
            this.tabToolbar.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Panel panel;
        private WebView2 webView;
        private ToolStrip tabToolbar;
        private ToolStripButton runButton;
        private ToolStripButton saveButton;
        private ToolStripButton saveToGalleryButton;
        private Button closeButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton powerShellVersionButton;
    }
}

