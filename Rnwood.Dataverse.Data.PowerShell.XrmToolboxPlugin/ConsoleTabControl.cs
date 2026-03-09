using ConEmu.WinForms;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class ConsoleTabControl : UserControl
    {
        public event EventHandler CloseRequested;

        // Raised when the console process exits
        public event EventHandler ProcessExited;

        // The PowerShell version used by this console tab
        private PowerShellVersion _powerShellVersion = PowerShellVersion.Desktop;

        // ConEmuControl kept as a private field via designer
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

        private enum MONITOR_DPI_TYPE
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2
        }

        private const uint MONITOR_DEFAULTTONEAREST = 2;

        public ConsoleTabControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the PowerShell version to use for this console tab.
        /// </summary>
        public PowerShellVersion PowerShellVersion
        {
            get => _powerShellVersion;
            set => _powerShellVersion = value;
        }

        /// <summary>
        /// Starts the embedded ConEmu session using the provided start info.
        /// The control will raise <see cref="ProcessExited"/> when the console process exits.
        /// </summary>
        /// <param name="startInfo">Start info for ConEmu.</param>
        public void StartSession(ConEmuStartInfo startInfo)
        {
            try
            {
                var result = conEmuControl.Start(startInfo);
                if (result != null)
                {
                    result.ConsoleProcessExited += (s, e) =>
                    {
                        try
                        {
                            ProcessExited?.Invoke(this, EventArgs.Empty);
                        }
                        catch
                        {
                            // swallow
                        }
                    };
                }
            }
            catch
            {
                // Do not let failures here crash the host; caller may show UI errors.
            }
        }

        /// <summary>
        /// Convenience method to start a session by providing the script path. Builds the ConEmuStartInfo internally.
        /// </summary>
        /// <param name="title">Tab title (not used by control but kept for parity).</param>
        /// <param name="tempScriptPath">Path to the PowerShell script to run.</param>
        public void StartSessionWithScript(string title, string tempScriptPath)
        {
            try
            {
                string executableName = PowerShellDetector.GetExecutableName(_powerShellVersion);
                
                ConEmuStartInfo startInfo = new ConEmuStartInfo();
                startInfo.ConsoleProcessCommandLine = $"{executableName} -NoLogo -NoExit -ExecutionPolicy Bypass -File \"{tempScriptPath}\"";
                startInfo.StartupDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                startInfo.ConEmuConsoleExtenderExecutablePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "conemu", "conemuc.exe");
                startInfo.SetEnv("POWERSHELL_UPDATECHECK", "Off");

                var configXml = new System.Xml.XmlDocument();
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.conemu.xml"))
                {
                    configXml.Load(stream);
                }

                //Deals with DPI scaling issue with the conemu control when it's running in 
                // XRM Toolbox which is not DPI aware.
                IntPtr monitor = MonitorFromWindow(Handle, MONITOR_DEFAULTTONEAREST);
                uint dpiX, dpiY;
                GetDpiForMonitor(monitor, (int)MONITOR_DPI_TYPE.MDT_RAW_DPI, out dpiX, out dpiY);
                if (dpiX != 96)
                { 
                    // Dynamically set font size - DPI issue
                    var fontSizeNode = configXml.SelectSingleNode("//value[@name='FontSize']");
                    fontSizeNode.Attributes["data"].Value = Math.Floor(12.0 * (96.0/(float)dpiX)).ToString();
                }


                startInfo.BaseConfiguration = configXml;

                StartSession(startInfo);
            }
            catch
            {
                // swallow to avoid crashing host
            }
        }

        /// <summary>
        /// Starts a session by creating a temporary script from provided content, starting ConEmu, and scheduling cleanup of the temp file.
        /// </summary>
        public void StartSessionWithScriptContent(string title, string scriptContent)
        {
            string tempScriptPath = null;
            try
            {
                tempScriptPath = Path.Combine(Path.GetTempPath(), $"xrmtoolbox-{Guid.NewGuid()}.ps1");
                File.WriteAllText(tempScriptPath, scriptContent, Encoding.UTF8);

                StartSessionWithScript(title, tempScriptPath);

                // Schedule cleanup of temp script
                _ = Task.Delay(30000).ContinueWith(t =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(tempScriptPath) && File.Exists(tempScriptPath))
                        {
                            File.Delete(tempScriptPath);
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                });
            }
            catch
            {
                // if writing the temp file failed, attempt to delete if created
                try { if (!string.IsNullOrEmpty(tempScriptPath) && File.Exists(tempScriptPath)) File.Delete(tempScriptPath); } catch { }
            }
        }

        // Named event handler referenced by designer
        private void CloseButton_Click(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Dispose the ConEmu control if it is open.
        /// </summary>
        public void DisposeConsole()
        {
            try
            {
                if (conEmuControl != null && conEmuControl.IsConsoleEmulatorOpen)
                {
                    conEmuControl.Dispose();
                }
            }
            catch
            {
                // ignore
            }
        }

        public void SetConnectionInfo(string orgName, string url)
        {
            orgNameLabel.Text = orgName;
            urlLabel.Text = url;
        }
    }
}