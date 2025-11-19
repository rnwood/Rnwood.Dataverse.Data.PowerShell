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
        private ConnectionInfo connectionInfo;
        private int scriptSessionCounter = 1;
        private CrmServiceClient service;

        public ConsoleControl()
        {
            InitializeComponent();
        }

        private TabPage CreateConsoleTab(string title, ConsoleTabControl consoleTabControl)
        {
            TabPage tabPage = new TabPage(title);
            consoleTabControl.Dock = DockStyle.Fill;
            tabPage.Controls.Add(consoleTabControl);
            consoleTabControl.CloseRequested += (s, e) => {
                tabControl.TabPages.Remove(tabPage);
                if (tabControls.ContainsKey(tabPage)) {
                    tabControls[tabPage].DisposeConsole();
                    tabControls.Remove(tabPage);
                }
            };
            return tabPage;
        }

        public void StartEmbeddedPowerShellConsole(CrmServiceClient service)
        {
            this.service = service;
            try
            {
                var connectionInfo = ExtractConnectionInformation(service);
                this.connectionInfo = connectionInfo;

                // Dispose any existing console sessions before starting a new one
                DisposeResources();

                StartSession("Main Console", "", connectionInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start PowerShell console: {ex.Message}.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConsoleControl_ConsoleProcessExited(object sender, ConsoleProcessExitedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Extracts connection information from a CrmServiceClient for use in PowerShell operations.
        /// </summary>
        public class ConnectionInfo
        {
            public string Url { get; set; }
            public string Token { get; set; }
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
                    Token = accessToken
                };
            }
            catch (Exception)
            {
                // Could not extract connection information, will fall back to manual connection
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
                    Token = accessToken
                };
            }
            catch (Exception)
            {
                // Could not extract connection information, will fall back to manual connection
                return null;
            }
        }

        private string GenerateConnectionScript(string bundledModulePath, string tempConnectionFile, string userScript)
        {
            string script = $@"
# Check PowerShell execution environment
Write-Host '================================================' -ForegroundColor Cyan
Write-Host 'Rnwood.Dataverse.Data.PowerShell Console' -ForegroundColor Cyan
Write-Host '================================================' -ForegroundColor Cyan
Write-Host ''

# Check for Restricted Language Mode
if ($ExecutionContext.SessionState.LanguageMode -eq 'RestrictedLanguage') {{
    Write-Host 'ERROR: PowerShell is running in Restricted Language Mode' -ForegroundColor Red
    Write-Host 'This security setting prevents the module from loading.' -ForegroundColor Red
    Write-Host ''
    Write-Host 'To fix this, you may need to:' -ForegroundColor Yellow
    Write-Host '  1. Check your organization''s PowerShell security policies' -ForegroundColor Yellow
    Write-Host '  2. Contact your IT administrator' -ForegroundColor Yellow
    Write-Host '  3. Run PowerShell as Administrator and execute: Set-ExecutionPolicy RemoteSigned' -ForegroundColor Yellow
    Write-Host ''
    Write-Host 'Press any key to exit...' -ForegroundColor Yellow
    $null = $host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
    exit 1
}}

# Check execution policy
$executionPolicy = Get-ExecutionPolicy
Write-Host ""Current Execution Policy: $executionPolicy"" -ForegroundColor Cyan

if ($executionPolicy -eq 'Restricted' -or $executionPolicy -eq 'AllSigned') {{
    Write-Host ''
    Write-Host 'WARNING: PowerShell Execution Policy may prevent scripts from running' -ForegroundColor Yellow
    Write-Host ""Current policy: $executionPolicy"" -ForegroundColor Yellow
    Write-Host ''
    Write-Host 'To fix this, run PowerShell as Administrator and execute:' -ForegroundColor Yellow
    Write-Host '  Set-ExecutionPolicy RemoteSigned -Scope LocalMachine' -ForegroundColor Yellow
    Write-Host ''
    Write-Host 'Or for current user only:' -ForegroundColor Yellow
    Write-Host '  Set-ExecutionPolicy RemoteSigned -Scope CurrentUser' -ForegroundColor Yellow
    Write-Host ''
    Write-Host 'Attempting to continue anyway...' -ForegroundColor Yellow
    Write-Host ''
}}

Write-Host 'Loading PowerShell module...' -ForegroundColor Yellow

# Try to load bundled module first
$bundledModulePath = '{bundledModulePath.Replace("\\", "\\\\")}'
$moduleLoaded = $false

try {{
    Import-Module $bundledModulePath/Rnwood.Dataverse.Data.PowerShell.psd1 -ErrorAction Stop
    Write-Host 'Module loaded from bundled location!' -ForegroundColor Green
    $moduleLoaded = $true
}} catch {{
    Write-Host ""Warning: Could not load bundled module: $($_.Exception.Message)"" -ForegroundColor Yellow
}}

Write-Host ''
";

            // Add connection logic if we have a temp connection file
            if (!string.IsNullOrEmpty(tempConnectionFile))
            {
                script += $@"
Write-Host 'Connecting to Dataverse using XrmToolbox connection...' -ForegroundColor Yellow

$tempConnectionFile = '{tempConnectionFile.Replace("\\", "\\\\")}'
try {{
    $lines = Get-Content $tempConnectionFile
    $url = $null
    $token = $null
    foreach ($line in $lines) {{
        if ($line -match '^URL:(.+)$') {{
            $url = $matches[1].Trim()
        }}
        elseif ($line -match '^TOKEN:(.+)$') {{
            $token = $matches[1].Trim()
        }}
    }}
    # Clean up the file
    Remove-Item $tempConnectionFile -ErrorAction SilentlyContinue
    if (!$url) {{
        Write-Host ""ERROR: URL is required for automatic connection from XrmToolbox."" -ForegroundColor Red
        Write-Host ""You can connect manually with:"" -ForegroundColor Yellow
        Write-Host ""  Get-DataverseConnection -Interactive -SetAsDefault"" -ForegroundColor Cyan
    }} else {{
        Write-Host ""Connecting to: $url"" -ForegroundColor Cyan
        if ($token) {{
            # Use OAuth token
            Write-Host 'Using OAuth token from XrmToolbox...' -ForegroundColor Yellow
            try {{
                Get-DataverseConnection -AccessToken $token -Url $url -SetAsDefault -ErrorAction Stop
                Write-Host 'Connected successfully!' -ForegroundColor Green
                Write-Host ''
                # Display connection info
                try {{
                    $whoami = Get-DataverseWhoAmI -Connection $global:connection
                    Write-Host 'Connected as:' -ForegroundColor Cyan
                    Write-Host ""  User ID: $($whoami.UserId)"" -ForegroundColor White
                    Write-Host ""  Organization: $($whoami.OrganizationId)"" -ForegroundColor White
                    Write-Host ''
                }} catch {{
                    Write-Host 'Warning: Could not retrieve user info' -ForegroundColor Yellow
                }}
            }} catch {{
                Write-Host ""ERROR: Failed to connect: $($_.Exception.Message)"" -ForegroundColor Red
                Write-Host 'You can try connecting manually with:' -ForegroundColor Yellow
                Write-Host '  Get-DataverseConnection -Url ""' + $url + '"" -Interactive -SetAsDefault' -ForegroundColor Cyan
            }}
        }} else {{
            Write-Host 'No access token available, connecting interactively...' -ForegroundColor Yellow
            try {{
                Get-DataverseConnection -Url $url -Interactive -SetAsDefault -ErrorAction Stop
                Write-Host 'Connected successfully!' -ForegroundColor Green
                Write-Host ''
            }} catch {{
                Write-Host ""ERROR: Failed to connect: $($_.Exception.Message)"" -ForegroundColor Red
                Write-Host 'You can try connecting manually with:' -ForegroundColor Yellow
                Write-Host '  Get-DataverseConnection -Interactive -SetAsDefault' -ForegroundColor Cyan
            }}
        }}
    }}
}} catch {{

    Write-Host ""WARNING: Could not read connection information from XrmToolbox: $($_.Exception.Message)"" -ForegroundColor Yellow
    Write-Host 'You can connect manually with:' -ForegroundColor Yellow
    Write-Host '  Get-DataverseConnection -Interactive -SetAsDefault' -ForegroundColor Cyan
}}
";
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
        }

        public void StartScriptSession(string script)
        {
            StartScriptSession(script, connectionInfo);
        }

        public void StartScriptSession(string script, ConnectionInfo connectionInfo)
        {
            StartSession($"Script Session {scriptSessionCounter++}", script, connectionInfo);
        }

        private void StartConEmuSession(string title, string scriptContent)
        {
            ConsoleTabControl consoleTabControl = new ConsoleTabControl();

            TabPage tabPage = CreateConsoleTab(title, consoleTabControl);

            tabControls[tabPage] = consoleTabControl;

            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;

            // Start the session using the new encapsulated method
            consoleTabControl.ProcessExited += (s, e) =>
            {
                this.Invoke((MethodInvoker)delegate {
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

            string tempConnectionFile = null;
            if (connectionInfo != null)
            {
                tempConnectionFile = Path.Combine(Path.GetTempPath(), $"xrmtoolbox-connection-{Guid.NewGuid()}.txt");
                File.WriteAllLines(tempConnectionFile, new[] { $"URL:{connectionInfo.Url}", !string.IsNullOrEmpty(connectionInfo.Token) ? $"TOKEN:{connectionInfo.Token}" : "" });
            }

            string connectionScript = GenerateConnectionScript(bundledModulePath, tempConnectionFile, userScript);

            StartConEmuSession(title, connectionScript);

            // Cleanup temp connection file only — script temp file is cleaned up by ConsoleTabControl
            _ = Task.Delay(30000).ContinueWith(t =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(tempConnectionFile) && File.Exists(tempConnectionFile))
                    {
                        File.Delete(tempConnectionFile);
                    }
                }
                catch
                {
                }
            });
        }

        private void NewInteractiveSessionButton_Click(object sender, EventArgs e)
        {
            if (service != null)
            {
                StartEmbeddedPowerShellConsole(service);
            }
        }
    }
}