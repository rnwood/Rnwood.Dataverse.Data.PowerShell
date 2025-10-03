using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Convert, "DataverseProductToKit", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ConvertProductToKitResponse))]
    ///<summary>Executes ConvertProductToKitRequest SDK message.</summary>
    public class ConvertDataverseProductToKitCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ProductId parameter")]
        public Guid ProductId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ConvertProductToKitRequest();
            request.ProductId = ProductId;
            if (ShouldProcess("Executing ConvertProductToKitRequest", "ConvertProductToKitRequest"))
            {
                var response = (ConvertProductToKitResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
