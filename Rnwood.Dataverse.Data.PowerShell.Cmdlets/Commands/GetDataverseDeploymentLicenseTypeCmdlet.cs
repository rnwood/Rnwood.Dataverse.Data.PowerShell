using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseDeploymentLicenseType")]
    [OutputType(typeof(RetrieveDeploymentLicenseTypeResponse))]
    ///<summary>Executes RetrieveDeploymentLicenseTypeRequest SDK message.</summary>
    public class GetDataverseDeploymentLicenseTypeCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveDeploymentLicenseTypeRequest();

            var response = (RetrieveDeploymentLicenseTypeResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
