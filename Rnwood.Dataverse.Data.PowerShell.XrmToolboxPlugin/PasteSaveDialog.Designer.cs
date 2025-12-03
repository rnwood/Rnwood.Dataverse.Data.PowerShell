namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    partial class PasteSaveDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.titleLabel = new System.Windows.Forms.Label();
            this.titleTextBox = new System.Windows.Forms.TextBox();
            this.apiKeyLabel = new System.Windows.Forms.Label();
            this.apiKeyTextBox = new System.Windows.Forms.TextBox();
            this.visibilityLabel = new System.Windows.Forms.Label();
            this.publicRadioButton = new System.Windows.Forms.RadioButton();
            this.unlistedRadioButton = new System.Windows.Forms.RadioButton();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Location = new System.Drawing.Point(12, 15);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(200, 13);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Title (include #rnwdataversepowershell):";
            // 
            // titleTextBox
            // 
            this.titleTextBox.Location = new System.Drawing.Point(15, 31);
            this.titleTextBox.Name = "titleTextBox";
            this.titleTextBox.Size = new System.Drawing.Size(457, 20);
            this.titleTextBox.TabIndex = 1;
            // 
            // apiKeyLabel
            // 
            this.apiKeyLabel.AutoSize = true;
            this.apiKeyLabel.Location = new System.Drawing.Point(12, 64);
            this.apiKeyLabel.Name = "apiKeyLabel";
            this.apiKeyLabel.Size = new System.Drawing.Size(128, 13);
            this.apiKeyLabel.TabIndex = 2;
            this.apiKeyLabel.Text = "PasteBin API Key:";
            // 
            // apiKeyTextBox
            // 
            this.apiKeyTextBox.Location = new System.Drawing.Point(15, 80);
            this.apiKeyTextBox.Name = "apiKeyTextBox";
            this.apiKeyTextBox.Size = new System.Drawing.Size(457, 20);
            this.apiKeyTextBox.TabIndex = 3;
            // 
            // visibilityLabel
            // 
            this.visibilityLabel.AutoSize = true;
            this.visibilityLabel.Location = new System.Drawing.Point(12, 113);
            this.visibilityLabel.Name = "visibilityLabel";
            this.visibilityLabel.Size = new System.Drawing.Size(58, 13);
            this.visibilityLabel.TabIndex = 4;
            this.visibilityLabel.Text = "Visibility:";
            // 
            // publicRadioButton
            // 
            this.publicRadioButton.AutoSize = true;
            this.publicRadioButton.Checked = true;
            this.publicRadioButton.Location = new System.Drawing.Point(15, 129);
            this.publicRadioButton.Name = "publicRadioButton";
            this.publicRadioButton.Size = new System.Drawing.Size(120, 17);
            this.publicRadioButton.TabIndex = 5;
            this.publicRadioButton.TabStop = true;
            this.publicRadioButton.Text = "Public (searchable)";
            this.publicRadioButton.UseVisualStyleBackColor = true;
            // 
            // unlistedRadioButton
            // 
            this.unlistedRadioButton.AutoSize = true;
            this.unlistedRadioButton.Location = new System.Drawing.Point(15, 152);
            this.unlistedRadioButton.Name = "unlistedRadioButton";
            this.unlistedRadioButton.Size = new System.Drawing.Size(180, 17);
            this.unlistedRadioButton.TabIndex = 6;
            this.unlistedRadioButton.Text = "Unlisted (only visible via URL)";
            this.unlistedRadioButton.UseVisualStyleBackColor = true;
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(316, 185);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 30);
            this.saveButton.TabIndex = 7;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(397, 185);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 30);
            this.cancelButton.TabIndex = 8;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // PasteSaveDialog
            // 
            this.AcceptButton = this.saveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 227);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.unlistedRadioButton);
            this.Controls.Add(this.publicRadioButton);
            this.Controls.Add(this.visibilityLabel);
            this.Controls.Add(this.apiKeyTextBox);
            this.Controls.Add(this.apiKeyLabel);
            this.Controls.Add(this.titleTextBox);
            this.Controls.Add(this.titleLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PasteSaveDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Save to PasteBin";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.TextBox titleTextBox;
        private System.Windows.Forms.Label apiKeyLabel;
        private System.Windows.Forms.TextBox apiKeyTextBox;
        private System.Windows.Forms.Label visibilityLabel;
        private System.Windows.Forms.RadioButton publicRadioButton;
        private System.Windows.Forms.RadioButton unlistedRadioButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
