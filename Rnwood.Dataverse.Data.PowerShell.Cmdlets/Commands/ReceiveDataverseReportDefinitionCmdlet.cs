using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommunications.Receive, "DataverseReportDefinition")]
    [OutputType(typeof(DownloadReportDefinitionResponse))]
    ///<summary>Executes DownloadReportDefinitionRequest SDK message.</summary>
    public class ReceiveDataverseReportDefinitionCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ReportId parameter")]
        public Guid ReportId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new DownloadReportDefinitionRequest();
            request.ReportId = ReportId;
            var response = (DownloadReportDefinitionResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
