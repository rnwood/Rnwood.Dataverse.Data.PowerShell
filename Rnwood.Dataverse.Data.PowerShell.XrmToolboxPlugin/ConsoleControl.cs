using ConEmu.WinForms;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class ConsoleControl : UserControl
    {
        private Dictionary<TabPage, ConsoleTabControl> tabControls = new Dictionary<TabPage, ConsoleTabControl>();
        private int scriptSessionCounter = 1;
        private CrmServiceClient service;
        private CancellationTokenSource namedPipeCancellation;
        private string pipeName;

        public ConsoleControl()
        {
            InitializeComponent();
        }

        private TabPage CreateConsoleTab(string title, ConsoleTabControl consoleTabControl)
        {
            TabPage tabPage = new TabPage(title);
            consoleTabControl.Dock = DockStyle.Fill;
            tabPage.Controls.Add(consoleTabControl);
            consoleTabControl.CloseRequested += (s, e) =>
            {
                tabControl.TabPages.Remove(tabPage);
                if (tabControls.ContainsKey(tabPage))
                {
                    tabControls[tabPage].DisposeConsole();
                    tabControls.Remove(tabPage);
                }
            };
            return tabPage;
        }

        public void SetService(CrmServiceClient service)
        {
            this.service = service;
        }

        public void StartEmbeddedPowerShellConsole()
        {
            try
            {
                var connectionInfo = ExtractConnectionInformation(service);

                // Start named pipe server for dynamic token extraction
                if (connectionInfo != null && connectionInfo.Token == "DYNAMIC")
                {
                    StartNamedPipeServer();
                }

                StartSession("Interactive", "", connectionInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start PowerShell console: {ex.Message}.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConsoleControl_ConsoleProcessExited(object sender, ConsoleProcessExitedEventArgs e)
        {

        }

        /// <summary>
        /// Extracts connection information from a CrmServiceClient for use in PowerShell operations.
        /// </summary>
        public class ConnectionInfo
        {
            public string Url { get; set; }
            public string Token { get; set; }
            public string PipeName { get; set; }
            public string OrgName { get; set; }
        }

        /// <summary>
        /// Starts a named pipe server that provides fresh tokens on demand.
        /// </summary>
        private void StartNamedPipeServer()
        {
            pipeName = $"xrmtoolbox-token-{Guid.NewGuid()}";
            namedPipeCancellation = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!namedPipeCancellation.Token.IsCancellationRequested)
                {
                    try
                    {
                        using (var pipeServer = new NamedPipeServerStream(
                            pipeName,
                            PipeDirection.Out,
                            NamedPipeServerStream.MaxAllowedServerInstances,
                            PipeTransmissionMode.Message,
                            PipeOptions.Asynchronous))
                        {
                            // Wait for client connection
                            await pipeServer.WaitForConnectionAsync(namedPipeCancellation.Token);

                            try
                            {
                                // Extract fresh token
                                string token = service?.CurrentAccessToken;

                                if (!string.IsNullOrEmpty(token))
                                {
                                    byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
                                    await pipeServer.WriteAsync(tokenBytes, 0, tokenBytes.Length, namedPipeCancellation.Token);
                                    await pipeServer.FlushAsync(namedPipeCancellation.Token);
                                }
                            }
                            catch (Exception)
                            {
                                // Ignore errors writing to pipe
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        // Ignore errors and retry
                        if (!namedPipeCancellation.Token.IsCancellationRequested)
                        {
                            await Task.Delay(100);
                        }
                    }
                }
            }, namedPipeCancellation.Token);
        }

        private ConnectionInfo ExtractConnectionInformation(CrmServiceClient client)
        {
            try
            {
                if (client == null)
                {
                    return null;
                }

                // Extract connection URL
                string orgUrl = client.CrmConnectOrgUriActual?.ToString();
                if (string.IsNullOrEmpty(orgUrl))
                {
                    // Try alternative properties
                    if (client.ConnectedOrgPublishedEndpoints?.ContainsKey(Microsoft.Xrm.Sdk.Discovery.EndpointType.WebApplication) == true)
                    {
                        orgUrl = client.ConnectedOrgPublishedEndpoints[Microsoft.Xrm.Sdk.Discovery.EndpointType.WebApplication];
                    }
                }

                if (string.IsNullOrEmpty(orgUrl))
                {
                    return null;
                }

                // Check if token is available
                bool tokenAvailable = false;
                try
                {
                    var token = client.CurrentAccessToken;
                    tokenAvailable = !string.IsNullOrEmpty(token);
                }
                catch
                {
                    tokenAvailable = false;
                }

                return new ConnectionInfo
                {
                    Url = orgUrl,
                    Token = tokenAvailable ? "DYNAMIC" : null,
                    OrgName = client.ConnectedOrgFriendlyName
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static ConnectionInfo ExtractConnectionInfo(CrmServiceClient client)
        {
            try
            {
                if (client == null)
                {
                    return null;
                }

                // Extract connection URL
                string orgUrl = client.CrmConnectOrgUriActual?.ToString();
                if (string.IsNullOrEmpty(orgUrl))
                {
                    // Try alternative properties
                    if (client.ConnectedOrgPublishedEndpoints?.ContainsKey(Microsoft.Xrm.Sdk.Discovery.EndpointType.WebApplication) == true)
                    {
                        orgUrl = client.ConnectedOrgPublishedEndpoints[Microsoft.Xrm.Sdk.Discovery.EndpointType.WebApplication];
                    }
                }

                if (string.IsNullOrEmpty(orgUrl))
                {
                    return null;
                }

                // Try to get the access token
                string accessToken = null;
                try
                {
                    accessToken = client.CurrentAccessToken;
                }
                catch
                {
                    // If we can't get the token, that's okay - we'll fall back to interactive auth
                }

                return new ConnectionInfo
                {
                    Url = orgUrl,
                    Token = accessToken,
                    OrgName = client.ConnectedOrgFriendlyName
                };
            }
            catch (Exception)
            {
                // Could not extract connection information, will fall back to manual connection
                return null;
            }
        }

        private string GenerateConnectionScript(string bundledModulePath, ConnectionInfo connectionInfo, string userScript)
        {
            string script = $@"
# Check PowerShell execution environment

# Check for Restricted Language Mode
if ($ExecutionContext.SessionState.LanguageMode -eq 'RestrictedLanguage') {{
    Write-Host 'ERROR: PowerShell is running in Restricted Language Mode' -ForegroundColor Red
    Write-Host 'This security setting prevents the module from loading.' -ForegroundColor Red
    Write-Host ''
    Write-Host 'To fix this, you may need to:' -ForegroundColor Yellow
    Write-Host '  1. Check your organization''s PowerShell security policies' -ForegroundColor Yellow
    Write-Host '  2. Contact your IT administrator' -ForegroundColor Yellow
    Write-Host ''
    Write-Host 'Press any key to exit...' -ForegroundColor Yellow
    $null = $host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
    exit 1
}}

# Check execution policy
$executionPolicy = Get-ExecutionPolicy

if ($executionPolicy -eq 'Restricted' -or $executionPolicy -eq 'AllSigned') {{
    Write-Host ''
    Write-Host 'WARNING: PowerShell Execution Policy may prevent scripts from running' -ForegroundColor Yellow
    Write-Host ""Current policy: $executionPolicy"" -ForegroundColor Yellow
    Write-Host ''
    Write-Host 'To fix this, execute:' -ForegroundColor Yellow
    Write-Host '  Set-ExecutionPolicy RemoteSigned -Scope CurrentUser' -ForegroundColor Yellow
    Write-Host ''
    Write-Host 'Attempting to continue anyway...' -ForegroundColor Yellow
    Write-Host ''
}}

$bundledModulePath = '{bundledModulePath.Replace("\\", "\\\\")}'
Import-Module $bundledModulePath/Rnwood.Dataverse.Data.PowerShell.psd1 -ErrorAction Stop
";

            // Add connection logic if we have connection info
            if (connectionInfo != null && !string.IsNullOrEmpty(connectionInfo.Url))
            {
                script += $@"
Write-Host ""Connecting to: {connectionInfo.Url}"" -ForegroundColor Cyan
";

                if (connectionInfo.Token == "DYNAMIC" && !string.IsNullOrEmpty(pipeName))
                {
                    // Use named pipe for dynamic token retrieval
                    script += $@"
# Create script block that retrieves token from named pipe
$tokenScriptBlock = {{

    try {{
        $pipeName = '{pipeName}'
        $pipeClient = New-Object System.IO.Pipes.NamedPipeClientStream('.', $pipeName, [System.IO.Pipes.PipeDirection]::In)
        $pipeClient.Connect(5000)  # 5 second timeout
        
        $reader = New-Object System.IO.StreamReader($pipeClient)
        $token = $reader.ReadToEnd()
        
        $reader.Close()
        $pipeClient.Close()
        
        return $token
    }} catch {{
        throw ""Failed to retrieve token from XrmToolbox: $($_.Exception.Message)""
    }}
}}

try {{
    Get-DataverseConnection -AccessToken $tokenScriptBlock -Url '{connectionInfo.Url}' -SetAsDefault -ErrorAction Stop | Out-Null
    Write-Host 'Connected successfully using XrmToolbox token!' -ForegroundColor Green
    Write-Host ''
}} catch {{
    Write-Host ""WARNING: Failed to connect using XrmToolbox token: $($_.Exception.Message)"" -ForegroundColor Yellow
    Write-Host 'Falling back to interactive authentication...' -ForegroundColor Yellow
    try {{
        Get-DataverseConnection -Url '{connectionInfo.Url}' -Interactive -SetAsDefault -ErrorAction Stop | Out-Null
        Write-Host 'Connected successfully!' -ForegroundColor Green
        Write-Host ''
    }} catch {{
        Write-Host ""ERROR: Failed to connect: $($_.Exception.Message)"" -ForegroundColor Red
        Write-Host 'You can try connecting manually with:' -ForegroundColor Yellow
        Write-Host '  Get-DataverseConnection -Interactive -SetAsDefault' -ForegroundColor Cyan
    }}
}}
";
                }
                else
                {
                    // No token available, use interactive
                    script += $@"
Write-Host 'No access token available, connecting interactively...' -ForegroundColor Yellow 
try {{
    Get-DataverseConnection -Url '{connectionInfo.Url}' -Interactive -SetAsDefault -ErrorAction Stop | Out-Null
    Write-Host 'Connected successfully!' -ForegroundColor Green
    Write-Host ''
}} catch {{
    Write-Host ""ERROR: Failed to connect: $($_.Exception.Message)"" -ForegroundColor Red
    Write-Host 'You can try connecting manually with:' -ForegroundColor Yellow
    Write-Host '  Get-DataverseConnection -Interactive -SetAsDefault' -ForegroundColor Cyan
}}
";
                }
            }
            else
            {
                script += @"
Write-Host 'To connect to your Dataverse environment, run:' -ForegroundColor Yellow
Write-Host '  Get-DataverseConnection -Interactive -SetAsDefault' -ForegroundColor Cyan
";
            }

            // Append user script if provided
            if (!string.IsNullOrEmpty(userScript))
            {
                script += "\n" + userScript + "\n";
            }

            return script;
        }

        public void DisposeResources()
        {
            foreach (var tab in tabControls.Values.ToArray())
            {
                try
                {
                    tab.DisposeConsole();
                }
                catch
                {
                    // Ignore errors when disposing
                }
            }
            tabControls.Clear();

            // Stop named pipe server
            StopNamedPipeServer();
        }

        private void StopNamedPipeServer()
        {
            if (namedPipeCancellation != null)
            {
                try
                {
                    namedPipeCancellation.Cancel();
                    namedPipeCancellation.Dispose();
                }
                catch
                {
                    // Ignore cleanup errors
                }
                namedPipeCancellation = null;
            }
            pipeName = null;
        }

        public void StartScriptSession(string script)
        {
            var connectionInfo = ExtractConnectionInformation(service);
            StartScriptSession(script, connectionInfo);
        }

        public void StartScriptSession(string script, ConnectionInfo connectionInfo)
        {
            StartSession($"Script Session {scriptSessionCounter++}", script, connectionInfo);
        }

        private void StartConEmuSession(string title, string scriptContent, ConnectionInfo connectionInfo)
        {
            ConsoleTabControl consoleTabControl = new ConsoleTabControl();

            if (connectionInfo != null)
            {
                consoleTabControl.SetConnectionInfo(connectionInfo.OrgName, connectionInfo.Url);
            }

            TabPage tabPage = CreateConsoleTab(title, consoleTabControl);

            tabControls[tabPage] = consoleTabControl;

            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;

            // Start the session using the new encapsulated method
            consoleTabControl.ProcessExited += (s, e) =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    tabControl.TabPages.Remove(tabPage);
                    if (tabControls.ContainsKey(tabPage))
                    {
                        tabControls[tabPage].DisposeConsole();
                        tabControls.Remove(tabPage);
                    }
                });
            };

            consoleTabControl.StartSessionWithScriptContent(title, scriptContent);
        }

        private void StartSession(string title, string userScript, ConnectionInfo connectionInfo)
        {
            string bundledModulePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "PSModule");

            string connectionScript = GenerateConnectionScript(bundledModulePath, connectionInfo, userScript);

            StartConEmuSession(title, connectionScript, connectionInfo);
        }

        private void NewInteractiveSessionButton_Click(object sender, EventArgs e)
        {
            StartEmbeddedPowerShellConsole();
        }
    }
}