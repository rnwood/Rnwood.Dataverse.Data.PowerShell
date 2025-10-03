using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Update, "DataverseFeatureConfig", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(UpdateFeatureConfigResponse))]
    ///<summary>Executes UpdateFeatureConfigRequest SDK message.</summary>
    public class UpdateDataverseFeatureConfigCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FeatureType parameter")]
        public Int32 FeatureType { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ConfigData parameter")]
        public String ConfigData { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new UpdateFeatureConfigRequest();
            request.FeatureType = FeatureType;            request.ConfigData = ConfigData;
            if (ShouldProcess("Executing UpdateFeatureConfigRequest", "UpdateFeatureConfigRequest"))
            {
                var response = (UpdateFeatureConfigResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
