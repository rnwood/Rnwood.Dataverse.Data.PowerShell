using System;
using System.Drawing;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
// Removed webview2 - using simple multiline TextBox for comments

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public class CommentDialog : Form
    {
        private TextBox txtComment;
        private Button btnSubmit;
        private Button btnCancel;
        // No WebView2 - no ready task required

        public Task<string> GetCommentAsync() => Task.FromResult(txtComment.Text);

        public CommentDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Must be called before ShowDialog() to prevent deadlock.
        /// </summary>

        // No OnShown override needed; initialization removed

        private void InitializeComponent()
        {
            this.Text = "Add Comment";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            txtComment = new TextBox { Location = new Point(10, 10), Size = new Size(560, 300), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, Multiline = true, ScrollBars = ScrollBars.Vertical };

            btnSubmit = new Button { Text = "Submit", Location = new Point(410, 320), Size = new Size(75, 23), DialogResult = DialogResult.OK, Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            btnCancel = new Button { Text = "Cancel", Location = new Point(495, 320), Size = new Size(75, 23), DialogResult = DialogResult.Cancel, Anchor = AnchorStyles.Bottom | AnchorStyles.Right };

            this.Controls.Add(txtComment);
            this.Controls.Add(btnSubmit);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnSubmit;
            this.CancelButton = btnCancel;
        }



        // No WebView2 message handling - content stored in txtComment
    }
}
