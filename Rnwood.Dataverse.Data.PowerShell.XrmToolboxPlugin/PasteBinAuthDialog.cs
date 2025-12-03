using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class PasteBinAuthDialog : Form
    {
        public string ApiKey => apiKeyTextBox.Text;
        public string Username => usernameTextBox.Text;
        public string Password => passwordTextBox.Text;

        public PasteBinAuthDialog()
        {
            InitializeComponent();
        }

        private void SignInButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(apiKeyTextBox.Text))
            {
                MessageBox.Show("API Key is required.\n\nGet your API key from: https://pastebin.com/doc_api#1", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                apiKeyTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(usernameTextBox.Text))
            {
                MessageBox.Show("Username is required.", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                usernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(passwordTextBox.Text))
            {
                MessageBox.Show("Password is required.", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                passwordTextBox.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void GetApiKeyLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://pastebin.com/doc_api#1",
                    UseShellExecute = true
                });
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}
