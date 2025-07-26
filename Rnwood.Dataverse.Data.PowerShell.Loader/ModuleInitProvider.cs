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
			string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

#if NETCOREAPP
			new CmdletsLoadContext(basePath).LoadFromAssemblyName(new AssemblyName( "Rnwood.Dataverse.Data.PowerShell.Cmdlets"));
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


#if NETCOREAPP

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
					return LoadFromAssemblyPath(path);
				}

				return null;
			}
		}
#endif

	}

}
