using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific.Commands
{
	[CmdletProvider("Dataverse", ProviderCapabilities.None)]
	public class ModuleInitProvider : DriveCmdletProvider
	{
		static ModuleInitProvider()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;


			Assembly.Load(new AssemblyName("Microsoft.Xrm.Sdk"));
		}

		private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			string assyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			string assyName = args.Name.Split(',')[0];
			string assyFile = assyDir + "/" + assyName + ".dll";

			if (File.Exists(assyFile))
			{
				Console.WriteLine("Resolved assembly " + args.Name);
				return Assembly.LoadFrom(assyFile);
			}

			Console.WriteLine("Did not resolve assembly " + args.Name);

			return null;
		}
	}
}
