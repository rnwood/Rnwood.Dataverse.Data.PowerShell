using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;
using Microsoft.Xrm.Tooling.Connector;
using System.Windows.Resources;
using System.IO;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class MainControl : PluginControlBase, IGitHubPlugin, IPayPalPlugin
    {
        private CrmServiceClient service;

        public MainControl()
        {
            InitializeComponent();
            this.Load += PowerShellConsolePlugin_Load;
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
                scriptEditorControl.CompletionResolved += ScriptEditorControl_CompletionResolved;
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

                string filename = scriptEditorControl.GetCurrentFileName();

                // Send script to console for execution with the selected PowerShell version
                var connectionInfo = ConsoleControl.ExtractConnectionInfo(service);
                var powerShellVersion = scriptEditorControl.GetCurrentPowerShellVersion();
                consoleControl.StartScriptSession(filename, script, connectionInfo, powerShellVersion);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to run script: {ex.Message}",
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
