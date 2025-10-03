using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Publish, "DataverseAllXmlAsync", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PublishAllXmlAsyncResponse))]
    ///<summary>Executes PublishAllXmlAsyncRequest SDK message.</summary>
    public class PublishDataverseAllXmlAsyncCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new PublishAllXmlAsyncRequest();

            if (ShouldProcess("Executing PublishAllXmlAsyncRequest", "PublishAllXmlAsyncRequest"))
            {
                var response = (PublishAllXmlAsyncResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
