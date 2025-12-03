using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class DiscussionsControl : UserControl
    {
        private const string DISCUSSIONS_URL = "https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell/discussions";

        public DiscussionsControl()
        {
            InitializeComponent();
            this.Load += DiscussionsControl_Load;
        }

        private async void DiscussionsControl_Load(object sender, EventArgs e)
        {
            try
            {
                await LoadDiscussions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load discussions: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadDiscussions()
        {
            // Ensure WebView2 is initialized
            if (discussionsWebView.CoreWebView2 == null)
            {
                await discussionsWebView.EnsureCoreWebView2Async();
            }

            // Navigate to GitHub discussions
            discussionsWebView.CoreWebView2.Navigate(DISCUSSIONS_URL);
        }

        private void DiscussionsWebView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            progressBar.Visible = true;
        }

        private void DiscussionsWebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            progressBar.Visible = false;
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            if (discussionsWebView.CoreWebView2?.CanGoBack == true)
            {
                discussionsWebView.CoreWebView2.GoBack();
            }
        }

        private void ForwardButton_Click(object sender, EventArgs e)
        {
            if (discussionsWebView.CoreWebView2?.CanGoForward == true)
            {
                discussionsWebView.CoreWebView2.GoForward();
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            discussionsWebView.CoreWebView2?.Reload();
        }

        private void HomeButton_Click(object sender, EventArgs e)
        {
            if (discussionsWebView.CoreWebView2 != null)
            {
                discussionsWebView.CoreWebView2.Navigate(DISCUSSIONS_URL);
            }
        }
    }
}
