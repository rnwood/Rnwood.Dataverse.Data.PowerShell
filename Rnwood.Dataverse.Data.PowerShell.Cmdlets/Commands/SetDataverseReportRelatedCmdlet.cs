using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseReportRelated", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SetReportRelatedResponse))]
    ///<summary>Executes SetReportRelatedRequest SDK message.</summary>
    public class SetDataverseReportRelatedCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ReportId parameter")]
        public Guid ReportId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Entities parameter")]
        public Int32[] Entities { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Categories parameter")]
        public Int32[] Categories { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Visibility parameter")]
        public Int32[] Visibility { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SetReportRelatedRequest();
            request.ReportId = ReportId;            request.Entities = Entities;            request.Categories = Categories;            request.Visibility = Visibility;
            if (ShouldProcess("Executing SetReportRelatedRequest", "SetReportRelatedRequest"))
            {
                var response = (SetReportRelatedResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
