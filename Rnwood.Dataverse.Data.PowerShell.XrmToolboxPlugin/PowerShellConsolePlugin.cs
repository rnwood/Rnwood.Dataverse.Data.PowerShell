using System;
using System.IO;
using System.IO.Pipes;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;
using Microsoft.PowerPlatform.Dataverse.Client;
using ConEmu.WinForms;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class PowerShellConsolePlugin : PluginControlBase, IGitHubPlugin, IPayPalPlugin
    {
        private ConEmuControl conEmuControl;
        private string pipeName;
        private CancellationTokenSource pipeServerCancellation;

        public PowerShellConsolePlugin()
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
            
            // PowerShellConsolePlugin
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.conEmuControl);
            this.Name = "PowerShellConsolePlugin";
            this.Size = new System.Drawing.Size(800, 600);
            this.Load += PowerShellConsolePlugin_Load;
            this.ResumeLayout(false);
        }

        public string RepositoryName => "Rnwood.Dataverse.Data.PowerShell";
        public string UserName => "rnwood";
        public string DonationDescription => "Support development of this PowerShell module";
        public string EmailAccount => "rob@rnwood.co.uk";

        private void PowerShellConsolePlugin_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                StartEmbeddedPowerShellConsole();
            }
        }

        private void StartEmbeddedPowerShellConsole()
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
                var connectionInfo = ExtractConnectionInformation();
                
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
                startInfo.ConEmuConsoleExtenderExecutablePath = null; // Use default
                
                // Start the embedded PowerShell console
                conEmuControl.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start PowerShell console: {ex.Message}\n\nPlease ensure ConEmu is installed or the ConEmu.Control package is properly configured.", 
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

        private class ConnectionInfo
        {
            public string Url { get; set; }
            public string Token { get; set; }
        }

        private ConnectionInfo ExtractConnectionInformation()
        {
            try
            {
                if (Service == null)
                {
                    return null;
                }

                ServiceClient client = Service as ServiceClient;
                if (client == null)
                {
                    return null;
                }

                // Extract connection URL
                string orgUrl = client.ConnectedOrgUriActual?.ToString();
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
                    // Use reflection to access the internal access token
                    var currentAccessTokenField = client.GetType().GetProperty("CurrentAccessToken", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic);
                    
                    if (currentAccessTokenField != null)
                    {
                        accessToken = currentAccessTokenField.GetValue(client) as string;
                    }
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
            catch (Exception ex)
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

if (Test-Path $bundledModulePath) {{
    try {{
        $env:PSModulePath = ""$bundledModulePath;$env:PSModulePath""
        Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        Write-Host 'Module loaded from bundled location!' -ForegroundColor Green
        $moduleLoaded = $true
    }} catch {{
        Write-Host ""Warning: Could not load bundled module: $($_.Exception.Message)"" -ForegroundColor Yellow
    }}
}}

# Fall back to installed module
if (-not $moduleLoaded) {{
    try {{
        Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        Write-Host 'Module loaded from installed location!' -ForegroundColor Green
        $moduleLoaded = $true
    }} catch {{
        Write-Host 'ERROR: Failed to load module from either bundled or installed location.' -ForegroundColor Red
        Write-Host ''
        Write-Host 'The module is not installed. To install it, run:' -ForegroundColor Yellow
        Write-Host '  Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser' -ForegroundColor Yellow
        Write-Host ''
        Write-Host 'Error details:' -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host ''
        Write-Host 'Press any key to exit...' -ForegroundColor Yellow
        $null = $host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
        exit 1
    }}
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
    
    if ($url) {{
        Write-Host ""Connecting to: $url"" -ForegroundColor Cyan
        
        # Build connection string
        if ($token) {{
            # Use OAuth token if available
            $connectionString = ""AuthType=OAuth;Url=$url;AccessToken=$token""
            Write-Host 'Using OAuth token from XrmToolbox...' -ForegroundColor Yellow
        }} else {{
            # Fall back to interactive authentication
            $connectionString = ""Url=$url""
            Write-Host 'OAuth token not available, will use interactive authentication...' -ForegroundColor Yellow
        }}
        
        try {{
            $global:connection = Get-DataverseConnection -ConnectionString $connectionString -ErrorAction Stop
            
            if ($global:connection) {{
                Write-Host 'Connected successfully!' -ForegroundColor Green
                Write-Host 'Connection available in $connection variable' -ForegroundColor Cyan
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
            }} else {{
                Write-Host 'WARNING: Connection could not be established' -ForegroundColor Yellow
                Write-Host 'You can create a new connection with:' -ForegroundColor Yellow
                Write-Host '  $connection = Get-DataverseConnection -Url ""' + $url + '"" -Interactive' -ForegroundColor Cyan
            }}
        }} catch {{
            Write-Host ""ERROR: Failed to connect: $($_.Exception.Message)"" -ForegroundColor Red
            Write-Host 'You can try connecting manually with:' -ForegroundColor Yellow
            Write-Host '  $connection = Get-DataverseConnection -Url ""' + $url + '"" -Interactive' -ForegroundColor Cyan
        }}
    }}
}} catch {{
    Write-Host ""WARNING: Could not read connection information from pipe: $($_.Exception.Message)"" -ForegroundColor Yellow
    Write-Host 'You can connect manually with:' -ForegroundColor Yellow
    Write-Host '  $connection = Get-DataverseConnection -Url ""https://yourorg.crm.dynamics.com"" -Interactive' -ForegroundColor Cyan
}}
";
            }
            else
            {
                script += @"
Write-Host 'To connect to your Dataverse environment, run:' -ForegroundColor Yellow
Write-Host '  $connection = Get-DataverseConnection -Url ""https://yourorg.crm.dynamics.com"" -Interactive' -ForegroundColor Cyan
";
            }

            script += @"
Write-Host ''
Write-Host '================================================' -ForegroundColor Cyan
Write-Host 'Quick Start Examples:' -ForegroundColor Cyan
Write-Host '================================================' -ForegroundColor Cyan
Write-Host ''
Write-Host '# Query records' -ForegroundColor White
Write-Host 'Get-DataverseRecord -Connection $connection -TableName account' -ForegroundColor Gray
Write-Host ''
Write-Host '# Create a record' -ForegroundColor White
Write-Host '$newAccount = @{ name = ""Contoso"" }' -ForegroundColor Gray
Write-Host 'Set-DataverseRecord -Connection $connection -TableName account -Record $newAccount' -ForegroundColor Gray
Write-Host ''
Write-Host '# Use SQL queries' -ForegroundColor White
Write-Host 'Invoke-DataverseSql -Connection $connection -Sql ""SELECT TOP 10 name, accountnumber FROM account""' -ForegroundColor Gray
Write-Host ''
Write-Host '# Get help' -ForegroundColor White
Write-Host 'Get-Help Get-DataverseRecord -Full' -ForegroundColor Gray
Write-Host ''
Write-Host '================================================' -ForegroundColor Cyan
Write-Host ''
Remove-Item $MyInvocation.MyCommand.Path -Force -ErrorAction SilentlyContinue
function prompt {
    ""XrmToolbox PS $($executionContext.SessionState.Path.CurrentLocation)> ""
}
";

            return script;
        }

        public override void ClosingPlugin(PluginCloseInfo info)
        {
            // Cancel pipe server if running
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

            // ConEmu control will handle cleanup automatically
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

            base.ClosingPlugin(info);
        }
    }
}
