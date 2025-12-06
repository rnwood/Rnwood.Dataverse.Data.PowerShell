using McTools.Xrm.Connection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.WebServiceClient;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using XrmToolBox;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPluginHost
{
    /// <summary>
    /// Host process for executing XrmToolbox plugins from PowerShell.
    /// This runs on .NET Framework 4.8 to ensure compatibility with XrmToolbox plugins.
    /// </summary>
    class Program
    {
        private static string _pipeName;
        private static CrmServiceClient _serviceClient;

        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                if (args.Length < 3)
                {
                    Console.Error.WriteLine("Usage: XrmToolboxPluginHost <plugin-directory> <pipe-name> <connection-string>");
                    Console.Error.WriteLine("  plugin-directory: Path to the directory containing the plugin DLLs");
                    Console.Error.WriteLine("  pipe-name: Named pipe to use for token retrieval");
                    Console.Error.WriteLine("  connection-string: Connection string for Dataverse");
                    return 1;
                }

                string pluginDirectory = args[0];
                _pipeName = args[1];
                string connectionString = args[2];

                Console.WriteLine($"XrmToolbox Plugin Host starting...");
                Console.WriteLine($"Plugin directory: {pluginDirectory}");
                Console.WriteLine($"Named pipe: {_pipeName}");

                // Set up assembly redirection for XrmToolbox assemblies
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

                PluginManagerExtended.Instance.Plugins = [];
                PluginManagerExtended.Instance.PluginsExt = [];

                // Load the plugin
                var pluginLoader = new PluginLoader();
                var plugin = pluginLoader.LoadPlugin(pluginDirectory);

                Console.WriteLine($"Plugin loaded: {plugin.GetType().FullName}");

                // Create connection with external token management
                _serviceClient = CreateConnection(connectionString);

                if (_serviceClient == null)
                {
                    Console.Error.WriteLine("Failed to create connection");
                    return 1;
                }

                Console.WriteLine("Connection established");

                // Show the plugin in a form
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var pluginControl = plugin.GetControl();

                if (pluginControl == null)
                {
                    Console.Error.WriteLine("Plugin returned null control");
                    return 1;
                }

                var form = new Form
                {
                    Text = $"XrmToolbox Plugin: {plugin.GetType().Name}",
                    Width = 1200,
                    Height = 800,
                    StartPosition = FormStartPosition.CenterScreen
                };

                Control control = pluginControl as Control;
                if (control != null)
                {
                    control.Dock = DockStyle.Fill;
                    form.Controls.Add(control);
                }

                Debugger.Launch();

                // Use CrmServiceClient for XrmToolbox plugin compatibility
                pluginControl.UpdateConnection(_serviceClient, new ConnectionDetail()
                {
                    AuthType = Microsoft.Xrm.Sdk.Client.AuthenticationProviderType.None,
                    ConnectionName = _serviceClient.ConnectedOrgFriendlyName,
                    EnvironmentText = _serviceClient.ConnectedOrgFriendlyName,
                    OrganizationVersion = _serviceClient.ConnectedOrgVersion?.ToString(),
                    OrganizationDataServiceUrl = _serviceClient.ConnectedOrgPublishedEndpoints?[EndpointType.OrganizationDataService],
                    ConnectionId = Guid.NewGuid(),
                    IsCustomAuth = true,
                    WebApplicationUrl = _serviceClient.ConnectedOrgPublishedEndpoints?[EndpointType.WebApplication],
                    ServiceClient = _serviceClient
                });

                Application.Run(form);

                Console.WriteLine("Plugin host exiting");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        static CrmServiceClient CreateConnection(string connectionString)
        {

            Console.WriteLine("Creating connection...");

            // Parse simple connection string format
            // Expected format: "Url=https://org.crm.dynamics.com"
            var parts = connectionString.Split(';');
            string url = null;

            foreach (var part in parts)
            {
                var keyValue = part.Split(new[] { '=' }, 2);
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();

                    if (key.Equals("Url", StringComparison.OrdinalIgnoreCase))
                    {
                        url = value;
                    }
                }
            }

            if (string.IsNullOrEmpty(url))
            {
                Console.Error.WriteLine("No URL found in connection string");
                return null;
            }

            Console.WriteLine($"Connecting to: {url}");

            // Create CrmServiceClient using the factory
            var client = CrmServiceClientFactory.Create(url, _pipeName);

            // Verify connection by executing WhoAmI request
            client.Execute(new WhoAmIRequest());

            if (client.IsReady)
            {
                Console.WriteLine($"Connected to: {client.ConnectedOrgFriendlyName}");
                return client;
            }
            else
            {
                throw new InvalidOperationException($"Connection failed: {client.LastCrmError}");
            }

        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            // List of assemblies to redirect (same as XrmToolbox)
            var assembliesToRedirect = new[]
            {
                "Newtonsoft.Json",
                "McTools.Xrm.Connection",
                "McTools.Xrm.Connection.WinForms",
                "XrmToolBox.Extensibility",
                "XrmToolBox.ToolLibrary",
                "Microsoft.Xrm.Sdk",
                "Microsoft.Xrm.Sdk.Workflow",
                "Microsoft.Crm.Sdk.Proxy",
                "Microsoft.Xrm.Tooling.Connector",
                "Microsoft.Xrm.Tooling.Ui.Styles",
                "Microsoft.Xrm.Tooling.CrmConnectControl",
                "Microsoft.IdentityModel.Clients.ActiveDirectory",
                "WeifenLuo.WinFormsUI.Docking",
                "WeifenLuo.WinFormsUI.Docking.ThemeVS2015",
                "ScintillaNET",
                "Microsoft.Web.WebView2.Core",
                "Microsoft.Web.WebView2.WinForms",
                "Microsoft.Toolkit.Uwp.Notifications"
            };

            if (assembliesToRedirect.Contains(assemblyName.Name, StringComparer.OrdinalIgnoreCase))
            {
                // Try to load from the current directory
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName.Name + ".dll");
                if (File.Exists(path))
                {
                    Console.WriteLine($"Redirecting assembly load: {assemblyName.Name} -> {path}");
                    return Assembly.LoadFrom(path);
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Factory for creating CrmServiceClient with external token via named pipe.
    /// Uses OrganizationWebProxyClient internally since the older SDK version doesn't have
    /// the token provider constructor pattern.
    /// </summary>
    static class CrmServiceClientFactory
    {
        /// <summary>
        /// Creates a CrmServiceClient using an access token from the named pipe.
        /// </summary>
        /// <param name="url">The Dataverse organization URL</param>
        /// <param name="pipeName">The named pipe to use for token retrieval</param>
        /// <returns>A configured CrmServiceClient</returns>
        public static CrmServiceClient Create(string url, string pipeName)
        {
            string token = GetTokenFromPipe(pipeName);
            
            // Build the organization service URI
            // The URL might be:
            // - Base URL: https://org.crm.dynamics.com/
            // - Full service URL: https://org.crm.dynamics.com/XRMServices/2011/Organization.svc
            // - Web endpoint URL: https://org.crm.dynamics.com/XRMServices/2011/Organization.svc/web
            
            Uri baseUri = new Uri(url);
            Uri orgServiceUri;
            
            string path = baseUri.AbsolutePath.TrimEnd('/');
            
            if (path.EndsWith("/Organization.svc/web", StringComparison.OrdinalIgnoreCase))
            {
                // Already the correct web endpoint
                orgServiceUri = baseUri;
            }
            else if (path.EndsWith("/Organization.svc", StringComparison.OrdinalIgnoreCase))
            {
                // Need to add /web
                orgServiceUri = new Uri(url.TrimEnd('/') + "/web");
            }
            else
            {
                // Base URL - need to add the full path
                string baseUrl = baseUri.GetLeftPart(UriPartial.Authority);
                orgServiceUri = new Uri(baseUrl + "/XRMServices/2011/Organization.svc/web");
            }
            
            Console.WriteLine($"Creating OrganizationWebProxyClient for: {orgServiceUri}");
            
            // Create an OrganizationWebProxyClient with the access token
            var webProxyClient = new OrganizationWebProxyClient(orgServiceUri, false);
            webProxyClient.HeaderToken = token;
            
            Console.WriteLine($"OrganizationWebProxyClient created with token ({token?.Length ?? 0} characters)");
            
            // Create CrmServiceClient from the web proxy client
            var client = new CrmServiceClient(webProxyClient);
            
            Console.WriteLine($"CrmServiceClient created, IsReady: {client.IsReady}");
            
            if (!client.IsReady)
            {
                Console.WriteLine($"Connection not ready, error: {client.LastCrmError}");
                throw new InvalidOperationException($"Failed to create CrmServiceClient: {client.LastCrmError}");
            }
            
            return client;
        }

        private static string GetTokenFromPipe(string pipeName)
        {
            try
            {
                Console.WriteLine("Fetching token from named pipe...");
                using (var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.In))
                {
                    pipeClient.Connect(30000);

                    using (var reader = new StreamReader(pipeClient))
                    {
                        string token = reader.ReadToEnd();
                        Console.WriteLine($"Token received from named pipe ({token?.Length ?? 0} characters)");
                        return token;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve token from named pipe: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Loads XrmToolbox plugins from a directory
    /// </summary>
    class PluginLoader
    {
        public IXrmToolBoxPlugin LoadPlugin(string pluginDirectory)
        {
            if (!Directory.Exists(pluginDirectory))
            {
                throw new DirectoryNotFoundException($"Plugin directory not found: {pluginDirectory}");
            }

            // Get DLLs only in the root directory (ignore subfolders)
            var dllFiles = Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly);

            if (dllFiles.Length != 1)
            {
                throw new InvalidOperationException($"Expected exactly 1 DLL in plugin directory '{pluginDirectory}', but found {dllFiles.Length}.");
            }

            var dllFile = dllFiles[0];
            Console.WriteLine($"Loading plugin from: {Path.GetFileName(dllFile)}");

            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(dllFile);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load assembly from '{dllFile}': {ex.Message}", ex);
            }

            // Find types that implement IXrmToolBoxPlugin
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(IXrmToolBoxPlugin).IsAssignableFrom(t) &&
                           !t.IsAbstract &&
                           !t.IsInterface &&
                           t.GetConstructor(Type.EmptyTypes) != null)
                .ToList();

            if (pluginTypes.Count != 1)
            {
                throw new InvalidOperationException($"Expected exactly 1 constructible plugin type in '{Path.GetFileName(dllFile)}', but found {pluginTypes.Count}.");
            }

            var pluginType = pluginTypes[0];
            Console.WriteLine($"Found plugin type: {pluginType.FullName}");

            try
            {
                var plugin = Activator.CreateInstance(pluginType) as IXrmToolBoxPlugin;
                return plugin;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to instantiate plugin type '{pluginType.FullName}': {ex.Message}", ex);
            }
        }
    }
}
