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
			// Determine the appropriate target framework based on PowerShell version
			string targetFramework = GetTargetFrameworkForPowerShellVersion();
			string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"/../../cmdlets/{targetFramework}";
			var alc = new CmdletsLoadContext(basePath);

			AssemblyLoadContext.Default.Resolving += (s, args) =>
			{
				AssemblyName assemblyName = new AssemblyName(args.Name);
				if (assemblyName.Name == "Rnwood.Dataverse.Data.PowerShell.Cmdlets" || assemblyName.Name == "Microsoft.ApplicationInsights") {
					return alc.LoadFromAssemblyName(assemblyName);
				}

				return null;
			};

			// Don't explicitly load the assembly here - let PowerShell load it from the manifest
			// The Resolving event above will intercept and redirect the load
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
		private static string GetTargetFrameworkForPowerShellVersion()
		{
			try
			{
				// Get PowerShell version from the PSVersionTable
				var psVersionTable = System.Management.Automation.PowerShell.Create()
					.AddScript("$PSVersionTable.PSVersion")
					.Invoke();

				if (psVersionTable.Count > 0 && psVersionTable[0].BaseObject != null)
				{
					var versionObj = psVersionTable[0].BaseObject;
					// Extract Major and Minor properties using reflection
					var majorProp = versionObj.GetType().GetProperty("Major");
					var minorProp = versionObj.GetType().GetProperty("Minor");
					
					if (majorProp != null && minorProp != null)
					{
						int major = (int)majorProp.GetValue(versionObj);
						int minor = (int)minorProp.GetValue(versionObj);
						
						// Use net8.0 for PowerShell 7.4 and later
						if (major > 7 || (major == 7 && minor >= 4))
						{
							return "net8.0";
						}
					}
				}
			}
			catch
			{
				// Fall back to net6.0 on any error
			}

			// Fall back to net6.0 for older PowerShell Core versions
			return "net6.0";
		}
#endif


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
