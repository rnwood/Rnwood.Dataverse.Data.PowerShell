using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
// webview removed - replaced with multiline TextBox

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public class ScriptSaveDialog : Form
    {
        private TextBox txtTitle;
        private TextBox txtDescription;
        private CheckedListBox lstTags;
        private Button btnSave;
        private Button btnCancel;
        // webview removed - no ready task needed.

        public string ScriptTitle => txtTitle.Text;
        public List<string> SelectedTags => lstTags.CheckedItems.Cast<string>().ToList();
        
        // We'll retrieve this async
        public Task<string> GetDescriptionAsync() => Task.FromResult(txtDescription.Text);

        private string _initialDescription;

        public ScriptSaveDialog(string title, string description, List<string> availableTags, List<string> selectedTags)
        {
            InitializeComponent();
            // No WebView2; nothing to initialize
            
            txtTitle.Text = title;
            
            // Populate tags
            if (availableTags != null)
            {
                foreach (var tag in availableTags)
                {
                    if (tag == "(All)") continue;
                    bool isChecked = selectedTags != null && selectedTags.Contains(tag);
                    lstTags.Items.Add(tag, isChecked);
                }
            }
            
            _initialDescription = description;
            txtDescription.Text = description ?? string.Empty;
        }

        // No initialization required when using simple TextBox for description
        // Keep InitializeAsync API for callers (no-op)

        // No OnShown fallback required; removal of InitializeAsync simplifies dialog

        private void InitializeComponent()
        {
            this.Text = "Save Script to Gallery";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblTitle = new Label { Text = "Title:", Location = new Point(10, 10), AutoSize = true };
            txtTitle = new TextBox { Location = new Point(10, 30), Width = 760, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            var lblDescription = new Label { Text = "Description (Markdown):", Location = new Point(10, 60), AutoSize = true };
            txtDescription = new TextBox { Location = new Point(10, 80), Size = new Size(760, 350), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, Multiline = true, ScrollBars = ScrollBars.Vertical };

            var lblTags = new Label { Text = "Tags:", Location = new Point(10, 440), AutoSize = true, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            lstTags = new CheckedListBox { Location = new Point(10, 460), Size = new Size(760, 60), Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, MultiColumn = true };

            btnSave = new Button { Text = "Save", Location = new Point(610, 530), Size = new Size(75, 23), DialogResult = DialogResult.OK, Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            btnCancel = new Button { Text = "Cancel", Location = new Point(695, 530), Size = new Size(75, 23), DialogResult = DialogResult.Cancel, Anchor = AnchorStyles.Bottom | AnchorStyles.Right };

            this.Controls.Add(lblTitle);
            this.Controls.Add(txtTitle);
            this.Controls.Add(lblDescription);
            this.Controls.Add(txtDescription);
            this.Controls.Add(lblTags);
            this.Controls.Add(lstTags);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }



        // Description now stored in txtDescription. No webview or helpers required.
    }
}
