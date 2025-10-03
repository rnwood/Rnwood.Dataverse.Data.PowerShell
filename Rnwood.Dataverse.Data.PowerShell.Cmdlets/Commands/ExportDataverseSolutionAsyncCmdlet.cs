using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Export, "DataverseSolutionAsync")]
    [OutputType(typeof(ExportSolutionAsyncResponse))]
    ///<summary>Executes ExportSolutionAsyncRequest SDK message.</summary>
    public class ExportDataverseSolutionAsyncCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SolutionName parameter")]
        public String SolutionName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Managed parameter")]
        public Boolean Managed { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TargetVersion parameter")]
        public String TargetVersion { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportAutoNumberingSettings parameter")]
        public Boolean ExportAutoNumberingSettings { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportCalendarSettings parameter")]
        public Boolean ExportCalendarSettings { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportCustomizationSettings parameter")]
        public Boolean ExportCustomizationSettings { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportEmailTrackingSettings parameter")]
        public Boolean ExportEmailTrackingSettings { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportGeneralSettings parameter")]
        public Boolean ExportGeneralSettings { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportMarketingSettings parameter")]
        public Boolean ExportMarketingSettings { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportOutlookSynchronizationSettings parameter")]
        public Boolean ExportOutlookSynchronizationSettings { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportRelationshipRoles parameter")]
        public Boolean ExportRelationshipRoles { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportIsvConfig parameter")]
        public Boolean ExportIsvConfig { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportSales parameter")]
        public Boolean ExportSales { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportExternalApplications parameter")]
        public Boolean ExportExternalApplications { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ExportComponentsParams parameter")]
        public Microsoft.Xrm.Sdk.ExportComponentsParams ExportComponentsParams { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ExportSolutionAsyncRequest();
            request.SolutionName = SolutionName;            request.Managed = Managed;            request.TargetVersion = TargetVersion;            request.ExportAutoNumberingSettings = ExportAutoNumberingSettings;            request.ExportCalendarSettings = ExportCalendarSettings;            request.ExportCustomizationSettings = ExportCustomizationSettings;            request.ExportEmailTrackingSettings = ExportEmailTrackingSettings;            request.ExportGeneralSettings = ExportGeneralSettings;            request.ExportMarketingSettings = ExportMarketingSettings;            request.ExportOutlookSynchronizationSettings = ExportOutlookSynchronizationSettings;            request.ExportRelationshipRoles = ExportRelationshipRoles;            request.ExportIsvConfig = ExportIsvConfig;            request.ExportSales = ExportSales;            request.ExportExternalApplications = ExportExternalApplications;            request.ExportComponentsParams = ExportComponentsParams;
            var response = (ExportSolutionAsyncResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
