namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    partial class GistSaveDialog
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
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.fileNameLabel = new System.Windows.Forms.Label();
            this.fileNameTextBox = new System.Windows.Forms.TextBox();
            this.visibilityLabel = new System.Windows.Forms.Label();
            this.publicRadioButton = new System.Windows.Forms.RadioButton();
            this.privateRadioButton = new System.Windows.Forms.RadioButton();
            this.tokenLabel = new System.Windows.Forms.Label();
            this.tokenTextBox = new System.Windows.Forms.TextBox();
            this.tokenLinkLabel = new System.Windows.Forms.LinkLabel();
            this.updateExistingCheckBox = new System.Windows.Forms.CheckBox();
            this.infoLabel = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Location = new System.Drawing.Point(12, 45);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(188, 13);
            this.descriptionLabel.TabIndex = 0;
            this.descriptionLabel.Text = "Description (include #rnwdataversepowershell):";
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Location = new System.Drawing.Point(15, 61);
            this.descriptionTextBox.Multiline = true;
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(457, 60);
            this.descriptionTextBox.TabIndex = 1;
            // 
            // fileNameLabel
            // 
            this.fileNameLabel.AutoSize = true;
            this.fileNameLabel.Location = new System.Drawing.Point(12, 134);
            this.fileNameLabel.Name = "fileNameLabel";
            this.fileNameLabel.Size = new System.Drawing.Size(104, 13);
            this.fileNameLabel.TabIndex = 2;
            this.fileNameLabel.Text = "File Name (*.ps1):";
            // 
            // fileNameTextBox
            // 
            this.fileNameTextBox.Location = new System.Drawing.Point(15, 150);
            this.fileNameTextBox.Name = "fileNameTextBox";
            this.fileNameTextBox.Size = new System.Drawing.Size(457, 20);
            this.fileNameTextBox.TabIndex = 3;
            this.fileNameTextBox.Text = "script.ps1";
            // 
            // visibilityLabel
            // 
            this.visibilityLabel.AutoSize = true;
            this.visibilityLabel.Location = new System.Drawing.Point(12, 183);
            this.visibilityLabel.Name = "visibilityLabel";
            this.visibilityLabel.Size = new System.Drawing.Size(58, 13);
            this.visibilityLabel.TabIndex = 4;
            this.visibilityLabel.Text = "Visibility:";
            // 
            // publicRadioButton
            // 
            this.publicRadioButton.AutoSize = true;
            this.publicRadioButton.Checked = true;
            this.publicRadioButton.Location = new System.Drawing.Point(15, 199);
            this.publicRadioButton.Name = "publicRadioButton";
            this.publicRadioButton.Size = new System.Drawing.Size(212, 17);
            this.publicRadioButton.TabIndex = 5;
            this.publicRadioButton.TabStop = true;
            this.publicRadioButton.Text = "Public (visible in script gallery)";
            this.publicRadioButton.UseVisualStyleBackColor = true;
            // 
            // privateRadioButton
            // 
            this.privateRadioButton.AutoSize = true;
            this.privateRadioButton.Location = new System.Drawing.Point(15, 222);
            this.privateRadioButton.Name = "privateRadioButton";
            this.privateRadioButton.Size = new System.Drawing.Size(220, 17);
            this.privateRadioButton.TabIndex = 6;
            this.privateRadioButton.Text = "Private (not visible in script gallery)";
            this.privateRadioButton.UseVisualStyleBackColor = true;
            // 
            // tokenLabel
            // 
            this.tokenLabel.AutoSize = true;
            this.tokenLabel.Location = new System.Drawing.Point(12, 252);
            this.tokenLabel.Name = "tokenLabel";
            this.tokenLabel.Size = new System.Drawing.Size(165, 13);
            this.tokenLabel.TabIndex = 7;
            this.tokenLabel.Text = "GitHub Personal Access Token:";
            // 
            // tokenTextBox
            // 
            this.tokenTextBox.Location = new System.Drawing.Point(15, 268);
            this.tokenTextBox.Name = "tokenTextBox";
            this.tokenTextBox.Size = new System.Drawing.Size(457, 20);
            this.tokenTextBox.TabIndex = 8;
            this.tokenTextBox.UseSystemPasswordChar = true;
            // 
            // tokenLinkLabel
            // 
            this.tokenLinkLabel.AutoSize = true;
            this.tokenLinkLabel.Location = new System.Drawing.Point(12, 291);
            this.tokenLinkLabel.Name = "tokenLinkLabel";
            this.tokenLinkLabel.Size = new System.Drawing.Size(316, 13);
            this.tokenLinkLabel.TabIndex = 9;
            this.tokenLinkLabel.TabStop = true;
            this.tokenLinkLabel.Text = "Create token at: https://github.com/settings/tokens (scope: gist)";
            this.tokenLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler((s, e) => 
                System.Diagnostics.Process.Start("https://github.com/settings/tokens"));
            // 
            // updateExistingCheckBox
            // 
            this.updateExistingCheckBox.AutoSize = true;
            this.updateExistingCheckBox.Location = new System.Drawing.Point(15, 320);
            this.updateExistingCheckBox.Name = "updateExistingCheckBox";
            this.updateExistingCheckBox.Size = new System.Drawing.Size(245, 17);
            this.updateExistingCheckBox.TabIndex = 10;
            this.updateExistingCheckBox.Text = "Update existing gist (instead of creating new)";
            this.updateExistingCheckBox.UseVisualStyleBackColor = true;
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic);
            this.infoLabel.ForeColor = System.Drawing.Color.Gray;
            this.infoLabel.Location = new System.Drawing.Point(12, 15);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(100, 13);
            this.infoLabel.TabIndex = 11;
            this.infoLabel.Text = "Opened from gist";
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(316, 353);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 30);
            this.saveButton.TabIndex = 12;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(397, 353);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 30);
            this.cancelButton.TabIndex = 13;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // GistSaveDialog
            // 
            this.AcceptButton = this.saveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 395);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.updateExistingCheckBox);
            this.Controls.Add(this.tokenLinkLabel);
            this.Controls.Add(this.tokenTextBox);
            this.Controls.Add(this.tokenLabel);
            this.Controls.Add(this.privateRadioButton);
            this.Controls.Add(this.publicRadioButton);
            this.Controls.Add(this.visibilityLabel);
            this.Controls.Add(this.fileNameTextBox);
            this.Controls.Add(this.fileNameLabel);
            this.Controls.Add(this.descriptionTextBox);
            this.Controls.Add(this.descriptionLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GistSaveDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Save to GitHub Gist";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Label fileNameLabel;
        private System.Windows.Forms.TextBox fileNameTextBox;
        private System.Windows.Forms.Label visibilityLabel;
        private System.Windows.Forms.RadioButton publicRadioButton;
        private System.Windows.Forms.RadioButton privateRadioButton;
        private System.Windows.Forms.Label tokenLabel;
        private System.Windows.Forms.TextBox tokenTextBox;
        private System.Windows.Forms.LinkLabel tokenLinkLabel;
        private System.Windows.Forms.CheckBox updateExistingCheckBox;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
