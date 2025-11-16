using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPluginLoader
{
    [Export(typeof(IXrmToolBoxPlugin)),
    ExportMetadata("Name", "PowerShell Scripting Workspace"),
    ExportMetadata("Description", "This is a description for my first plugin"),
    // Please specify the base64 content of a 32x32 pixels image
    ExportMetadata("SmallImageBase64", null),
    // Please specify the base64 content of a 80x80 pixels image
    ExportMetadata("BigImageBase64", null),
    ExportMetadata("BackgroundColor", "Lavender"),
    ExportMetadata("PrimaryFontColor", "Black"),
    ExportMetadata("SecondaryFontColor", "Gray")]   
    public class PluginLoader : PluginBase
    {
        static PluginLoader()
        {
            try
            {
                string loaderDir = Path.GetDirectoryName(AppContext.BaseDirectory);
                string pluginSubdir = Path.Combine(loaderDir, "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin");

                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {

                    string assemblyName = new AssemblyName(args.Name).Name;
                    string dllPath = Path.Combine(pluginSubdir, assemblyName + ".dll");

                    if (File.Exists(dllPath))
                    {
                        return Assembly.LoadFrom(dllPath);
                    }


                    return null;
                };
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"Failed to set up assembly resolve event: {ex.Message}");
            }
        }

        public override IXrmToolBoxPluginControl GetControl()
        {
            return new PowerShellConsolePlugin();
        }
    }
}