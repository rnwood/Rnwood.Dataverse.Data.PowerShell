using System;
using System.IO;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;
using Microsoft.PowerPlatform.Dataverse.Client;
using ConEmu.WinForms;
using System.Security.Cryptography;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class PowerShellConsolePlugin : PluginControlBase, IGitHubPlugin, IPayPalPlugin
    {
        private ConEmuControl conEmuControl;
        private string connectionDataFile;

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
                // Create temporary initialization script
                string tempScriptPath = Path.Combine(Path.GetTempPath(), $"xrmtoolbox-ps-init-{Guid.NewGuid()}.ps1");
                
                // Extract and save connection information from XrmToolbox
                string connectionInfo = ExtractConnectionInformation();
                
                string connectionScript = GenerateConnectionScript(connectionInfo);
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

        private string ExtractConnectionInformation()
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

                // Create a temporary secure file to pass connection info
                connectionDataFile = Path.Combine(Path.GetTempPath(), $"xrmtoolbox-conn-{Guid.NewGuid()}.dat");
                
                var connectionData = new StringBuilder();
                connectionData.AppendLine($"URL:{orgUrl}");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    connectionData.AppendLine($"TOKEN:{accessToken}");
                }

                // Write with restricted permissions
                File.WriteAllText(connectionDataFile, connectionData.ToString());
                
                // Set file permissions to current user only (Windows ACL)
                var fileInfo = new FileInfo(connectionDataFile);
                var fileSecurity = fileInfo.GetAccessControl();
                fileSecurity.SetAccessRuleProtection(true, false); // Remove inherited permissions
                
                var currentUser = System.Security.Principal.WindowsIdentity.GetCurrent();
                var fileAccessRule = new System.Security.AccessControl.FileSystemAccessRule(
                    currentUser.User,
                    System.Security.AccessControl.FileSystemRights.FullControl,
                    System.Security.AccessControl.AccessControlType.Allow);
                fileSecurity.AddAccessRule(fileAccessRule);
                fileInfo.SetAccessControl(fileSecurity);

                return connectionDataFile;
            }
            catch (Exception ex)
            {
                // Could not extract connection information, will fall back to manual connection
                return null;
            }
        }

        private string GenerateConnectionScript(string connectionDataFile)
        {
            string script = @"
Write-Host '================================================' -ForegroundColor Cyan
Write-Host 'Rnwood.Dataverse.Data.PowerShell Console' -ForegroundColor Cyan
Write-Host '================================================' -ForegroundColor Cyan
Write-Host ''
Write-Host 'Loading PowerShell module...' -ForegroundColor Yellow
try {
    Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
    Write-Host 'Module loaded successfully!' -ForegroundColor Green
} catch {
    Write-Host 'ERROR: Failed to load module. Please ensure it is installed:' -ForegroundColor Red
    Write-Host '  Install-Module Rnwood.Dataverse.Data.PowerShell -Scope CurrentUser' -ForegroundColor Yellow
    Write-Host ''
    Write-Host 'Error details:' -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit
}
Write-Host ''
";

            // Add connection logic if we have connection data
            if (!string.IsNullOrEmpty(connectionDataFile))
            {
                script += $@"
Write-Host 'Connecting to Dataverse using XrmToolbox connection...' -ForegroundColor Yellow

$connectionDataFile = '{connectionDataFile.Replace("\\", "\\\\")}'
if (Test-Path $connectionDataFile) {{
    try {{
        $connectionData = Get-Content $connectionDataFile -Raw
        $lines = $connectionData -split ""`n""
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
        
        # Clean up the connection data file immediately
        Remove-Item $connectionDataFile -Force -ErrorAction SilentlyContinue
        
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
        Write-Host ""WARNING: Could not read connection information: $($_.Exception.Message)"" -ForegroundColor Yellow
        Write-Host 'You can connect manually with:' -ForegroundColor Yellow
        Write-Host '  $connection = Get-DataverseConnection -Url ""https://yourorg.crm.dynamics.com"" -Interactive' -ForegroundColor Cyan
    }}
}} else {{
    Write-Host 'Connection data not available. Connect manually with:' -ForegroundColor Yellow
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
            // Clean up connection data file if it exists
            if (!string.IsNullOrEmpty(connectionDataFile) && File.Exists(connectionDataFile))
            {
                try
                {
                    File.Delete(connectionDataFile);
                }
                catch
                {
                    // Ignore errors when deleting
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
