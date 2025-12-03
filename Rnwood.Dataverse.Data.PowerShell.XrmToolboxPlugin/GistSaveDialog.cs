using System;
using System.Windows.Forms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class GistSaveDialog : Form
    {
        private GistInfo _existingGist;

        public string Description => descriptionTextBox.Text;
        public string FileName => fileNameTextBox.Text;
        public bool IsPublic => publicRadioButton.Checked;
        public bool UpdateExisting => updateExistingCheckBox.Checked;
        public string GitHubToken => tokenTextBox.Text;

        public GistSaveDialog(GistInfo existingGist = null)
        {
            InitializeComponent();
            _existingGist = existingGist;

            if (_existingGist != null)
            {
                // Pre-fill with existing gist info
                descriptionTextBox.Text = _existingGist.Description ?? "";
                fileNameTextBox.Text = _existingGist.GetFirstPowerShellFile() ?? "script.ps1";
                publicRadioButton.Checked = _existingGist.IsPublic;
                privateRadioButton.Checked = !_existingGist.IsPublic;
                updateExistingCheckBox.Checked = true;
                updateExistingCheckBox.Enabled = true;

                // Show info about existing gist
                infoLabel.Text = $"Opened from gist: {_existingGist.Id}";
                infoLabel.Visible = true;
            }
            else
            {
                updateExistingCheckBox.Enabled = false;
                updateExistingCheckBox.Checked = false;
                infoLabel.Visible = false;
            }

            // Ensure description contains the hashtag
            if (!descriptionTextBox.Text.Contains("#rnwdataversepowershell"))
            {
                descriptionTextBox.Text = descriptionTextBox.Text.TrimEnd() + " #rnwdataversepowershell";
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(descriptionTextBox.Text))
            {
                MessageBox.Show("Description is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                descriptionTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(fileNameTextBox.Text))
            {
                MessageBox.Show("File name is required.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                fileNameTextBox.Focus();
                return;
            }

            if (!fileNameTextBox.Text.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("File name must have .ps1 extension.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                fileNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(tokenTextBox.Text))
            {
                MessageBox.Show("GitHub Personal Access Token is required.\n\n" +
                              "Create a token at: https://github.com/settings/tokens\n" +
                              "Required scope: gist", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tokenTextBox.Focus();
                return;
            }

            // Ensure hashtag is in description
            if (!descriptionTextBox.Text.Contains("#rnwdataversepowershell"))
            {
                descriptionTextBox.Text = descriptionTextBox.Text.TrimEnd() + " #rnwdataversepowershell";
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
