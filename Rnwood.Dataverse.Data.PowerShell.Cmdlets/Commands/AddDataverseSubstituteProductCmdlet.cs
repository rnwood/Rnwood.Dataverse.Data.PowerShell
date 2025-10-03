using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "DataverseSubstituteProduct", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(AddSubstituteProductResponse))]
    ///<summary>Executes AddSubstituteProductRequest SDK message.</summary>
    public class AddDataverseSubstituteProductCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ProductId parameter")]
        public Guid ProductId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SubstituteId parameter")]
        public Guid SubstituteId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new AddSubstituteProductRequest();
            request.ProductId = ProductId;            request.SubstituteId = SubstituteId;
            if (ShouldProcess("Executing AddSubstituteProductRequest", "AddSubstituteProductRequest"))
            {
                var response = (AddSubstituteProductResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
