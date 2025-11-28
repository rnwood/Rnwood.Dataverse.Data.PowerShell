using System;
using System.Windows.Forms;
using ConEmu.WinForms;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    public partial class ConsoleTabControl : UserControl
    {
        public event EventHandler CloseRequested;

        // Raised when the console process exits
        public event EventHandler ProcessExited;

        // ConEmuControl kept as a private field via designer
        public ConsoleTabControl()
        {
            InitializeComponent();
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
                ConEmuStartInfo startInfo = new ConEmuStartInfo();
                startInfo.ConsoleProcessCommandLine = $"powershell.exe -NoLogo -NoExit -ExecutionPolicy Bypass -File \"{tempScriptPath}\"";
                startInfo.StartupDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                startInfo.ConEmuConsoleExtenderExecutablePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "conemu", "conemuc.exe");
                startInfo.SetEnv("POWERSHELL_UPDATECHECK", "Off");

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
                        configXml.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><key name=\"Software\"><key name=\"ConEmu\"><key name=\"Vanilla\"><value name=\"FontSize\" type=\"dword\" data=\"00000008\"/></key></key></key>");
                    }
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