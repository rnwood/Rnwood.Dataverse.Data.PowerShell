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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.newInteractiveSessionButton = new System.Windows.Forms.ToolStripSplitButton();
            this.newPowerShell5SessionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newPowerShell7SessionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 25);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(800, 575);
            this.tabControl.TabIndex = 0;
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newInteractiveSessionButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(800, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "toolStrip";
            // 
            // newInteractiveSessionButton
            // 
            this.newInteractiveSessionButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newPowerShell5SessionMenuItem,
            this.newPowerShell7SessionMenuItem});
            this.newInteractiveSessionButton.Image = global::Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.Properties.Resources.mdi__terminal_line;
            this.newInteractiveSessionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newInteractiveSessionButton.Name = "newInteractiveSessionButton";
            this.newInteractiveSessionButton.Size = new System.Drawing.Size(164, 22);
            this.newInteractiveSessionButton.Text = "New Interactive Session";
            this.newInteractiveSessionButton.ButtonClick += new System.EventHandler(this.NewInteractiveSessionButton_Click);
            // 
            // newPowerShell5SessionMenuItem
            // 
            this.newPowerShell5SessionMenuItem.Name = "newPowerShell5SessionMenuItem";
            this.newPowerShell5SessionMenuItem.Size = new System.Drawing.Size(180, 22);
            this.newPowerShell5SessionMenuItem.Text = "PowerShell 5.1";
            this.newPowerShell5SessionMenuItem.Click += new System.EventHandler(this.NewPowerShell5SessionMenuItem_Click);
            // 
            // newPowerShell7SessionMenuItem
            // 
            this.newPowerShell7SessionMenuItem.Name = "newPowerShell7SessionMenuItem";
            this.newPowerShell7SessionMenuItem.Size = new System.Drawing.Size(180, 22);
            this.newPowerShell7SessionMenuItem.Text = "PowerShell 7+";
            this.newPowerShell7SessionMenuItem.Click += new System.EventHandler(this.NewPowerShell7SessionMenuItem_Click);
            // 
            // ConsoleControl
            // 
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.toolStrip);
            this.Name = "ConsoleControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TabControl tabControl;
        private ToolStrip toolStrip;
        private ToolStripSplitButton newInteractiveSessionButton;
        private ToolStripMenuItem newPowerShell5SessionMenuItem;
        private ToolStripMenuItem newPowerShell7SessionMenuItem;
    }
}