using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    partial class ConsoleControl
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
            this.tabControl = new TabControl();
            this.toolStrip = new ToolStrip();
            this.newInteractiveSessionButton = new ToolStripButton();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newInteractiveSessionButton});
            this.toolStrip.Dock = DockStyle.Top;
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "toolStrip";
            // 
            // newInteractiveSessionButton
            // 
            this.newInteractiveSessionButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.newInteractiveSessionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newInteractiveSessionButton.Name = "newInteractiveSessionButton";
            this.newInteractiveSessionButton.Size = new System.Drawing.Size(123, 22);
            this.newInteractiveSessionButton.Text = "New Interactive Session";
            this.newInteractiveSessionButton.Click += new System.EventHandler(this.NewInteractiveSessionButton_Click);
            // 
            // tabControl
            // 
            this.tabControl.Dock = DockStyle.Fill;
            this.tabControl.Name = "tabControl";
            this.tabControl.TabIndex = 0;
            // 
            // ConsoleControl
            // 
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.toolStrip);
            this.Name = "ConsoleControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TabControl tabControl;
        private ToolStrip toolStrip;
        private ToolStripButton newInteractiveSessionButton;
    }
}