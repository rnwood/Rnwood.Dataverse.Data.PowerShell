using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommunications.Receive, "DataverseSolutionExportData")]
    [OutputType(typeof(DownloadSolutionExportDataResponse))]
    ///<summary>Executes DownloadSolutionExportDataRequest SDK message.</summary>
    public class ReceiveDataverseSolutionExportDataCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportJobId parameter")]
        public Guid ExportJobId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new DownloadSolutionExportDataRequest();
            request.ExportJobId = ExportJobId;
            var response = (DownloadSolutionExportDataResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
