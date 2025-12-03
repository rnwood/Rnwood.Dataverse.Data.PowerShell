using System;
using System.Windows.Forms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class PasteSaveDialog : Form
    {
        public string Title => titleTextBox.Text;
        public string ApiKey => apiKeyTextBox.Text;
        public bool IsPublic => publicRadioButton.Checked;

        public PasteSaveDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(titleTextBox.Text))
            {
                MessageBox.Show("Title is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                titleTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(apiKeyTextBox.Text))
            {
                MessageBox.Show("PasteBin API Key is required.\n\n" +
                              "Get your API key from: https://pastebin.com/doc_api#1",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                apiKeyTextBox.Focus();
                return;
            }

            // Ensure hashtag is in title
            if (!titleTextBox.Text.Contains("#rnwdataversepowershell"))
            {
                titleTextBox.Text = titleTextBox.Text.TrimEnd() + " #rnwdataversepowershell";
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
