using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseProductFromKit", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RemoveProductFromKitResponse))]
    ///<summary>Executes RemoveProductFromKitRequest SDK message.</summary>
    public class RemoveDataverseProductFromKitCmdlet : OrganizationServiceCmdlet
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

            var request = new RemoveProductFromKitRequest();
            request.KitId = KitId;            request.ProductId = ProductId;
            if (ShouldProcess("Executing RemoveProductFromKitRequest", "RemoveProductFromKitRequest"))
            {
                var response = (RemoveProductFromKitResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
