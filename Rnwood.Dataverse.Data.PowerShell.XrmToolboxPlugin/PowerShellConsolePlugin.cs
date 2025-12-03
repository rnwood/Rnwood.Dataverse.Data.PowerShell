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
        private PasteBinService _pasteService;

        public PowerShellConsolePlugin()
        {
            InitializeComponent();
            this.Load += PowerShellConsolePlugin_Load;
            _pasteService = new PasteBinService();
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
                scriptEditorControl.SaveToPasteRequested += ScriptEditorControl_SaveToPasteRequested;
                scriptEditorControl.CompletionResolved += ScriptEditorControl_CompletionResolved;

                // Wire up script gallery events
                scriptGalleryControl.OpenPasteRequested += ScriptGalleryControl_OpenPasteRequested;
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

        private async void ScriptEditorControl_SaveToPasteRequested(object sender, EventArgs e)
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

                // Prompt for title and API key
                using (var dialog = new PasteSaveDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // Create new paste
                        var result = await _pasteService.CreatePasteAsync(
                            dialog.Title,
                            script,
                            dialog.IsPublic,
                            dialog.ApiKey);

                        MessageBox.Show($"Paste created successfully!\n\nURL: {result.Url}",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save paste: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ScriptGalleryControl_OpenPasteRequested(object sender, PasteInfo paste)
        {
            try
            {
                // Fetch full content if not already loaded
                if (string.IsNullOrEmpty(paste.Content))
                {
                    paste = await _pasteService.GetPasteAsync(paste.Key);
                }
                
                await scriptEditorControl.OpenFromPasteAsync(paste);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open paste: {ex.Message}",
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
