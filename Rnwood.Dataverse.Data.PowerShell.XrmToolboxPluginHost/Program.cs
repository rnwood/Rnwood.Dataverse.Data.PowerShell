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
using System.ComponentModel.Composition;

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
                    Console.Error.WriteLine("Usage: XrmToolboxPluginHost <plugin-directory> <pipe-name> <url> [name]");
                    Console.Error.WriteLine("  plugin-directory: Path to the directory containing the plugin DLLs");
                    Console.Error.WriteLine("  pipe-name: Named pipe to use for token retrieval");
                    Console.Error.WriteLine("  url: Dataverse organization URL");
                    Console.Error.WriteLine("  name: Optional name of the plugin to load if multiple are present");
                    return 1;
                }

                string pluginDirectory = args[0];
                _pipeName = args[1];
                string url = args[2];
                string name = args.Length > 3 && !string.IsNullOrEmpty(args[3]) ? args[3] : null;

                Console.WriteLine($"XrmToolbox Plugin Host starting...");
                Console.WriteLine($"Plugin directory: {pluginDirectory}");
                Console.WriteLine($"Named pipe: {_pipeName}");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Set up assembly redirection for XrmToolbox assemblies
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

                PluginManagerExtended.Instance.Plugins = [];
                PluginManagerExtended.Instance.PluginsExt = [];

                // Load the plugin
                var pluginLoader = new PluginLoader();
                var plugin = pluginLoader.LoadPlugin(pluginDirectory, name);

                Console.WriteLine($"Plugin loaded: {plugin.GetType().FullName}");

                // Get the plugin name for the window title
                string pluginName = GetPluginName(plugin.GetType()) ?? plugin.GetType().Name;

                // Get the plugin image for the window icon
                string pluginImage = GetPluginImage(plugin.GetType());

                // Create connection with external token management
                _serviceClient = CreateConnection(url);

                if (_serviceClient == null)
                {
                    Console.Error.WriteLine("Failed to create connection");
                    return 1;
                }

                Console.WriteLine("Connection established");

                // Get the web application URL for the title
                string webAppUrl = _serviceClient.ConnectedOrgPublishedEndpoints?[EndpointType.WebApplication] ?? new Uri(new Uri(url), "/").ToString();

                // Show the plugin in a form


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

                if (pluginControl is IStatusBarMessager statusBarMessager)
                {
                    statusBarMessager.SendMessageToStatusBar += (s, ea) =>
                    {
                        Console.WriteLine($"StatusBar Message: {ea.Message}");
                    };
                }

                if (pluginControl is IMessageBusHost messageBusHost)
                {
                    messageBusHost.OnOutgoingMessage += (s, ea) =>
                    {
                        Console.WriteLine($"MessageBus Message: {ea.TargetArgument}");
                    };
                }

                if (pluginControl is IDuplicatableTool duplicatableTool)
                {
                    duplicatableTool.DuplicateRequested += (s, ea) =>
                    {
                        Console.WriteLine("Duplicate requested - not supported in this host");
                    };
                }

                pluginControl.OnRequestConnection += (s, ea) =>
                {
                    Console.WriteLine("Plugin requested connection update");
                };

                pluginControl.OnWorkAsync += (s, ea) =>
                {
                    Console.WriteLine("Plugin started async work");
                };

                pluginControl.OnCloseTool += (s, ea) =>
                {
                    Console.WriteLine("Plugin requested close");
                    Application.Exit();
                };


                var form = new Form
                {
                    Text = $"XrmToolbox Plugin: {pluginName} [{webAppUrl}]",
                    Width = 1200,
                    Height = 800,
                    StartPosition = FormStartPosition.CenterScreen
                };

                // Set the plugin icon if available
                if (!string.IsNullOrEmpty(pluginImage))
                {
                    try
                    {
                        var assembly = plugin.GetType().Assembly;
                        using (var stream = new MemoryStream(Convert.FromBase64String(pluginImage)))
                        {
                            using (var bitmap = new System.Drawing.Bitmap(stream))
                            {
                                form.Icon = System.Drawing.Icon.FromHandle(bitmap.GetHicon());
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        // Ignore if can't load the icon
                        Console.WriteLine("Failed to load plugin icon: " + ex.Message);

                    }
                }

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

        private static void PluginControl_OnCloseTool(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void PluginControl_OnWorkAsync(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static string GetPluginName(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(ExportMetadataAttribute), false)
                .Cast<ExportMetadataAttribute>()
                .Where(attr => attr.Name == "Name")
                .Select(attr => attr.Value as string)
                .FirstOrDefault();
            return attrs;
        }

        private static string GetPluginImage(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(ExportMetadataAttribute), false)
                .Cast<ExportMetadataAttribute>()
                .Where(attr => attr.Name == "SmallImageBase64")
                .Select(attr => attr.Value as string)
                .FirstOrDefault();
            return attrs;
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

