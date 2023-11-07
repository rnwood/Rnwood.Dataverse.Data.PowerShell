using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;

using Microsoft.Xrm.Sdk;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet("Invoke", "DataverseRequest")]
    ///<summary>Invokes a Dataverse request.</summary>
    public class InvokeDataverseRequestCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true)]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, HelpMessage="Request to execute", ValueFromRemainingArguments=true, ValueFromPipeline=true)]
        public  OrganizationRequest Request { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteObject(Connection.Execute(Request));
        }
    }
}
