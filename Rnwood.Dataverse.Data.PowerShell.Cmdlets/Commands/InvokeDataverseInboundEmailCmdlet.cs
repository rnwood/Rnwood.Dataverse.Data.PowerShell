using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseInboundEmail")]
    [OutputType(typeof(ProcessInboundEmailResponse))]
    ///<summary>Executes ProcessInboundEmailRequest SDK message.</summary>
    public class InvokeDataverseInboundEmailCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "InboundEmailActivity parameter")]
        public Guid InboundEmailActivity { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ProcessInboundEmailRequest();
            request.InboundEmailActivity = InboundEmailActivity;
            var response = (ProcessInboundEmailResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
