using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;

#if NETCOREAPP
using System.Runtime.Loader;
#endif

namespace Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific.Loader
{
	public class ModuleInitProvider : IModuleAssemblyInitializer
	{
		public void OnImport()
		{

#if NET
			// The manifest loads the appropriate target framework (net8.0 for Core)
			// The loader just needs to handle dependency resolution for the cmdlets
			string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../cmdlets";
			// Get the actual target framework directory that was loaded
			var loaderPath = Assembly.GetExecutingAssembly().Location;
			var targetFramework = Path.GetFileName(Path.GetDirectoryName(loaderPath));
			basePath = Path.Combine(basePath, targetFramework);
			
			var alc = new CmdletsLoadContext(basePath);

			AssemblyLoadContext.Default.Resolving += (s, args) =>
			{
				AssemblyName assemblyName = new AssemblyName(args.Name);
				if (assemblyName.Name == "Rnwood.Dataverse.Data.PowerShell.Cmdlets" || assemblyName.Name == "Microsoft.ApplicationInsights") {
					return alc.LoadFromAssemblyName(assemblyName);
				}

				return null;
			};
#else
			string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../cmdlets/net462";

			AppDomain.CurrentDomain.AssemblyResolve += (s, args) =>
			{
				AssemblyName assemblyName = new AssemblyName(args.Name);
				string path = Path.Combine(basePath, assemblyName.Name + ".dll");

				if (File.Exists(path))
				{
					//Console.WriteLine("Assembly " + assemblyName.Name + " redirected");
					return Assembly.LoadFrom(path);
				}

				//Console.WriteLine("Assembly " + assemblyName.Name + " not resolved");
				return null;
			};
#endif


		}

#if NET

		public class CmdletsLoadContext : AssemblyLoadContext
		{
			public CmdletsLoadContext(string basePath)
			{
				this.basePath = basePath;
			}

			private readonly string basePath;

			protected override Assembly Load(AssemblyName assemblyName)
			{
				string path = Path.Combine(basePath, assemblyName.Name + ".dll");

				if (File.Exists(path))
				{
					//Console.WriteLine("Assembly " + assemblyName.Name + " redirected");
					return LoadFromAssemblyPath(path);
				} else {
					//Console.WriteLine("Assembly " + assemblyName.Name + " not resolved");
				}

				return null;
			}
		}
#endif

	}

}
