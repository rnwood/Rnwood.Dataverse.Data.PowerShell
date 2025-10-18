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
            this.panel = new Panel();
            this.panel.Dock = DockStyle.Fill;
            this.Controls.Add(panel);

            this.webView = new WebView2();
            this.webView.Dock = DockStyle.Fill;
            this.panel.Controls.Add(webView);

            this.tabToolbar = new ToolStrip();
            this.tabToolbar.ImageList = toolbarImages;
            this.tabToolbar.GripStyle = ToolStripGripStyle.Hidden;
            this.tabToolbar.Dock = DockStyle.Top;

            this.runButton = new ToolStripButton();
            this.runButton.Text = "Run (F5)";
            this.runButton.ImageIndex = 0;
            this.runButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.runButton.Click += (s, e) => RunRequested?.Invoke(this, EventArgs.Empty);

            this.saveButton = new ToolStripButton();
            this.saveButton.Text = "Save";
            this.saveButton.ImageIndex = 3;
            this.saveButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.saveButton.Click += (s, e) => SaveRequested?.Invoke(this, EventArgs.Empty);

            this.tabToolbar.Items.AddRange(new ToolStripItem[] { runButton, saveButton });

            this.panel.Controls.Add(tabToolbar);

            this.closeButton = new Button();
            this.closeButton.Text = "X";
            this.closeButton.Size = new Size(20, 20);
            this.closeButton.Location = new Point(panel.Width - 25, tabToolbar.Height + 5);
            this.closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.closeButton.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);
            this.panel.Controls.Add(closeButton);
            this.closeButton.BringToFront();

            this.webView.WebMessageReceived += EditorWebView_WebMessageReceived;
        }

        #endregion

        private Panel panel;
        private WebView2 webView;
        private ToolStrip tabToolbar;
        private ToolStripButton runButton;
        private ToolStripButton saveButton;
        private Button closeButton;
        private ImageList toolbarImages;
    }
}