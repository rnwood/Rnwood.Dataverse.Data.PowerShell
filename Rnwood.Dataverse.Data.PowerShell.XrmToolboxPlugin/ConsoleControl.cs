using ConEmu.WinForms;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.ComponentModel.Composition;
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
        private ConEmuControl conEmuControl;
        private string pipeName;
        private CancellationTokenSource pipeServerCancellation;

        public ConsoleControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.conEmuControl = new ConEmuControl();
            this.SuspendLayout();

            // conEmuControl
            this.conEmuControl.Dock = DockStyle.Fill;
            this.conEmuControl.Location = new System.Drawing.Point(0, 0);
            this.conEmuControl.Name = "conEmuControl";
            this.conEmuControl.Size = new System.Drawing.Size(800, 600);
            this.conEmuControl.TabIndex = 0;
            this.conEmuControl.AutoStartInfo = null;

            this.Controls.Add(this.conEmuControl);
            this.Name = "ConsoleControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.ResumeLayout(false);
        }

        public void StartEmbeddedPowerShellConsole(CrmServiceClient service)
        {
            try
            {
                // Get the bundled module path
                string pluginDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
                string bundledModulePath = Path.Combine(pluginDirectory, "PSModule");

                // Create temporary initialization script
                string tempScriptPath = Path.Combine(Path.GetTempPath(), $"xrmtoolbox-ps-init-{Guid.NewGuid()}.ps1");

                // Start named pipe server for connection data
                pipeName = $"XrmToolbox_{Guid.NewGuid()}";
                pipeServerCancellation = new CancellationTokenSource();

                // Extract connection information from XrmToolbox
                var connectionInfo = ExtractConnectionInformation(service);

                // Start pipe server in background
                if (connectionInfo != null)
                {
                    Task.Run(() => StartPipeServer(connectionInfo, pipeServerCancellation.Token));
                }

                string connectionScript = GenerateConnectionScript(bundledModulePath, connectionInfo != null ? pipeName : null);
                File.WriteAllText(tempScriptPath, connectionScript);

                // Configure ConEmu startup  
                ConEmuStartInfo startInfo = new ConEmuStartInfo();
                startInfo.ConsoleProcessCommandLine = $"powershell.exe -NoExit -ExecutionPolicy Bypass -File \"{tempScriptPath}\"";
                startInfo.StartupDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                startInfo.ConEmuConsoleExtenderExecutablePath = Path.Combine(Assembly.GetExecutingAssembly().Location, "..", "conemu", "conemuc.exe"); // Use default

                // Start the embedded PowerShell console
                conEmuControl.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start PowerShell console: {ex.Message}.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartPipeServer(ConnectionInfo connectionInfo, CancellationToken cancellationToken)
        {
            try
            {
                using (var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.Out, 1,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                {
                    // Wait for connection with timeout
                    var connectTask = pipeServer.WaitForConnectionAsync(cancellationToken);
                    if (connectTask.Wait(30000, cancellationToken)) // 30 second timeout
                    {
                        using (var writer = new StreamWriter(pipeServer))
                        {
                            writer.WriteLine($"URL:{connectionInfo.Url}");
                            if (!string.IsNullOrEmpty(connectionInfo.Token))
                            {
                                writer.WriteLine($"TOKEN:{connectionInfo.Token}");
                            }
                            writer.Flush();
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Pipe server failed, PowerShell will fall back to manual connection
            }
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

        private string GenerateConnectionScript(string bundledModulePath, string pipeName)
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

            // Add connection logic if we have a pipe name
            if (!string.IsNullOrEmpty(pipeName))
            {
                script += $@"
Write-Host 'Connecting to Dataverse using XrmToolbox connection...' -ForegroundColor Yellow

$pipeName = '{pipeName}'
try {{
    $pipeClient = New-Object System.IO.Pipes.NamedPipeClientStream('.', $pipeName, [System.IO.Pipes.PipeDirection]::In)
    $pipeClient.Connect(5000)  # 5 second timeout
    
    $reader = New-Object System.IO.StreamReader($pipeClient)
    $url = $null
    $token = $null
    
    while ($true) {{
        $line = $reader.ReadLine()
        if ($line -eq $null) {{ break }}
        
        if ($line -match '^URL:(.+)$') {{
            $url = $matches[1].Trim()
        }}
        elseif ($line -match '^TOKEN:(.+)$') {{
            $token = $matches[1].Trim()
        }}
    }}
    
    $reader.Close()
    $pipeClient.Close()
    
    if (!$url -or !$token) {{
        Write-Host ""ERROR: Both URL and access token are required for automatic connection from XrmToolbox."" -ForegroundColor Red
        Write-Host ""You can connect manually with:"" -ForegroundColor Yellow
        Write-Host ""  Get-DataverseConnection -Interactive -SetAsDefault"" -ForegroundColor Cyan
    }} else {{
        Write-Host ""Connecting to: $url"" -ForegroundColor Cyan
        
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
Write-Host '  Get-DataverseConnection -Interactive  -SetAsDefault -ForegroundColor Cyan
";
            }

            script += @"
Remove-Item $MyInvocation.MyCommand.Path -Force -ErrorAction SilentlyContinue
function prompt {
    ""XrmToolbox PS $($executionContext.SessionState.Path.CurrentLocation)> ""
}
";

            return script;
        }

        public void DisposeResources()
        {
            if (pipeServerCancellation != null)
            {
                try
                {
                    pipeServerCancellation.Cancel();
                    pipeServerCancellation.Dispose();
                }
                catch
                {
                    // Ignore errors when cancelling
                }
            }

            if (conEmuControl != null && conEmuControl.IsConsoleEmulatorOpen)
            {
                try
                {
                    conEmuControl.Dispose();
                }
                catch
                {
                    // Ignore errors when disposing
                }
            }
        }
    }
}