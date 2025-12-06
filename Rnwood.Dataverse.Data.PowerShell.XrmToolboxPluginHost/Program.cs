using McTools.Xrm.Connection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
                    Console.Error.WriteLine("Usage: XrmToolboxPluginHost <plugin-directory> <pipe-name> <url>");
                    Console.Error.WriteLine("  plugin-directory: Path to the directory containing the plugin DLLs");
                    Console.Error.WriteLine("  pipe-name: Named pipe to use for token retrieval");
                    Console.Error.WriteLine("  url: Dataverse organization URL");
                    return 1;
                }

                string pluginDirectory = args[0];
                _pipeName = args[1];
                string url = args[2];

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
                _serviceClient = CreateConnection(url);

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

                if (pluginControl is IStatusBarMessenger statusBarMessenger)
                {
                    statusBarMessenger.SendMessageToStatusBar += (s, ea) =>
                    {
                        Console.WriteLine($"StatusBar Message: {ea.Message}");
                    };
                }

                if (pluginControl is IMessageBusHost messageBusHost)
                {
                    messageBusHost.OnOutgoingMessage += (s, ea) =>
                    {
                        Console.WriteLine($"MessageBus Message: {ea.TargetArgument}");

                        ea.
                    };
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


                form.Load += (s, ea) =>
                {

                    // Use CrmServiceClient for XrmToolbox plugin compatibility
                    pluginControl.UpdateConnection(_serviceClient, new ConnectionDetail()
                    {
                        AuthType = Microsoft.Xrm.Sdk.Client.AuthenticationProviderType.None,
                        ConnectionName = _serviceClient.ConnectedOrgFriendlyName ?? url,
                        EnvironmentText = _serviceClient.ConnectedOrgFriendlyName ?? url,
                        OrganizationVersion = _serviceClient.ConnectedOrgVersion?.ToString(),
                        OrganizationFriendlyName = _serviceClient.ConnectedOrgFriendlyName ?? "powershell",
                        OrganizationServiceUrl = _serviceClient.ConnectedOrgPublishedEndpoints?[EndpointType.OrganizationService] ?? url,
                        OrganizationDataServiceUrl = _serviceClient.ConnectedOrgPublishedEndpoints?[EndpointType.OrganizationDataService] ?? url,
                        ConnectionId = Guid.NewGuid(),
                        IsCustomAuth = true,
                        WebApplicationUrl = _serviceClient.ConnectedOrgPublishedEndpoints?[EndpointType.WebApplication] ?? new Uri(new Uri(url), "/").ToString(),
                        ServiceClient = _serviceClient,
                        OriginalUrl = _serviceClient.CrmConnectOrgUriActual?.ToString() ?? url
                    });
                };

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

        static CrmServiceClient CreateConnection(string url)
        {

            Console.WriteLine("Creating connection...");

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
}

