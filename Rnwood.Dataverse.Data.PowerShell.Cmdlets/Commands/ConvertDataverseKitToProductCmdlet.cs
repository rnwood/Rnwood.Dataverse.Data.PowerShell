using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Convert, "DataverseKitToProduct", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ConvertKitToProductResponse))]
    ///<summary>Executes ConvertKitToProductRequest SDK message.</summary>
    public class ConvertDataverseKitToProductCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "KitId parameter")]
        public Guid KitId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ConvertKitToProductRequest();
            request.KitId = KitId;
            if (ShouldProcess("Executing ConvertKitToProductRequest", "ConvertKitToProductRequest"))
            {
                var response = (ConvertKitToProductResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
