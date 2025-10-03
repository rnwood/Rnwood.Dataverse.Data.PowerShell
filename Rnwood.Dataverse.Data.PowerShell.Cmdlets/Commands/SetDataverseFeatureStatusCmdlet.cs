using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseFeatureStatus", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SetFeatureStatusResponse))]
    ///<summary>Executes SetFeatureStatusRequest SDK message.</summary>
    public class SetDataverseFeatureStatusCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FeatureType parameter")]
        public Int32 FeatureType { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Status parameter")]
        public Boolean Status { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ConfigData parameter")]
        public String ConfigData { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IsSolutionUninstall parameter")]
        public Boolean IsSolutionUninstall { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SetFeatureStatusRequest();
            request.FeatureType = FeatureType;            request.Status = Status;            request.ConfigData = ConfigData;            request.IsSolutionUninstall = IsSolutionUninstall;
            if (ShouldProcess("Executing SetFeatureStatusRequest", "SetFeatureStatusRequest"))
            {
                var response = (SetFeatureStatusResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
