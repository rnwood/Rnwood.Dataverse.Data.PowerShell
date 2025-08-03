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
			string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/../../cmdlets/net6.0";

#if NET
			var alc = new CmdletsLoadContext(basePath);

			AssemblyLoadContext.Default.Resolving += (s, args) =>
			{
				AssemblyName assemblyName = new AssemblyName(args.Name);
				if (assemblyName.Name == "Rnwood.Dataverse.Data.PowerShell.Cmdlets" || assemblyName.Name == "Microsoft.ApplicationInsights") {
					return alc.LoadFromAssemblyName(assemblyName);
				}

				return null;
			};

			//Load the assembly
			AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName("Rnwood.Dataverse.Data.PowerShell.Cmdlets"));
#else


			AppDomain.CurrentDomain.AssemblyResolve += (s, args) =>
			{
				AssemblyName assemblyName = new AssemblyName(args.Name);
				string path = Path.Combine(basePath, assemblyName.Name + ".dll");

				if (File.Exists(path))
				{
					return Assembly.LoadFrom(path);
				}

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
				Console.WriteLine("Assembly " + assemblyName.Name + " redirected");
					return LoadFromAssemblyPath(path);
				} else {
					Console.WriteLine("Assembly " + assemblyName.Name + " not resolved");
				}

				return null;
			}
		}
#endif

	}

}
