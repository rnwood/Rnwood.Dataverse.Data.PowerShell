using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseWhoAmI")]
    [OutputType(typeof(WhoAmIResponse))]
    public class GetDataverseWhoAmICmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnnection cmdlet, or string specifying Dataverse organization URL (e.g. http://server.com/MyOrg/)")]
        public override ServiceClient Connection { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            WhoAmIRequest request = new WhoAmIRequest();
            WhoAmIResponse response = (WhoAmIResponse)Connection.Execute(request);

            WriteObject(response);
        }
    }
}
