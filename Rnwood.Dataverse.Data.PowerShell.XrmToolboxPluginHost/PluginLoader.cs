using System;
using System.IO;
using System.Linq;
using System.Reflection;
using XrmToolBox.Extensibility.Interfaces;
using System.ComponentModel.Composition;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPluginHost
{
    /// <summary>
    /// Loads XrmToolbox plugins from a directory
    /// </summary>
    class PluginLoader
    {
        public IXrmToolBoxPlugin LoadPlugin(string pluginDirectory, string name = null)
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

            if (pluginTypes.Count == 0)
            {
                throw new InvalidOperationException($"No constructible plugin types found in '{Path.GetFileName(dllFile)}'.");
            }

            Type pluginType = null;

            // Helper to format type for messages
            string FormatType(Type t)
            {
                var pn = GetPluginName(t);
                return pn != null ? $"{pn} ({t.FullName})" : t.FullName;
            }

            if (name == null)
            {
                // If only one plugin type, use it
                if (pluginTypes.Count == 1)
                {
                    pluginType = pluginTypes[0];
                }
                else
                {
                    // Exclude plugins whose control implements ICompanion and try to find a single non-companion
                    var nonCompanionTypes = new System.Collections.Generic.List<Type>();

                    foreach (var t in pluginTypes)
                    {
                        try
                        {
                            var inst = Activator.CreateInstance(t) as IXrmToolBoxPlugin;
                            if (inst != null)
                            {
                                var control = inst.GetControl();
                                if (control is XrmToolBox.Extensibility.Interfaces.ICompanion)
                                {
                                    // skip companions
                                    continue;
                                }
                            }

                            nonCompanionTypes.Add(t);
                        }
                        catch
                        {
                            // If instantiation fails, keep the type as a candidate
                            nonCompanionTypes.Add(t);
                        }
                    }

                    if (nonCompanionTypes.Count == 1)
                    {
                        pluginType = nonCompanionTypes[0];
                    }
                    else if (nonCompanionTypes.Count == 0)
                    {
                        var list = string.Join("\n  - ", pluginTypes.Select(FormatType));
                        throw new InvalidOperationException($"Multiple plugin types found in '{Path.GetFileName(dllFile)}', but all are companion tools. Please specify a plugin name. Found:\n  - {list}");
                    }
                    else
                    {
                        var list = string.Join("\n  - ", nonCompanionTypes.Select(FormatType));
                        throw new InvalidOperationException($"Multiple plugin types found in '{Path.GetFileName(dllFile)}'. Please specify a plugin name using the -Name parameter. Candidates:\n  - {list}");
                    }
                }
            }
            else
            {
                // Match by ExportMetadata Name attribute - case-insensitive partial match
                var matchingTypes = pluginTypes
                    .Where(t =>
                    {
                        var pn = GetPluginName(t);
                        return !string.IsNullOrEmpty(pn) && pn.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0;
                    })
                    .ToList();

                if (matchingTypes.Count == 0)
                {
                    var list = string.Join("\n  - ", pluginTypes.Select(FormatType));
                    throw new InvalidOperationException($"No plugin with name matching '{name}' found in '{Path.GetFileName(dllFile)}'. Available plugins:\n  - {list}");
                }

                if (matchingTypes.Count > 1)
                {
                    var list = string.Join("\n  - ", matchingTypes.Select(FormatType));
                    throw new InvalidOperationException($"Multiple plugins match the name '{name}' in '{Path.GetFileName(dllFile)}'. Be more specific. Matches:\n  - {list}");
                }

                pluginType = matchingTypes[0];
            }

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

        private string GetPluginName(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(ExportMetadataAttribute), false)
                .Cast<ExportMetadataAttribute>()
                .Where(attr => attr.Name == "Name")
                .Select(attr => attr.Value as string)
                .FirstOrDefault();
            return attrs;
        }
    }
}

