using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Base class for cmdlets that interact with a Dataverse organization service.
	/// </summary>
	public abstract class OrganizationServiceCmdlet : PSCmdlet
    {
		/// <summary>
		/// Gets or sets the Dataverse connection to use for the cmdlet.
		/// </summary>
        public abstract ServiceClient Connection { get; set; }
	}
}