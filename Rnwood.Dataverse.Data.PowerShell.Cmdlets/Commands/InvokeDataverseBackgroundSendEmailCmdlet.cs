using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseBackgroundSendEmail")]
    [OutputType(typeof(BackgroundSendEmailResponse))]
    ///<summary>Executes BackgroundSendEmailRequest SDK message.</summary>
    public class InvokeDataverseBackgroundSendEmailCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Query parameter")]
        public Microsoft.Xrm.Sdk.Query.QueryBase Query { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new BackgroundSendEmailRequest();
            request.Query = Query;
            var response = (BackgroundSendEmailResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
