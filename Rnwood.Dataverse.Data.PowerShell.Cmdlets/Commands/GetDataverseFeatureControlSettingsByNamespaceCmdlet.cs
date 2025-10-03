using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseFeatureControlSettingsByNamespace")]
    [OutputType(typeof(RetrieveFeatureControlSettingsByNamespaceResponse))]
    ///<summary>Executes RetrieveFeatureControlSettingsByNamespaceRequest SDK message.</summary>
    public class GetDataverseFeatureControlSettingsByNamespaceCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "NamespaceName parameter")]
        public String NamespaceName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveFeatureControlSettingsByNamespaceRequest();
            request.NamespaceName = NamespaceName;
            var response = (RetrieveFeatureControlSettingsByNamespaceResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
