using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Threading;
using Rnwood.Dataverse.Data.PowerShell.Commands;

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

                // Don't try to resolve satellite assemblies (culture-specific resource assemblies)
                // These are for localization and should be allowed to fail gracefully
                if (assemblyName.CultureName != null && !string.IsNullOrEmpty(assemblyName.CultureName) && assemblyName.CultureName != "neutral")
                {
                    return null;
                }

                string path = Path.Combine(basePath, assemblyName.Name + ".dll");

                if (File.Exists(path))
                {
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

            // Bump up the min threads reserved for this app to ramp connections faster - minWorkerThreads defaults to 4, minIOCP defaults to 4 
            ThreadPool.SetMinThreads(100, 100);
            // Change max connections from .NET to a remote service default: 2
            System.Net.ServicePointManager.DefaultConnectionLimit = 65000;
            // Turn off the Expect 100 to continue message - 'true' will cause the caller to wait until it round-trip confirms a connection to the server 
            System.Net.ServicePointManager.Expect100Continue = false;
            // Can decrease overall transmission overhead but can cause delay in data packet arrival
            System.Net.ServicePointManager.UseNagleAlgorithm = false;

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
                }
                else
                {
                    //Console.WriteLine("Assembly " + assemblyName.Name + " not resolved");
                }

                return null;
            }
        }
#endif

    }

}
