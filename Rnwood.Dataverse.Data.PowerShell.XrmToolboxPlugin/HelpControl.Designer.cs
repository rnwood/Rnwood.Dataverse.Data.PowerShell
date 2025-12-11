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
    partial class HelpControl
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
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.homeButton = new System.Windows.Forms.ToolStripButton();
            this.backButton = new System.Windows.Forms.ToolStripButton();
            this.forwardButton = new System.Windows.Forms.ToolStripButton();
            this.autoHelpButton = new System.Windows.Forms.ToolStripButton();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.searchCombo = new System.Windows.Forms.ToolStripComboBox();
            this.anchorsCombo = new System.Windows.Forms.ToolStripComboBox();
            this.comboToolStrip = new System.Windows.Forms.ToolStrip();
            this.helpWebView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.toolStrip.SuspendLayout();
            this.comboToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.helpWebView2)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.homeButton,
            this.backButton,
            this.forwardButton,
            this.autoHelpButton,
            this.progressBar});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(400, 25);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip";
            // 
            // homeButton
            // 
            this.homeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.homeButton.Image = global::Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.Properties.Resources.mdi__home;
            this.homeButton.Name = "homeButton";
            this.homeButton.Size = new System.Drawing.Size(23, 22);
            this.homeButton.Text = "Home";
            this.homeButton.ToolTipText = "Home";
            this.homeButton.Click += new System.EventHandler(this.HomeButton_Click);
            // 
            // backButton
            // 
            this.backButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.backButton.Enabled = false;
            this.backButton.Image = global::Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.Properties.Resources.mdi__arrow_back;
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(23, 22);
            this.backButton.Text = "Back";
            this.backButton.ToolTipText = "Back";
            this.backButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // forwardButton
            // 
            this.forwardButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.forwardButton.Enabled = false;
            this.forwardButton.Image = global::Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.Properties.Resources.mdi__arrow_forward;
            this.forwardButton.Name = "forwardButton";
            this.forwardButton.Size = new System.Drawing.Size(23, 22);
            this.forwardButton.Text = "Forward";
            this.forwardButton.ToolTipText = "Forward";
            this.forwardButton.Click += new System.EventHandler(this.ForwardButton_Click);
            // 
            // autoHelpButton
            // 
            this.autoHelpButton.Checked = true;
            this.autoHelpButton.CheckOnClick = true;
            this.autoHelpButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoHelpButton.Name = "autoHelpButton";
            this.autoHelpButton.Size = new System.Drawing.Size(107, 22);
            this.autoHelpButton.Text = "Auto follow editor";
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 22);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.Visible = false;
            // 
            // searchCombo
            // 
            this.searchCombo.DropDownWidth = 600;
            this.searchCombo.Name = "searchCombo";
            this.searchCombo.Size = new System.Drawing.Size(200, 27);
            this.searchCombo.ToolTipText = "Search";
            this.searchCombo.SelectedIndexChanged += new System.EventHandler(this.SearchCombo_SelectedIndexChanged);
            // 
            // anchorsCombo
            // 
            this.anchorsCombo.DropDownWidth = 600;
            this.anchorsCombo.Name = "anchorsCombo";
            this.anchorsCombo.Size = new System.Drawing.Size(200, 23);
            this.anchorsCombo.ToolTipText = "Anchors";
            this.anchorsCombo.SelectedIndexChanged += new System.EventHandler(this.AnchorsCombo_SelectedIndexChanged);
            // 
            // comboToolStrip
            // 
            this.comboToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.comboToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.searchCombo,
            this.anchorsCombo});
            this.comboToolStrip.Location = new System.Drawing.Point(0, 25);
            this.comboToolStrip.Name = "comboToolStrip";
            this.comboToolStrip.Size = new System.Drawing.Size(400, 27);
            this.comboToolStrip.TabIndex = 1;
            this.comboToolStrip.Text = "comboToolStrip";
            // 
            // helpWebView2
            // 
            this.helpWebView2.AllowExternalDrop = true;
            this.helpWebView2.CreationProperties = null;
            this.helpWebView2.DefaultBackgroundColor = System.Drawing.Color.White;
            this.helpWebView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.helpWebView2.Location = new System.Drawing.Point(0, 52);
            this.helpWebView2.Name = "helpWebView2";
            this.helpWebView2.Size = new System.Drawing.Size(400, 548);
            this.helpWebView2.TabIndex = 1;
            this.helpWebView2.ZoomFactor = 1D;
            this.helpWebView2.NavigationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>(this.HelpWebView2_NavigationCompleted);
            // 
            // HelpControl
            // 
            this.Controls.Add(this.helpWebView2);
            this.Controls.Add(this.comboToolStrip);
            this.Controls.Add(this.toolStrip);
            this.Name = "HelpControl";
            this.Size = new System.Drawing.Size(400, 600);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.comboToolStrip.ResumeLayout(false);
            this.comboToolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.helpWebView2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ToolStrip toolStrip;
        private ToolStripButton homeButton;
        private ToolStripButton backButton;
        private ToolStripButton forwardButton;
        private ToolStripButton autoHelpButton;
        private ToolStripComboBox searchCombo;
        private ToolStripComboBox anchorsCombo;
        private ToolStripProgressBar progressBar;
        private ToolStrip comboToolStrip;
        private WebView2 helpWebView2;
    }
}
