using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Update, "DataverseProductProperties", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(UpdateProductPropertiesResponse))]
    ///<summary>Executes UpdateProductPropertiesRequest SDK message.</summary>
    public class UpdateDataverseProductPropertiesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PropertyInstanceList parameter")]
        public Microsoft.Xrm.Sdk.EntityCollection PropertyInstanceList { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new UpdateProductPropertiesRequest();
            request.PropertyInstanceList = PropertyInstanceList;
            if (ShouldProcess("Executing UpdateProductPropertiesRequest", "UpdateProductPropertiesRequest"))
            {
                var response = (UpdateProductPropertiesResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
