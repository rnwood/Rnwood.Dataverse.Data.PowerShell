using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Gets information about the current user from a Dataverse connection.
	/// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseWhoAmI")]
    [OutputType(typeof(WhoAmIResponse))]
    public class GetDataverseWhoAmICmdlet : OrganizationServiceCmdlet
    {
		/// <summary>
		/// Executes the WhoAmI request.
		/// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            WhoAmIRequest request = new WhoAmIRequest();
            WhoAmIResponse response = (WhoAmIResponse)Connection.Execute(request);

            WriteObject(response);
        }
    }
}
