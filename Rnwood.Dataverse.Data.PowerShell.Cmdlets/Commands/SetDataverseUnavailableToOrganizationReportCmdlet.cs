using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseUnavailableToOrganizationReport", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(MakeUnavailableToOrganizationReportResponse))]
    ///<summary>Executes MakeUnavailableToOrganizationReportRequest SDK message.</summary>
    public class SetDataverseUnavailableToOrganizationReportCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ReportId parameter")]
        public Guid ReportId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new MakeUnavailableToOrganizationReportRequest();
            request.ReportId = ReportId;
            if (ShouldProcess("Executing MakeUnavailableToOrganizationReportRequest", "MakeUnavailableToOrganizationReportRequest"))
            {
                var response = (MakeUnavailableToOrganizationReportResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
