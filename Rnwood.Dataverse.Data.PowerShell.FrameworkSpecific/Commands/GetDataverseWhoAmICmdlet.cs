using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseWhoAmI")]
    ///<summary>Retrieves details about the current Dataverse user and organization specified by the connection provided.</summary>
    public class GetDataverseWhoAmICmdlet : DataverseCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from New-DataverseConnnection cmdlet")]
        public override DataverseConnection Connection { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            WhoAmIRequest request = new WhoAmIRequest();
            WhoAmIResponse response = (WhoAmIResponse)Connection.Service.Execute(request);

            WriteObject(response);
        }
    }
}
