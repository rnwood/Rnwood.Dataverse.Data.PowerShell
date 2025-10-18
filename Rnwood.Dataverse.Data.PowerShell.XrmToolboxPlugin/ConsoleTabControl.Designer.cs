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
            this.conEmuControl = new ConEmuControl();
            this.closeButton = new Button();
            this.SuspendLayout();
            // 
            // conEmuControl
            // 
            this.conEmuControl.Dock = DockStyle.Fill;
            this.conEmuControl.Name = "conEmuControl";
            // 
            // closeButton
            // 
            this.closeButton.Text = "X";
            this.closeButton.Size = new Size(20, 20);
            this.closeButton.Location = new Point(this.Width - 25, 5);
            this.closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.closeButton.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);
            // 
            // ConsoleTabControl
            // 
            this.Controls.Add(this.conEmuControl);
            this.Controls.Add(this.closeButton);
            this.closeButton.BringToFront();
            this.Name = "ConsoleTabControl";
            this.Size = new Size(800, 600);
            this.ResumeLayout(false);
        }

        #endregion

        private ConEmuControl conEmuControl;
        private Button closeButton;
    }
}