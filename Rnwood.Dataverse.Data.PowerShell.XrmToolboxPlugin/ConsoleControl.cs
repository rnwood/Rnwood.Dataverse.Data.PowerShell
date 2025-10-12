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
        private TabControl tabControl;
        private Dictionary<TabPage, ConEmuControl> conEmuControls = new Dictionary<TabPage, ConEmuControl>();
        private string pipeName;
        private CancellationTokenSource pipeServerCancellation;
        private ConnectionInfo connectionInfo;
        private int scriptSessionCounter = 1;
        private CrmServiceClient service;
        private ToolStrip toolStrip;
        private ToolStripButton newInteractiveSessionButton;

        public ConsoleControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.tabControl = new TabControl();
            this.toolStrip = new ToolStrip();
            this.newInteractiveSessionButton = new ToolStripButton();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newInteractiveSessionButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(800, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "toolStrip";
            // 
            // newInteractiveSessionButton
            // 
            this.newInteractiveSessionButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.newInteractiveSessionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newInteractiveSessionButton.Name = "newInteractiveSessionButton";
            this.newInteractiveSessionButton.Size = new System.Drawing.Size(123, 22);
            this.newInteractiveSessionButton.Text = "New Interactive Session";
            this.newInteractiveSessionButton.Click += new System.EventHandler(this.NewInteractiveSessionButton_Click);
            // 
            // tabControl
            // 
            this.tabControl.Dock = DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 25);
            this.tabControl.Name = "tabControl";
            this.tabControl.Size = new System.Drawing.Size(800, 600);
            this.tabControl.TabIndex = 0;
            // 
            // ConsoleControl
            // 
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.toolStrip);
            this.Name = "ConsoleControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private TabPage CreateConsoleTab(string title, ConEmuControl emuControl)
        {
            TabPage tabPage = new TabPage(title);
            emuControl.Dock = DockStyle.Fill;
            tabPage.Controls.Add(emuControl);
            Button closeButton = new Button();
            closeButton.Text = "X";
            closeButton.Size = new Size(20, 20);
            closeButton.Location = new Point(tabPage.Width - 25, 5);
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.Click += (s, e) => {
                tabControl.TabPages.Remove(tabPage);
                if (conEmuControls.ContainsKey(tabPage)) {
                    conEmuControls[tabPage].Dispose();
                    conEmuControls.Remove(tabPage);
                }
            };
            tabPage.Controls.Add(closeButton);
            closeButton.BringToFront();
            return tabPage;
        }

        public void StartEmbeddedPowerShellConsole(CrmServiceClient service)
        {
            this.service = service;
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
                this.connectionInfo = connectionInfo;

                // Start pipe server in background
                if (connectionInfo != null)
                {
                    Task.Run(() => StartPipeServer(connectionInfo, pipeServerCancellation.Token));
                }

                string connectionScript = GenerateConnectionScript(bundledModulePath, connectionInfo != null ? pipeName : null, "");
                File.WriteAllText(tempScriptPath, connectionScript);

                // Configure ConEmu startup  
                ConEmuStartInfo startInfo = new ConEmuStartInfo();
                startInfo.ConsoleProcessCommandLine = $"powershell.exe -NoExit -ExecutionPolicy Bypass -File \"{tempScriptPath}\"";
                startInfo.StartupDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                startInfo.ConEmuConsoleExtenderExecutablePath = Path.Combine(Assembly.GetExecutingAssembly().Location, "..", "conemu", "conemuc.exe"); // Use default
                var configXml = new System.Xml.XmlDocument();
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.conemu.xml"))
                {
                    configXml.Load(stream);

                }
                startInfo.BaseConfiguration = configXml; // Smaller font

                // Create new ConEmuControl
                ConEmuControl newConEmuControl = new ConEmuControl();
                newConEmuControl.AutoStartInfo = null;

                TabPage tabPage = CreateConsoleTab("Main Console", newConEmuControl);

                conEmuControls[tabPage] = newConEmuControl;

                tabControl.TabPages.Add(tabPage);
                tabControl.SelectedTab = tabPage;

                // Start the embedded PowerShell console
                newConEmuControl.Start(startInfo).ConsoleProcessExited += (s, e) =>
                {
                    this.Invoke((MethodInvoker)delegate {
                        tabControl.TabPages.Remove(tabPage);
                        if (conEmuControls.ContainsKey(tabPage))
                        {
                            conEmuControls[tabPage].Dispose();
                            conEmuControls.Remove(tabPage);
                        }
                    });
                };
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

        private string GenerateConnectionScript(string bundledModulePath, string pipeName, string userScript)
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
Write-Host '  Get-DataverseConnection -Interactive -SetAsDefault' -ForegroundColor Cyan
";
            }


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

            foreach (var emu in conEmuControls.Values)
            {
                if (emu.IsConsoleEmulatorOpen)
                {
                    try
                    {
                        emu.Dispose();
                    }
                    catch
                    {
                        // Ignore errors when disposing
                    }
                }
            }
            conEmuControls.Clear();
        }

        public void StartScriptSession(string script)
        {
            StartScriptSession(script, connectionInfo);
        }

        public void StartScriptSession(string script, ConnectionInfo connectionInfo)
        {
            string bundledModulePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "PSModule");

            string pipeName = null;
            CancellationTokenSource scriptPipeCancellation = null;

            if (connectionInfo != null)
            {
                pipeName = $"XrmToolbox_Script_{Guid.NewGuid()}";
                scriptPipeCancellation = new CancellationTokenSource();
                Task.Run(() => StartPipeServer(connectionInfo, scriptPipeCancellation.Token));
            }

            string connectionScript = GenerateConnectionScript(bundledModulePath, pipeName, script);
            string tempScriptPath = Path.Combine(Path.GetTempPath(), $"xrmtoolbox-script-{Guid.NewGuid()}.ps1");
            File.WriteAllText(tempScriptPath, connectionScript);

            ConEmuStartInfo startInfo = new ConEmuStartInfo();
            startInfo.ConsoleProcessCommandLine = $"powershell.exe -ExecutionPolicy Bypass -NoExit -File \"{tempScriptPath}\"";
            startInfo.StartupDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            startInfo.ConEmuConsoleExtenderExecutablePath = Path.Combine(Assembly.GetExecutingAssembly().Location, "..", "conemu", "conemuc.exe");

            var configXml = new System.Xml.XmlDocument();
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.conemu.xml"))
            {
                if (stream != null)
                {
                    configXml.Load(stream);
                }
                else
                {
                    // Fallback to inline XML if resource not found
                    configXml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<key name=""Software"">
  <key name=""ConEmu"">
    <key name="".Vanilla"">
      <value name=""FontSize"" type=""dword"" data=""00000008""/>
    </key>
  </key>
</key>");
                }
            }
            startInfo.BaseConfiguration = configXml;

            // Create new ConEmuControl
            ConEmuControl newConEmuControl = new ConEmuControl();
            newConEmuControl.AutoStartInfo = null;

            TabPage tabPage = CreateConsoleTab($"Script Session {scriptSessionCounter++}", newConEmuControl);

            conEmuControls[tabPage] = newConEmuControl;

            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;

            // Start the script session
            newConEmuControl.Start(startInfo).ConsoleProcessExited += (s, e) =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    tabControl.TabPages.Remove(tabPage);
                    if (conEmuControls.ContainsKey(tabPage))
                    {
                        conEmuControls[tabPage].Dispose();
                        conEmuControls.Remove(tabPage);
                    }
                });
            };

            // Schedule cleanup
            _ = Task.Delay(30000).ContinueWith(t =>
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