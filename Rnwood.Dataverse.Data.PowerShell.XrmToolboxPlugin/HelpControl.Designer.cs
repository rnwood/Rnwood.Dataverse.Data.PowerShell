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
            this.toolStrip = new ToolStrip();
            this.homeButton = new ToolStripButton();
            this.backButton = new ToolStripButton();
            this.forwardButton = new ToolStripButton();
            this.searchCombo = new ToolStripComboBox();
            this.helpWebView2 = new WebView2();
            this.SuspendLayout();

            // toolStrip
            this.toolStrip.Items.AddRange(new ToolStripItem[] {
                this.homeButton,
                this.backButton,
                this.forwardButton,
                this.searchCombo});
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip";
            this.toolStrip.Dock = DockStyle.Top;

            // homeButton
            this.homeButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.homeButton.Name = "homeButton";
            this.homeButton.Size = new System.Drawing.Size(23, 22);
            this.homeButton.Text = "Home";
            this.homeButton.ToolTipText = "Home";
            this.homeButton.Click += new EventHandler(this.HomeButton_Click);

            // backButton
            this.backButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.backButton.Enabled = false;
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(23, 22);
            this.backButton.Text = "Back";
            this.backButton.ToolTipText = "Back";
            this.backButton.Click += new EventHandler(this.BackButton_Click);

            // forwardButton
            this.forwardButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.forwardButton.Enabled = false;
            this.forwardButton.Name = "forwardButton";
            this.forwardButton.Size = new System.Drawing.Size(23, 22);
            this.forwardButton.Text = "Forward";
            this.forwardButton.ToolTipText = "Forward";
            this.forwardButton.Click += new EventHandler(this.ForwardButton_Click);

            // searchCombo
            this.searchCombo.Name = "searchCombo";
            this.searchCombo.Size = new System.Drawing.Size(300, 23);
            this.searchCombo.DropDownWidth = 1000;
            this.searchCombo.ToolTipText = "Search";
            this.searchCombo.SelectedIndexChanged += new EventHandler(this.SearchCombo_SelectedIndexChanged);


            // helpWebView2
            this.helpWebView2.Dock = DockStyle.Fill;
            this.helpWebView2.Name = "helpWebView2";
            this.helpWebView2.TabIndex = 1;
            this.helpWebView2.NavigationCompleted += new EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>(this.HelpWebView2_NavigationCompleted);

            this.Controls.Add(this.helpWebView2);
            this.Controls.Add(this.toolStrip);
            this.Name = "HelpControl";
            this.Size = new System.Drawing.Size(400, 600);
            this.Padding = new Padding(0);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip;
        private ToolStripButton homeButton;
        private ToolStripButton backButton;
        private ToolStripButton forwardButton;
        private ToolStripComboBox searchCombo;
        private WebView2 helpWebView2;
    }
}
