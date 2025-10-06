﻿using Microsoft.PowerPlatform.Dataverse.Client;
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
		private ServiceClient _connection;

		/// <summary>
		/// Gets or sets the Dataverse connection to use for the cmdlet.
		/// </summary>
		[Parameter(Mandatory = false, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g. http://server.com/MyOrg/). If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.")]
        public virtual ServiceClient Connection 
		{ 
			get { return _connection; }
			set { _connection = value; }
		}

		/// <summary>
		/// Initializes the cmdlet processing and resolves the connection.
		/// </summary>
		protected override void BeginProcessing()
		{
			base.BeginProcessing();

			// If no connection was provided, try to use the default
			if (_connection == null)
			{
				_connection = DefaultConnectionManager.DefaultConnection;
				
				if (_connection == null)
				{
					ThrowTerminatingError(new ErrorRecord(
						new InvalidOperationException("No connection provided and no default connection is set. Either provide a -Connection parameter or set a default connection using: Get-DataverseConnection -SetAsDefault <parameters>"),
						"NoConnection",
						ErrorCategory.InvalidOperation,
						null));
				}
			}
		}
	}
}