using System;
using System.IO;
using System.Linq;
using System.Reflection;
using XrmToolBox.Extensibility.Interfaces;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPluginHost
{
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

