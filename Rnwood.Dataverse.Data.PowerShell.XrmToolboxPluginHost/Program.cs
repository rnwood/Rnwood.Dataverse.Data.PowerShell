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
using Microsoft.Xrm.Tooling.Connector;
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
        private static CancellationTokenSource _namedPipeCancellation;
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

                // Load the plugin
                var pluginLoader = new PluginLoader();
                var plugin = pluginLoader.LoadPlugin(pluginDirectory);

                if (plugin == null)
                {
                    Console.Error.WriteLine("Failed to load plugin from directory");
                    return 1;
                }

                Console.WriteLine($"Plugin loaded: {plugin.GetType().FullName}");

                // Create connection
                _serviceClient = CreateConnection(connectionString);

                if (_serviceClient == null)
                {
                    Console.Error.WriteLine("Failed to create connection");
                    return 1;
                }

                Console.WriteLine("Connection established");

                // Start named pipe server for token refresh
                StartNamedPipeServer();

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

                form.FormClosing += (s, e) => StopNamedPipeServer();

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
                        var constructor = eventArgsType.GetConstructor(new[] { typeof(object), typeof(CrmServiceClient) });
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
                StopNamedPipeServer();
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
            try
            {
                Console.WriteLine("Creating connection from connection string...");
                
                // Parse simple connection string format
                // Expected format: "Url=https://org.crm.dynamics.com;AccessToken=token"
                var parts = connectionString.Split(';');
                string url = null;
                string accessToken = null;

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
                        else if (key.Equals("AccessToken", StringComparison.OrdinalIgnoreCase))
                        {
                            accessToken = value;
                        }
                    }
                }

                if (string.IsNullOrEmpty(url))
                {
                    Console.Error.WriteLine("No URL found in connection string");
                    return null;
                }

                Console.WriteLine($"Connecting to: {url}");

                // Create client - will use interactive auth if no token provided
                CrmServiceClient client;
                
                if (!string.IsNullOrEmpty(accessToken) && accessToken != "unknown")
                {
                    // Use provided access token
                    client = new CrmServiceClient($"AuthType=OAuth;Url={url};AccessToken={accessToken};RequireNewInstance=True");
                }
                else
                {
                    // Fall back to interactive auth
                    Console.WriteLine("No access token provided, using interactive authentication");
                    client = new CrmServiceClient($"AuthType=OAuth;Url={url};AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Auto");
                }

                if (client.IsReady)
                {
                    Console.WriteLine($"Connected to: {client.ConnectedOrgFriendlyName}");
                    return client;
                }
                else
                {
                    Console.Error.WriteLine($"Connection failed: {client.LastCrmError}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to create connection: {ex.Message}");
                return null;
            }
        }

        static void StartNamedPipeServer()
        {
            _namedPipeCancellation = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_namedPipeCancellation.Token.IsCancellationRequested)
                {
                    try
                    {
                        using (var pipeServer = new NamedPipeServerStream(
                            _pipeName,
                            PipeDirection.Out,
                            NamedPipeServerStream.MaxAllowedServerInstances,
                            PipeTransmissionMode.Message,
                            PipeOptions.Asynchronous))
                        {
                            // Wait for client connection
                            await pipeServer.WaitForConnectionAsync(_namedPipeCancellation.Token);

                            try
                            {
                                // Extract fresh token
                                string token = _serviceClient?.CurrentAccessToken;

                                if (!string.IsNullOrEmpty(token))
                                {
                                    byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
                                    await pipeServer.WriteAsync(tokenBytes, 0, tokenBytes.Length, _namedPipeCancellation.Token);
                                    await pipeServer.FlushAsync(_namedPipeCancellation.Token);
                                    Console.WriteLine("Token sent via named pipe");
                                }
                            }
                            catch (Exception)
                            {
                                // Ignore errors writing to pipe
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Named pipe error: {ex.Message}");
                        // Ignore errors and retry
                        if (!_namedPipeCancellation.Token.IsCancellationRequested)
                        {
                            await Task.Delay(100);
                        }
                    }
                }
            }, _namedPipeCancellation.Token);

            Console.WriteLine($"Named pipe server started: {_pipeName}");
        }

        static void StopNamedPipeServer()
        {
            _namedPipeCancellation?.Cancel();
            Console.WriteLine("Named pipe server stopped");
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
