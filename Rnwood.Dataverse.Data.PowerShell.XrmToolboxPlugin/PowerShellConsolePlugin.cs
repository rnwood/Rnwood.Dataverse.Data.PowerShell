using System;
using System.IO;
using System.IO.Pipes;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.Management.Automation;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;
using Microsoft.PowerPlatform.Dataverse.Client;
using ConEmu.WinForms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class PowerShellConsolePlugin : PluginControlBase, IGitHubPlugin, IPayPalPlugin
    {
        private ConEmuControl conEmuControl;
        private string pipeName;
        private CancellationTokenSource pipeServerCancellation;
        private Button helpButton;
        private Panel helpPanel;
        private RichTextBox helpTextBox;
        private Button closeHelpButton;
        
        // Script editor components
        private Panel editorPanel;
        private WebView2 editorWebView;
        private Button toggleViewButton;
        private Panel editorToolbar;
        private Button runScriptButton;
        private Button newScriptButton;
        private Button openScriptButton;
        private Button saveScriptButton;
        private bool isEditorView = false;
        private string currentScriptPath = null;

        public PowerShellConsolePlugin()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.conEmuControl = new ConEmuControl();
            this.helpButton = new Button();
            this.helpPanel = new Panel();
            this.helpTextBox = new RichTextBox();
            this.closeHelpButton = new Button();
            
            this.SuspendLayout();
            
            // conEmuControl
            this.conEmuControl.Dock = DockStyle.Fill;
            this.conEmuControl.Location = new System.Drawing.Point(0, 0);
            this.conEmuControl.Name = "conEmuControl";
            this.conEmuControl.Size = new System.Drawing.Size(800, 600);
            this.conEmuControl.TabIndex = 0;
            this.conEmuControl.AutoStartInfo = null;
            
            // helpButton
            this.helpButton.Text = "? Help";
            this.helpButton.Size = new System.Drawing.Size(80, 30);
            this.helpButton.Location = new System.Drawing.Point(10, 10);
            this.helpButton.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.helpButton.ForeColor = System.Drawing.Color.White;
            this.helpButton.FlatStyle = FlatStyle.Flat;
            this.helpButton.FlatAppearance.BorderSize = 0;
            this.helpButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.helpButton.Cursor = Cursors.Hand;
            this.helpButton.TabIndex = 1;
            this.helpButton.Click += HelpButton_Click;
            
            // helpPanel
            this.helpPanel.Visible = false;
            this.helpPanel.Dock = DockStyle.Fill;
            this.helpPanel.BackColor = System.Drawing.Color.White;
            this.helpPanel.BorderStyle = BorderStyle.FixedSingle;
            this.helpPanel.Padding = new Padding(10);
            
            // closeHelpButton
            this.closeHelpButton.Text = "Close";
            this.closeHelpButton.Size = new System.Drawing.Size(100, 35);
            this.closeHelpButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.closeHelpButton.Location = new System.Drawing.Point(this.helpPanel.Width - 120, this.helpPanel.Height - 50);
            this.closeHelpButton.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.closeHelpButton.ForeColor = System.Drawing.Color.White;
            this.closeHelpButton.FlatStyle = FlatStyle.Flat;
            this.closeHelpButton.FlatAppearance.BorderSize = 0;
            this.closeHelpButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.closeHelpButton.Cursor = Cursors.Hand;
            this.closeHelpButton.Click += CloseHelpButton_Click;
            
            // helpTextBox
            this.helpTextBox.ReadOnly = true;
            this.helpTextBox.Dock = DockStyle.Fill;
            this.helpTextBox.BorderStyle = BorderStyle.None;
            this.helpTextBox.BackColor = System.Drawing.Color.White;
            this.helpTextBox.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.helpTextBox.Padding = new Padding(10);
            
            this.helpPanel.Controls.Add(this.helpTextBox);
            this.helpPanel.Controls.Add(this.closeHelpButton);
            this.closeHelpButton.BringToFront();
            
            // toggleViewButton
            this.toggleViewButton = new Button();
            this.toggleViewButton.Text = "📝 Script Editor";
            this.toggleViewButton.Size = new System.Drawing.Size(140, 30);
            this.toggleViewButton.Location = new System.Drawing.Point(this.Width - 150, 10);
            this.toggleViewButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.toggleViewButton.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.toggleViewButton.ForeColor = System.Drawing.Color.White;
            this.toggleViewButton.FlatStyle = FlatStyle.Flat;
            this.toggleViewButton.FlatAppearance.BorderSize = 0;
            this.toggleViewButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toggleViewButton.Cursor = Cursors.Hand;
            this.toggleViewButton.TabIndex = 2;
            this.toggleViewButton.Click += ToggleViewButton_Click;
            
            // editorPanel
            this.editorPanel = new Panel();
            this.editorPanel.Dock = DockStyle.Fill;
            this.editorPanel.Visible = false;
            this.editorPanel.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            
            // editorToolbar
            this.editorToolbar = new Panel();
            this.editorToolbar.Dock = DockStyle.Top;
            this.editorToolbar.Height = 45;
            this.editorToolbar.BackColor = System.Drawing.Color.FromArgb(45, 45, 45);
            this.editorToolbar.Padding = new Padding(5);
            
            // runScriptButton
            this.runScriptButton = new Button();
            this.runScriptButton.Text = "▶ Run (F5)";
            this.runScriptButton.Size = new System.Drawing.Size(100, 35);
            this.runScriptButton.Location = new System.Drawing.Point(5, 5);
            this.runScriptButton.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.runScriptButton.ForeColor = System.Drawing.Color.White;
            this.runScriptButton.FlatStyle = FlatStyle.Flat;
            this.runScriptButton.FlatAppearance.BorderSize = 0;
            this.runScriptButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.runScriptButton.Cursor = Cursors.Hand;
            this.runScriptButton.Click += RunScriptButton_Click;
            
            // newScriptButton
            this.newScriptButton = new Button();
            this.newScriptButton.Text = "📄 New";
            this.newScriptButton.Size = new System.Drawing.Size(80, 35);
            this.newScriptButton.Location = new System.Drawing.Point(110, 5);
            this.newScriptButton.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.newScriptButton.ForeColor = System.Drawing.Color.White;
            this.newScriptButton.FlatStyle = FlatStyle.Flat;
            this.newScriptButton.FlatAppearance.BorderSize = 0;
            this.newScriptButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.newScriptButton.Cursor = Cursors.Hand;
            this.newScriptButton.Click += NewScriptButton_Click;
            
            // openScriptButton
            this.openScriptButton = new Button();
            this.openScriptButton.Text = "📁 Open";
            this.openScriptButton.Size = new System.Drawing.Size(80, 35);
            this.openScriptButton.Location = new System.Drawing.Point(195, 5);
            this.openScriptButton.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.openScriptButton.ForeColor = System.Drawing.Color.White;
            this.openScriptButton.FlatStyle = FlatStyle.Flat;
            this.openScriptButton.FlatAppearance.BorderSize = 0;
            this.openScriptButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.openScriptButton.Cursor = Cursors.Hand;
            this.openScriptButton.Click += OpenScriptButton_Click;
            
            // saveScriptButton
            this.saveScriptButton = new Button();
            this.saveScriptButton.Text = "💾 Save";
            this.saveScriptButton.Size = new System.Drawing.Size(80, 35);
            this.saveScriptButton.Location = new System.Drawing.Point(280, 5);
            this.saveScriptButton.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.saveScriptButton.ForeColor = System.Drawing.Color.White;
            this.saveScriptButton.FlatStyle = FlatStyle.Flat;
            this.saveScriptButton.FlatAppearance.BorderSize = 0;
            this.saveScriptButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.saveScriptButton.Cursor = Cursors.Hand;
            this.saveScriptButton.Click += SaveScriptButton_Click;
            
            this.editorToolbar.Controls.Add(this.runScriptButton);
            this.editorToolbar.Controls.Add(this.newScriptButton);
            this.editorToolbar.Controls.Add(this.openScriptButton);
            this.editorToolbar.Controls.Add(this.saveScriptButton);
            
            // editorWebView
            this.editorWebView = new WebView2();
            this.editorWebView.Dock = DockStyle.Fill;
            
            this.editorPanel.Controls.Add(this.editorWebView);
            this.editorPanel.Controls.Add(this.editorToolbar);
            
            // PowerShellConsolePlugin
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.conEmuControl);
            this.Controls.Add(this.editorPanel);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.toggleViewButton);
            this.Controls.Add(this.helpPanel);
            this.helpButton.BringToFront();
            this.toggleViewButton.BringToFront();
            this.helpPanel.BringToFront();
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
                InitializeMonacoEditor();
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
            $global:connection = Get-DataverseConnection -ConnectionString $connectionString -SetAsDefault -ErrorAction Stop
            
            if ($global:connection) {{
                Write-Host 'Connected successfully!' -ForegroundColor Green
                Write-Host 'Connection available in $connection variable and set as default' -ForegroundColor Cyan
                Write-Host 'You can now omit -Connection parameter in cmdlets!' -ForegroundColor Cyan
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
                Write-Host '  $connection = Get-DataverseConnection -Url ""' + $url + '"" -Interactive -SetAsDefault' -ForegroundColor Cyan
            }}
        }} catch {{
            Write-Host ""ERROR: Failed to connect: $($_.Exception.Message)"" -ForegroundColor Red
            Write-Host 'You can try connecting manually with:' -ForegroundColor Yellow
            Write-Host '  $connection = Get-DataverseConnection -Url ""' + $url + '"" -Interactive -SetAsDefault' -ForegroundColor Cyan
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

        private void HelpButton_Click(object sender, EventArgs e)
        {
            LoadAndShowHelp();
        }

        private void CloseHelpButton_Click(object sender, EventArgs e)
        {
            helpPanel.Visible = false;
        }

        private void LoadAndShowHelp()
        {
            try
            {
                // Load the embedded markdown file
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.GettingStarted.md";
                
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string markdownContent = reader.ReadToEnd();
                            
                            // Convert markdown to formatted text (basic conversion)
                            string formattedText = ConvertMarkdownToRichText(markdownContent);
                            
                            helpTextBox.Rtf = formattedText;
                            helpPanel.Visible = true;
                        }
                    }
                    else
                    {
                        // Fallback content if resource not found
                        helpTextBox.Text = @"Getting Started

The PowerShell console is now loaded with the Rnwood.Dataverse.Data.PowerShell module.

Quick Commands:
- Get-DataverseRecord -Connection $connection -TableName account
- Get-Help Get-DataverseRecord -Full
- Get-Command -Module Rnwood.Dataverse.Data.PowerShell

For full documentation, visit:
https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell";
                        helpPanel.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load help: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ConvertMarkdownToRichText(string markdown)
        {
            // Basic markdown to RTF conversion
            var rtf = new StringBuilder();
            rtf.AppendLine(@"{\rtf1\ansi\deff0");
            rtf.AppendLine(@"{\fonttbl{\f0 Segoe UI;}{\f1 Consolas;}}");
            rtf.AppendLine(@"{\colortbl;\red0\green0\blue0;\red0\green120\blue215;\red50\green50\blue50;\red200\green200\blue200;}");
            
            var lines = markdown.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            bool inCodeBlock = false;
            
            foreach (var line in lines)
            {
                if (line.StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;
                    continue;
                }
                
                if (inCodeBlock)
                {
                    // Code block - use monospace font and gray background
                    rtf.AppendLine(@"\f1\fs18\cf3 " + EscapeRtf(line) + @"\par");
                }
                else if (line.StartsWith("# "))
                {
                    // H1 - Large bold
                    rtf.AppendLine(@"\f0\fs28\b " + EscapeRtf(line.Substring(2)) + @"\b0\fs20\par");
                }
                else if (line.StartsWith("## "))
                {
                    // H2 - Medium bold
                    rtf.AppendLine(@"\par\f0\fs24\b " + EscapeRtf(line.Substring(3)) + @"\b0\fs20\par");
                }
                else if (line.StartsWith("### "))
                {
                    // H3 - Small bold
                    rtf.AppendLine(@"\par\f0\fs22\b " + EscapeRtf(line.Substring(4)) + @"\b0\fs20\par");
                }
                else if (line.StartsWith("- ") || line.StartsWith("* "))
                {
                    // Bullet point
                    rtf.AppendLine(@"\f0\fs20 \bullet  " + EscapeRtf(line.Substring(2)) + @"\par");
                }
                else if (line.Contains("]("))
                {
                    // Link - extract text and make it blue
                    var linkText = System.Text.RegularExpressions.Regex.Replace(line, @"\[([^\]]+)\]\([^\)]+\)", @"\cf2\ul $1\cf1\ulnone");
                    rtf.AppendLine(@"\f0\fs20 " + EscapeRtf(linkText) + @"\par");
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    // Regular text
                    rtf.AppendLine(@"\f0\fs20 " + EscapeRtf(line) + @"\par");
                }
                else
                {
                    // Empty line
                    rtf.AppendLine(@"\par");
                }
            }
            
            rtf.AppendLine("}");
            return rtf.ToString();
        }

        private string EscapeRtf(string text)
        {
            return text.Replace("\\", "\\\\")
                      .Replace("{", "\\{")
                      .Replace("}", "\\}")
                      .Replace("\n", "\\par\n");
        }

        // ========== Script Editor Methods ==========
        
        private async void InitializeMonacoEditor()
        {
            try
            {
                await editorWebView.EnsureCoreWebView2Async(null);
                
                // Load Monaco editor HTML
                string monacoHtml = GenerateMonacoEditorHtml();
                editorWebView.NavigateToString(monacoHtml);
                
                // Setup message handler for script operations
                editorWebView.WebMessageReceived += EditorWebView_WebMessageReceived;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize script editor: {ex.Message}\n\nWebView2 Runtime may not be installed.", 
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string GenerateMonacoEditorHtml()
        {
            // Get PowerShell cmdlets for IntelliSense
            string cmdletCompletions = GetPowerShellCmdletCompletions();
            
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <style>
        body {{ margin: 0; padding: 0; overflow: hidden; }}
        #container {{ width: 100%; height: 100vh; }}
    </style>
</head>
<body>
    <div id=""container""></div>
    
    <script src=""https://unpkg.com/monaco-editor@0.45.0/min/vs/loader.js""></script>
    <script>
        require.config({{ paths: {{ vs: 'https://unpkg.com/monaco-editor@0.45.0/min/vs' }} }});
        
        require(['vs/editor/editor.main'], function() {{
            // Create Monaco editor
            window.editor = monaco.editor.create(document.getElementById('container'), {{
                value: '# PowerShell Script\\n# Type your PowerShell commands here\\n# Press F5 or click Run to execute\\n\\n',
                language: 'powershell',
                theme: 'vs-dark',
                automaticLayout: true,
                fontSize: 14,
                minimap: {{ enabled: true }},
                scrollBeyondLastLine: false,
                wordWrap: 'on',
                lineNumbers: 'on',
                folding: true,
                renderWhitespace: 'selection'
            }});
            
            // Register PowerShell cmdlet completions
            var cmdlets = {cmdletCompletions};
            
            monaco.languages.registerCompletionItemProvider('powershell', {{
                provideCompletionItems: function(model, position) {{
                    var word = model.getWordUntilPosition(position);
                    var range = {{
                        startLineNumber: position.lineNumber,
                        endLineNumber: position.lineNumber,
                        startColumn: word.startColumn,
                        endColumn: word.endColumn
                    }};
                    
                    return {{
                        suggestions: cmdlets.map(c => ({{
                            label: c.label,
                            kind: monaco.languages.CompletionItemKind.Function,
                            documentation: c.documentation,
                            insertText: c.insertText,
                            range: range
                        }}))
                    }};
                }}
            }});
            
            // Add keyboard shortcuts
            editor.addCommand(monaco.KeyCode.F5, function() {{
                window.chrome.webview.postMessage({{ action: 'run' }});
            }});
            
            editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyS, function() {{
                window.chrome.webview.postMessage({{ action: 'save' }});
            }});
            
            editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyN, function() {{
                window.chrome.webview.postMessage({{ action: 'new' }});
            }});
            
            // Notify ready
            window.chrome.webview.postMessage({{ action: 'ready' }});
        }});
        
        // Handle get content requests
        function getContent() {{
            return editor.getValue();
        }}
        
        function setContent(content) {{
            editor.setValue(content);
        }}
    </script>
</body>
</html>";
        }

        private string GetPowerShellCmdletCompletions()
        {
            try
            {
                StringBuilder completions = new StringBuilder("[");
                
                // Get cmdlets from the Cmdlets assembly
                var cmdletsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Rnwood.Dataverse.Data.PowerShell.Cmdlets");
                
                if (cmdletsAssembly != null)
                {
                    var cmdletTypes = cmdletsAssembly.GetTypes()
                        .Where(t => t.Name.EndsWith("Cmdlet") && !t.IsAbstract && t.IsPublic);
                    
                    bool first = true;
                    foreach (var type in cmdletTypes)
                    {
                        try
                        {
                            var cmdletAttr = type.GetCustomAttributes(typeof(CmdletAttribute), false)
                                .FirstOrDefault() as CmdletAttribute;
                            
                            if (cmdletAttr != null)
                            {
                                if (!first) completions.Append(",");
                                first = false;
                                
                                string cmdletName = $"{cmdletAttr.VerbName}-{cmdletAttr.NounName}";
                                var parameters = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(p => p.GetCustomAttributes(typeof(ParameterAttribute), false).Any())
                                    .Select(p => $"-{p.Name}")
                                    .Take(5); // Limit to first 5 parameters
                                
                                string paramList = string.Join(", ", parameters);
                                completions.Append($"{{label:'{cmdletName}',insertText:'{cmdletName} ',documentation:'Parameters: {paramList}'}}");
                            }
                        }
                        catch
                        {
                            // Skip cmdlets that cause issues
                        }
                    }
                }
                
                // Add common PowerShell cmdlets
                string[] commonCmdlets = new string[] {
                    "Write-Host", "Write-Output", "Write-Verbose", "Write-Warning", "Write-Error",
                    "Get-Variable", "Set-Variable", "ForEach-Object", "Where-Object",
                    "Select-Object", "Sort-Object", "Group-Object", "Measure-Object",
                    "Import-Module", "Get-Module", "Get-Command", "Get-Help"
                };
                
                foreach (var cmdlet in commonCmdlets)
                {
                    completions.Append($",{{label:'{cmdlet}',insertText:'{cmdlet} ',documentation:'PowerShell built-in cmdlet'}}");
                }
                
                completions.Append("]");
                return completions.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting cmdlet completions: {ex.Message}");
                return "[]";
            }
        }

        private void EditorWebView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string message = e.TryGetWebMessageAsString();
                // Parse JSON message (simple parsing for action)
                if (message.Contains("\"action\":\"run\""))
                {
                    RunScriptButton_Click(null, null);
                }
                else if (message.Contains("\"action\":\"save\""))
                {
                    SaveScriptButton_Click(null, null);
                }
                else if (message.Contains("\"action\":\"new\""))
                {
                    NewScriptButton_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling web message: {ex.Message}");
            }
        }

        private void ToggleViewButton_Click(object sender, EventArgs e)
        {
            isEditorView = !isEditorView;
            
            if (isEditorView)
            {
                // Switch to editor view
                conEmuControl.Visible = false;
                editorPanel.Visible = true;
                toggleViewButton.Text = "💻 Console";
            }
            else
            {
                // Switch to console view
                editorPanel.Visible = false;
                conEmuControl.Visible = true;
                toggleViewButton.Text = "📝 Script Editor";
            }
        }

        private async void RunScriptButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Get script content from Monaco editor
                string script = await editorWebView.ExecuteScriptAsync("getContent()");
                
                // Remove JSON string quotes
                script = script.Trim('"').Replace("\\n", "\n").Replace("\\r", "\r")
                    .Replace("\\\"", "\"").Replace("\\\\", "\\");
                
                if (string.IsNullOrWhiteSpace(script))
                {
                    MessageBox.Show("Script is empty. Please enter some PowerShell commands.", 
                        "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                // Switch to console view
                ToggleViewButton_Click(null, null);
                
                // Give console time to become visible
                await Task.Delay(100);
                
                // Create temporary script file
                string tempScriptPath = Path.Combine(Path.GetTempPath(), $"xrmtoolbox-script-{Guid.NewGuid()}.ps1");
                File.WriteAllText(tempScriptPath, script);
                
                // The ConEmu control doesn't expose direct input methods
                // Instead, we'll write the script to a file and have the user run it manually
                // Or we can use a more sophisticated approach with named pipes
                
                // For now, display message to user
                MessageBox.Show($"Script ready to execute.\n\nRun this command in the console:\n& '{tempScriptPath}'",
                    "Script Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Schedule cleanup after some time
                Task.Delay(30000).ContinueWith(t =>
                {
                    try
                    {
                        if (File.Exists(tempScriptPath))
                        {
                            File.Delete(tempScriptPath);
                        }
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to run script: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NewScriptButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Create a new script? Any unsaved changes will be lost.", 
                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    editorWebView.ExecuteScriptAsync("setContent('# PowerShell Script\\n# Type your PowerShell commands here\\n# Press F5 or click Run to execute\\n\\n')");
                    currentScriptPath = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create new script: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void OpenScriptButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Filter = "PowerShell Scripts (*.ps1)|*.ps1|All Files (*.*)|*.*";
                    dialog.Title = "Open PowerShell Script";
                    
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string content = File.ReadAllText(dialog.FileName);
                        
                        // Escape for JavaScript
                        content = content.Replace("\\", "\\\\")
                                       .Replace("'", "\\'")
                                       .Replace("\n", "\\n")
                                       .Replace("\r", "\\r")
                                       .Replace("\"", "\\\"");
                        
                        await editorWebView.ExecuteScriptAsync($"setContent('{content}')");
                        currentScriptPath = dialog.FileName;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open script: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SaveScriptButton_Click(object sender, EventArgs e)
        {
            try
            {
                string scriptPath = currentScriptPath;
                
                if (string.IsNullOrEmpty(scriptPath))
                {
                    using (SaveFileDialog dialog = new SaveFileDialog())
                    {
                        dialog.Filter = "PowerShell Scripts (*.ps1)|*.ps1|All Files (*.*)|*.*";
                        dialog.Title = "Save PowerShell Script";
                        dialog.DefaultExt = "ps1";
                        
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            scriptPath = dialog.FileName;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                
                // Get script content
                string script = await editorWebView.ExecuteScriptAsync("getContent()");
                
                // Remove JSON string quotes and unescape
                script = script.Trim('"').Replace("\\n", "\n").Replace("\\r", "\r")
                    .Replace("\\\"", "\"").Replace("\\\\", "\\");
                
                File.WriteAllText(scriptPath, script);
                currentScriptPath = scriptPath;
                
                MessageBox.Show($"Script saved to: {scriptPath}", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save script: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
