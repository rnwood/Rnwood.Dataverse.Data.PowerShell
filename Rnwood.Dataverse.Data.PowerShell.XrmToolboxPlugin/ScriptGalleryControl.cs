using System;
using System.Windows.Forms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class ScriptGalleryControl : UserControl
    {
        private Label label;

        public ScriptGalleryControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.label = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // label
            this.label.AutoSize = false;
            this.label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label.Location = new System.Drawing.Point(0, 0);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(516, 1044);
            this.label.TabIndex = 0;
            this.label.Text = "Coming soon - community-driven script gallery";
            this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.Controls.Add(this.label);
            this.Name = "ScriptGalleryControl";
            this.Size = new System.Drawing.Size(516, 1044);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ResumeLayout(false);
        }
    }
}