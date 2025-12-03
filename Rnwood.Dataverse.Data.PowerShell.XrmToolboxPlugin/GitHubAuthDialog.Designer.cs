namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    partial class GitHubAuthDialog
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
            this.instructionsLabel = new System.Windows.Forms.Label();
            this.userCodeLabel = new System.Windows.Forms.Label();
            this.codeLabel = new System.Windows.Forms.Label();
            this.openBrowserButton = new System.Windows.Forms.Button();
            this.copyCodeButton = new System.Windows.Forms.Button();
            this.cancelAuthButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.titleLabel.Location = new System.Drawing.Point(12, 15);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(250, 20);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Sign in to GitHub";
            // 
            // instructionsLabel
            // 
            this.instructionsLabel.Location = new System.Drawing.Point(12, 50);
            this.instructionsLabel.Name = "instructionsLabel";
            this.instructionsLabel.Size = new System.Drawing.Size(460, 100);
            this.instructionsLabel.TabIndex = 1;
            this.instructionsLabel.Text = "Instructions will appear here";
            // 
            // codeLabel
            // 
            this.codeLabel.AutoSize = true;
            this.codeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.codeLabel.Location = new System.Drawing.Point(12, 160);
            this.codeLabel.Name = "codeLabel";
            this.codeLabel.Size = new System.Drawing.Size(120, 15);
            this.codeLabel.TabIndex = 2;
            this.codeLabel.Text = "Your code:";
            // 
            // userCodeLabel
            // 
            this.userCodeLabel.AutoSize = true;
            this.userCodeLabel.Font = new System.Drawing.Font("Consolas", 16F, System.Drawing.FontStyle.Bold);
            this.userCodeLabel.ForeColor = System.Drawing.Color.Blue;
            this.userCodeLabel.Location = new System.Drawing.Point(12, 180);
            this.userCodeLabel.Name = "userCodeLabel";
            this.userCodeLabel.Size = new System.Drawing.Size(120, 26);
            this.userCodeLabel.TabIndex = 3;
            this.userCodeLabel.Text = "XXXX-XXXX";
            // 
            // openBrowserButton
            // 
            this.openBrowserButton.Location = new System.Drawing.Point(16, 220);
            this.openBrowserButton.Name = "openBrowserButton";
            this.openBrowserButton.Size = new System.Drawing.Size(140, 35);
            this.openBrowserButton.TabIndex = 4;
            this.openBrowserButton.Text = "Open Browser";
            this.openBrowserButton.UseVisualStyleBackColor = true;
            this.openBrowserButton.Click += new System.EventHandler(this.OpenBrowserButton_Click);
            // 
            // copyCodeButton
            // 
            this.copyCodeButton.Location = new System.Drawing.Point(162, 220);
            this.copyCodeButton.Name = "copyCodeButton";
            this.copyCodeButton.Size = new System.Drawing.Size(140, 35);
            this.copyCodeButton.TabIndex = 5;
            this.copyCodeButton.Text = "Copy Code";
            this.copyCodeButton.UseVisualStyleBackColor = true;
            this.copyCodeButton.Click += new System.EventHandler(this.CopyCodeButton_Click);
            // 
            // cancelAuthButton
            // 
            this.cancelAuthButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelAuthButton.Location = new System.Drawing.Point(332, 220);
            this.cancelAuthButton.Name = "cancelAuthButton";
            this.cancelAuthButton.Size = new System.Drawing.Size(140, 35);
            this.cancelAuthButton.TabIndex = 6;
            this.cancelAuthButton.Text = "Cancel";
            this.cancelAuthButton.UseVisualStyleBackColor = true;
            this.cancelAuthButton.Click += new System.EventHandler(this.CancelAuthButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(16, 270);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(200, 13);
            this.statusLabel.TabIndex = 7;
            this.statusLabel.Text = "Waiting for authorization...";
            // 
            // GitHubAuthDialog
            // 
            this.AcceptButton = this.openBrowserButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelAuthButton;
            this.ClientSize = new System.Drawing.Size(484, 300);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.cancelAuthButton);
            this.Controls.Add(this.copyCodeButton);
            this.Controls.Add(this.openBrowserButton);
            this.Controls.Add(this.userCodeLabel);
            this.Controls.Add(this.codeLabel);
            this.Controls.Add(this.instructionsLabel);
            this.Controls.Add(this.titleLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GitHubAuthDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GitHub Authentication";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label instructionsLabel;
        private System.Windows.Forms.Label userCodeLabel;
        private System.Windows.Forms.Label codeLabel;
        private System.Windows.Forms.Button openBrowserButton;
        private System.Windows.Forms.Button copyCodeButton;
        private System.Windows.Forms.Button cancelAuthButton;
        private System.Windows.Forms.Label statusLabel;
    }
}
