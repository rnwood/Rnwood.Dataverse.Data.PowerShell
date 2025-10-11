using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class PowerShellConsolePlugin : PluginControlBase, IGitHubPlugin, IPayPalPlugin
    {
        private Process powershellProcess;
        private Panel consolePanel;

        public PowerShellConsolePlugin()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.consolePanel = new Panel();
            this.SuspendLayout();
            
            this.consolePanel.Dock = DockStyle.Fill;
            this.consolePanel.Location = new System.Drawing.Point(0, 0);
            this.consolePanel.Name = "consolePanel";
            this.consolePanel.Size = new System.Drawing.Size(800, 600);
            this.consolePanel.TabIndex = 0;
            
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.consolePanel);
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
                StartPowerShellConsole();
            }
        }

        private void StartPowerShellConsole()
        {
            try
            {
                string tempScriptPath = Path.Combine(Path.GetTempPath(), $"xrmtoolbox-ps-init-{Guid.NewGuid()}.ps1");
                
                string connectionScript = GenerateConnectionScript();
                File.WriteAllText(tempScriptPath, connectionScript);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoExit -ExecutionPolicy Bypass -File \"{tempScriptPath}\"",
                    UseShellExecute = true
                };

                string conEmuPath = FindConEmu();
                if (!string.IsNullOrEmpty(conEmuPath))
                {
                    psi.FileName = conEmuPath;
                    psi.Arguments = $"-run powershell.exe -NoExit -ExecutionPolicy Bypass -File \"{tempScriptPath}\"";
                }

                powershellProcess = new Process { StartInfo = psi };
                powershellProcess.EnableRaisingEvents = true;
                powershellProcess.Exited += PowershellProcess_Exited;
                
                powershellProcess.Start();

                Label infoLabel = new Label
                {
                    Text = "PowerShell console has been launched in a separate window.\n\n" +
                           "The Rnwood.Dataverse.Data.PowerShell module is pre-loaded.\n\n" +
                           "To connect to the same environment as XrmToolbox:\n" +
                           "  $connection = Get-DataverseConnection -Url \"<your-org-url>\" -Interactive\n\n" +
                           "Then you can query records:\n" +
                           "  Get-DataverseRecord -Connection $connection -TableName account\n\n" +
                           "Close this tab to terminate the PowerShell session.",
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                    Font = new System.Drawing.Font("Segoe UI", 10F)
                };
                consolePanel.Controls.Add(infoLabel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start PowerShell console: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FindConEmu()
        {
            string[] possiblePaths = new[]
            {
                @"C:\Program Files\ConEmu\ConEmu64.exe",
                @"C:\Program Files (x86)\ConEmu\ConEmu.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"ConEmu\ConEmu64.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"ConEmu\ConEmu64.exe")
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private string GenerateConnectionScript()
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
Write-Host 'To connect to your Dataverse environment, run:' -ForegroundColor Yellow
Write-Host '  $connection = Get-DataverseConnection -Url ""https://yourorg.crm.dynamics.com"" -Interactive' -ForegroundColor Cyan
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

        private void PowershellProcess_Exited(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => PowershellProcess_Exited(sender, e)));
                return;
            }

            if (consolePanel.Controls.Count > 0 && consolePanel.Controls[0] is Label label)
            {
                label.Text = "PowerShell console has been closed.\n\nClose this tab or restart the plugin to start a new session.";
            }
        }

        public override void ClosingPlugin(PluginCloseInfo info)
        {
            if (powershellProcess != null && !powershellProcess.HasExited)
            {
                try
                {
                    powershellProcess.Kill();
                }
                catch
                {
                    // Ignore errors when killing process
                }
            }

            base.ClosingPlugin(info);
        }
    }
}
