using System;
using System.IO;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;
using Microsoft.PowerPlatform.Dataverse.Client;
using ConEmu.WinForms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class PowerShellConsolePlugin : PluginControlBase, IGitHubPlugin, IPayPalPlugin
    {
        private ConEmuControl conEmuControl;

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
                string connectionScript = GenerateConnectionScript();
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

        public override void ClosingPlugin(PluginCloseInfo info)
        {
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
