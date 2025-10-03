using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "DataverseProductToKit", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(AddProductToKitResponse))]
    ///<summary>Executes AddProductToKitRequest SDK message.</summary>
    public class AddDataverseProductToKitCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "KitId parameter")]
        public Guid KitId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ProductId parameter")]
        public Guid ProductId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new AddProductToKitRequest();
            request.KitId = KitId;            request.ProductId = ProductId;
            if (ShouldProcess("Executing AddProductToKitRequest", "AddProductToKitRequest"))
            {
                var response = (AddProductToKitResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
