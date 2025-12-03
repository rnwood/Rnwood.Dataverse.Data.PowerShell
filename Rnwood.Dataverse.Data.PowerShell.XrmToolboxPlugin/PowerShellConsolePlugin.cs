using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;
using Microsoft.Xrm.Tooling.Connector;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class PowerShellConsolePlugin : PluginControlBase, IGitHubPlugin, IPayPalPlugin
    {
        private CrmServiceClient service;
        private GitHubGistService _gistService;

        public PowerShellConsolePlugin()
        {
            InitializeComponent();
            this.Load += PowerShellConsolePlugin_Load;
            _gistService = new GitHubGistService();
        }

        public string RepositoryName => "Rnwood.Dataverse.Data.PowerShell";
        public string UserName => "rnwood";
        public string DonationDescription => "Support development of this PowerShell module";
        public string EmailAccount => "rob@rnwood.co.uk";

        protected override void OnConnectionUpdated(ConnectionUpdatedEventArgs e)
        {
            base.OnConnectionUpdated(e);
            service = e.Service as CrmServiceClient;
            consoleControl.SetService(service);
        }

        private void PowerShellConsolePlugin_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                service = Service as CrmServiceClient;
                consoleControl.SetService(service);
                
                // Delay starting the console to ensure the form is fully loaded
                Task.Delay(1000).ContinueWith(_ =>
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        consoleControl.StartEmbeddedPowerShellConsole();
                    });
                });

                // Extract connection info for script editor
                var connectionInfo = ConsoleControl.ExtractConnectionInfo(service);
                if (connectionInfo != null)
                {
                    // Create a token provider function that extracts the token dynamically
                    Func<string> tokenProvider = () =>
                    {
                        try
                        {
                            return service?.CurrentAccessToken;
                        }
                        catch
                        {
                            return null;
                        }
                    };
                    
                    scriptEditorControl.InitializeMonacoEditor(tokenProvider, connectionInfo.Url);
                }
                else
                {
                    scriptEditorControl.InitializeMonacoEditor();
                }
                
                helpControl.LoadAndShowHelp();

                splitContainer.SplitterDistance = (splitContainer.Width / 3)*2;
                innerSplitContainer.SplitterDistance = (innerSplitContainer.Height / 3) * 2;

                // Wire up script editor events
                scriptEditorControl.RunScriptRequested += ScriptEditorControl_RunScriptRequested;
                scriptEditorControl.NewScriptRequested += ScriptEditorControl_NewScriptRequested;
                scriptEditorControl.OpenScriptRequested += ScriptEditorControl_OpenScriptRequested;
                scriptEditorControl.SaveScriptRequested += ScriptEditorControl_SaveScriptRequested;
                scriptEditorControl.SaveToGistRequested += ScriptEditorControl_SaveToGistRequested;
                scriptEditorControl.CompletionResolved += ScriptEditorControl_CompletionResolved;

                // Wire up script gallery events
                scriptGalleryControl.OpenGistRequested += ScriptGalleryControl_OpenGistRequested;
            }
        }

        private void ScriptEditorControl_CompletionResolved(object sender, CompletionItem e)
        {
            if (e == null) return;

            if (e.ResultType == CompletionResultType.Command)
            {
                helpControl.SelectHelpItem(e.CompletionText, null);
            }
            else if (e.ResultType == CompletionResultType.ParameterName && !string.IsNullOrEmpty(e.RelatedCommand))
            {
                helpControl.SelectHelpItem(e.RelatedCommand, e.CompletionText);
            }
        }

        private async void ScriptEditorControl_RunScriptRequested(object sender, EventArgs e)
        {
            try
            {
                // Get script content from Monaco editor
                string script = await scriptEditorControl.GetScriptContentAsync();

                if (string.IsNullOrWhiteSpace(script))
                {
                    MessageBox.Show("Script is empty. Please enter some PowerShell commands.",
                        "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Send script to console for execution
                var connectionInfo = ConsoleControl.ExtractConnectionInfo(service);
                consoleControl.StartScriptSession(script, connectionInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to run script: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ScriptEditorControl_NewScriptRequested(object sender, EventArgs e)
        {
            scriptEditorControl.CreateNewScript();
        }

        private void ScriptEditorControl_OpenScriptRequested(object sender, EventArgs e)
        {
            scriptEditorControl.OpenScript();
        }

        private void ScriptEditorControl_SaveScriptRequested(object sender, EventArgs e)
        {
            scriptEditorControl.SaveScript();
        }

        private async void ScriptEditorControl_SaveToGistRequested(object sender, EventArgs e)
        {
            try
            {
                // Get script content
                string script = await scriptEditorControl.GetScriptContentAsync();

                if (string.IsNullOrWhiteSpace(script))
                {
                    MessageBox.Show("Script is empty. Please enter some PowerShell commands.",
                        "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Check if this is an existing gist
                var existingGist = scriptEditorControl.GetCurrentTabGist();

                // Prompt for description and filename
                using (var dialog = new GistSaveDialog(existingGist))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Check if token is set
                        if (string.IsNullOrEmpty(dialog.GitHubToken))
                        {
                            MessageBox.Show("GitHub Personal Access Token is required to save gists.\n\n" +
                                          "Create a token at: https://github.com/settings/tokens\n" +
                                          "Required scope: gist",
                                "GitHub Token Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        _gistService.SetAccessToken(dialog.GitHubToken);

                        GistInfo result;
                        if (existingGist != null && dialog.UpdateExisting)
                        {
                            // Update existing gist
                            result = await _gistService.UpdateGistAsync(
                                existingGist.Id,
                                dialog.Description,
                                dialog.FileName,
                                script);

                            MessageBox.Show($"Gist updated successfully!\n\nURL: {result.HtmlUrl}",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            // Create new gist
                            result = await _gistService.CreateGistAsync(
                                dialog.Description,
                                dialog.FileName,
                                script,
                                dialog.IsPublic);

                            MessageBox.Show($"Gist created successfully!\n\nURL: {result.HtmlUrl}",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save gist: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ScriptGalleryControl_OpenGistRequested(object sender, GistInfo gist)
        {
            try
            {
                await scriptEditorControl.OpenFromGistAsync(gist);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open gist: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public override void ClosingPlugin(PluginCloseInfo info)
        {
            // Dispose completion service and other resources
            scriptEditorControl.DisposeResources();
            consoleControl.DisposeResources();

            base.ClosingPlugin(info);
        }
    }
}
