using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseQuantityDecimal")]
    [OutputType(typeof(GetQuantityDecimalResponse))]
    ///<summary>Executes GetQuantityDecimalRequest SDK message.</summary>
    public class GetDataverseQuantityDecimalCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ProductId parameter")]
        public Guid ProductId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UoMId parameter")]
        public Guid UoMId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new GetQuantityDecimalRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            request.ProductId = ProductId;            request.UoMId = UoMId;
            var response = (GetQuantityDecimalResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
