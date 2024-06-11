using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific
{
	public class ModuleInit : IModuleAssemblyInitializer
	{
		public void OnImport()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		}

		private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			string assyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			string assyName = args.Name.Split(',')[0];
			string assyFile = assyDir + "/" + assyName + ".dll";

			if (File.Exists(assyFile))
			{
				return Assembly.LoadFrom(assyFile);
			}

			return null;
		}
	}
}
