using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using McTools.Xrm.Connection;
using Microsoft.PowerPlatform.Dataverse.Client;
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
        private static ServiceClient _serviceClient;

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

                // Load the plugin
                var pluginLoader = new PluginLoader();
                var plugin = pluginLoader.LoadPlugin(pluginDirectory);

                if (plugin == null)
                {
                    Console.Error.WriteLine("Failed to load plugin from directory");
                    return 1;
                }

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

                // Update connection using reflection to call OnConnectionUpdated
                try
                {
                    // Create a simple event args object since we may not have access to the actual type
                    var eventArgsType = pluginControl.GetType().Assembly.GetType("XrmToolBox.Extensibility.ConnectionUpdatedEventArgs");
                    
                    if (eventArgsType == null)
                    {
                        // Try to find it in loaded assemblies
                        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            eventArgsType = asm.GetType("XrmToolBox.Extensibility.ConnectionUpdatedEventArgs");
                            if (eventArgsType != null) break;
                        }
                    }

                    if (eventArgsType != null)
                    {
                        // Try to find a constructor that accepts ServiceClient or its base types
                        var constructor = eventArgsType.GetConstructor(new[] { typeof(object), typeof(ServiceClient) });
                        
                        if (constructor == null)
                        {
                            // Try with IOrganizationService
                            constructor = eventArgsType.GetConstructor(new[] { typeof(object), typeof(Microsoft.Xrm.Sdk.IOrganizationService) });
                        }
                        
                        if (constructor != null)
                        {
                            var eventArgs = constructor.Invoke(new object[] { null, _serviceClient });
                            
                            var baseType = pluginControl.GetType().BaseType;
                            while (baseType != null && baseType != typeof(object))
                            {
                                var method = baseType.GetMethod("OnConnectionUpdated", 
                                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                                
                                if (method != null)
                                {
                                    method.Invoke(pluginControl, new object[] { eventArgs });
                                    Console.WriteLine("Connection injected successfully");
                                    break;
                                }
                                
                                baseType = baseType.BaseType;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Warning: Could not find suitable constructor for ConnectionUpdatedEventArgs");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Warning: Could not find ConnectionUpdatedEventArgs type");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not inject connection: {ex.Message}");
                }

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

        static ServiceClient CreateConnection(string connectionString)
        {
            try
            {
                Console.WriteLine("Creating connection from connection string...");
                
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

                // Create client with external token management
                // The token provider function will request tokens through the named pipe
                var client = new ServiceClient(
                    instanceUrl: new Uri(url),
                    tokenProviderFunction: async (instanceUrl) =>
                    {
                        Console.WriteLine("Token requested - fetching from named pipe...");
                        return await GetTokenFromPipeAsync();
                    }
                );

                if (client.IsReady)
                {
                    Console.WriteLine($"Connected to: {client.ConnectedOrgFriendlyName}");
                    return client;
                }
                else
                {
                    Console.Error.WriteLine($"Connection failed: {client.LastError}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to create connection: {ex.Message}");
                return null;
            }
        }

        static async Task<string> GetTokenFromPipeAsync()
        {
            try
            {
                // Request token from parent process through named pipe
                using (var pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.In))
                {
                    await pipeClient.ConnectAsync(5000); // 5 second timeout
                    
                    using (var reader = new StreamReader(pipeClient))
                    {
                        string token = await reader.ReadToEndAsync();
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

            // Load all DLLs in the directory
            var dllFiles = Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.AllDirectories);

            Console.WriteLine($"Searching {dllFiles.Length} DLLs for XrmToolbox plugins...");

            // Try to load XrmToolBox assemblies first to ensure they're available
            foreach (var dllFile in dllFiles)
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(dllFile);
                    if (fileName.StartsWith("XrmToolBox", StringComparison.OrdinalIgnoreCase) ||
                        fileName.StartsWith("McTools", StringComparison.OrdinalIgnoreCase))
                    {
                        Assembly.LoadFrom(dllFile);
                        Console.WriteLine($"Pre-loaded: {fileName}");
                    }
                }
                catch
                {
                    // Ignore errors pre-loading
                }
            }

            // Now search for plugin types
            foreach (var dllFile in dllFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dllFile);
                    
                    // Find types that implement IXrmToolBoxPlugin
                    var pluginTypes = assembly.GetTypes()
                        .Where(t => typeof(IXrmToolBoxPlugin).IsAssignableFrom(t) && 
                                   !t.IsAbstract && 
                                   !t.IsInterface &&
                                   t.GetConstructor(Type.EmptyTypes) != null)
                        .ToList();

                    if (pluginTypes.Any())
                    {
                        var pluginType = pluginTypes.First();
                        Console.WriteLine($"Found plugin type: {pluginType.FullName}");
                        
                        var plugin = Activator.CreateInstance(pluginType) as IXrmToolBoxPlugin;
                        return plugin;
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine($"Skipping {Path.GetFileName(dllFile)}: Type load errors");
                    foreach (var loaderEx in ex.LoaderExceptions.Take(3))
                    {
                        if (loaderEx != null)
                        {
                            Console.WriteLine($"  {loaderEx.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Skipping {Path.GetFileName(dllFile)}: {ex.Message}");
                }
            }

            return null;
        }
    }
}
