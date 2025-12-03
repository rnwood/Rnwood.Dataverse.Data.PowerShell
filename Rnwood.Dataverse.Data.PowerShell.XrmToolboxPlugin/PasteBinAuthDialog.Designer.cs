namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    partial class PasteBinAuthDialog
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
            this.apiKeyLabel = new System.Windows.Forms.Label();
            this.apiKeyTextBox = new System.Windows.Forms.TextBox();
            this.getApiKeyLink = new System.Windows.Forms.LinkLabel();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.signInButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.titleLabel.Location = new System.Drawing.Point(12, 15);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(180, 20);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Sign in to PasteBin";
            // 
            // apiKeyLabel
            // 
            this.apiKeyLabel.AutoSize = true;
            this.apiKeyLabel.Location = new System.Drawing.Point(12, 50);
            this.apiKeyLabel.Name = "apiKeyLabel";
            this.apiKeyLabel.Size = new System.Drawing.Size(128, 13);
            this.apiKeyLabel.TabIndex = 1;
            this.apiKeyLabel.Text = "PasteBin API Key:";
            // 
            // apiKeyTextBox
            // 
            this.apiKeyTextBox.Location = new System.Drawing.Point(15, 66);
            this.apiKeyTextBox.Name = "apiKeyTextBox";
            this.apiKeyTextBox.Size = new System.Drawing.Size(457, 20);
            this.apiKeyTextBox.TabIndex = 2;
            // 
            // getApiKeyLink
            // 
            this.getApiKeyLink.AutoSize = true;
            this.getApiKeyLink.Location = new System.Drawing.Point(12, 89);
            this.getApiKeyLink.Name = "getApiKeyLink";
            this.getApiKeyLink.Size = new System.Drawing.Size(300, 13);
            this.getApiKeyLink.TabIndex = 3;
            this.getApiKeyLink.TabStop = true;
            this.getApiKeyLink.Text = "Get your API key from: https://pastebin.com/doc_api#1";
            this.getApiKeyLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GetApiKeyLink_LinkClicked);
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Location = new System.Drawing.Point(12, 115);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(116, 13);
            this.usernameLabel.TabIndex = 4;
            this.usernameLabel.Text = "PasteBin Username:";
            // 
            // usernameTextBox
            // 
            this.usernameTextBox.Location = new System.Drawing.Point(15, 131);
            this.usernameTextBox.Name = "usernameTextBox";
            this.usernameTextBox.Size = new System.Drawing.Size(457, 20);
            this.usernameTextBox.TabIndex = 5;
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(12, 160);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(113, 13);
            this.passwordLabel.TabIndex = 6;
            this.passwordLabel.Text = "PasteBin Password:";
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(15, 176);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(457, 20);
            this.passwordTextBox.TabIndex = 7;
            this.passwordTextBox.UseSystemPasswordChar = true;
            // 
            // signInButton
            // 
            this.signInButton.Location = new System.Drawing.Point(316, 215);
            this.signInButton.Name = "signInButton";
            this.signInButton.Size = new System.Drawing.Size(75, 30);
            this.signInButton.TabIndex = 8;
            this.signInButton.Text = "Sign In";
            this.signInButton.UseVisualStyleBackColor = true;
            this.signInButton.Click += new System.EventHandler(this.SignInButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(397, 215);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 30);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // PasteBinAuthDialog
            // 
            this.AcceptButton = this.signInButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 257);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.signInButton);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.usernameTextBox);
            this.Controls.Add(this.usernameLabel);
            this.Controls.Add(this.getApiKeyLink);
            this.Controls.Add(this.apiKeyTextBox);
            this.Controls.Add(this.apiKeyLabel);
            this.Controls.Add(this.titleLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PasteBinAuthDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PasteBin Authentication";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label apiKeyLabel;
        private System.Windows.Forms.TextBox apiKeyTextBox;
        private System.Windows.Forms.LinkLabel getApiKeyLink;
        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.TextBox usernameTextBox;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Button signInButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
