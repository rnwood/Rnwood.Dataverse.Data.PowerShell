using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseReportHistoryLimit")]
    [OutputType(typeof(GetReportHistoryLimitResponse))]
    ///<summary>Executes GetReportHistoryLimitRequest SDK message.</summary>
    public class GetDataverseReportHistoryLimitCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ReportId parameter")]
        public Guid ReportId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetReportHistoryLimitRequest();
            request.ReportId = ReportId;
            var response = (GetReportHistoryLimitResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
