using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Publish, "DataverseAllXml", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PublishAllXmlResponse))]
    ///<summary>Executes PublishAllXmlRequest SDK message.</summary>
    public class PublishDataverseAllXmlCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new PublishAllXmlRequest();

            if (ShouldProcess("Executing PublishAllXmlRequest", "PublishAllXmlRequest"))
            {
                var response = (PublishAllXmlResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
