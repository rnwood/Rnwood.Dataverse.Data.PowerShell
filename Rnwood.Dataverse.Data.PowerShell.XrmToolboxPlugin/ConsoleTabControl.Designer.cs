using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConEmu.WinForms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    partial class ConsoleTabControl
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
            this.conEmuControl = new ConEmu.WinForms.ConEmuControl();
            this.closeButton = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.orgNameLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.urlLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // conEmuControl
            // 
            this.conEmuControl.AutoStartInfo = null;
            this.conEmuControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.conEmuControl.IsStatusbarVisible = true;
            this.conEmuControl.Location = new System.Drawing.Point(0, 0);
            this.conEmuControl.MinimumSize = new System.Drawing.Size(1, 1);
            this.conEmuControl.Name = "conEmuControl";
            this.conEmuControl.Size = new System.Drawing.Size(800, 600);
            this.conEmuControl.TabIndex = 0;
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Image = global::Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.Properties.Resources.mdi__close;
            this.closeButton.Location = new System.Drawing.Point(766, 8);
            this.closeButton.Name = "closeButton";
            this.closeButton.Padding = new System.Windows.Forms.Padding(3);
            this.closeButton.Size = new System.Drawing.Size(24, 24);
            this.closeButton.TabIndex = 1;
            this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.orgNameLabel,
            this.urlLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 578);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(800, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 2;
            // 
            // orgNameLabel
            // 
            this.orgNameLabel.Name = "orgNameLabel";
            this.orgNameLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // urlLabel
            // 
            this.urlLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.urlLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.urlLabel.Name = "urlLabel";
            this.urlLabel.Size = new System.Drawing.Size(4, 17);
            // 
            // ConsoleTabControl
            // 
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.conEmuControl);
            this.Controls.Add(this.statusStrip);
            this.Name = "ConsoleTabControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ConEmuControl conEmuControl;
        private Button closeButton;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel orgNameLabel;
        private ToolStripStatusLabel urlLabel;
    }
}