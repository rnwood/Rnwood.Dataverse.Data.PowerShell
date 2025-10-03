using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseFeatureControlSettings")]
    [OutputType(typeof(RetrieveFeatureControlSettingsResponse))]
    ///<summary>Executes RetrieveFeatureControlSettingsRequest SDK message.</summary>
    public class GetDataverseFeatureControlSettingsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "NamespaceValue parameter")]
        public String NamespaceValue { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FeatureControlName parameter")]
        public String FeatureControlName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveFeatureControlSettingsRequest();
            request.NamespaceValue = NamespaceValue;            request.FeatureControlName = FeatureControlName;
            var response = (RetrieveFeatureControlSettingsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
