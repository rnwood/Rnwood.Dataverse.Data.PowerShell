using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsDiagnostic.Measure, "DataversePrice")]
    [OutputType(typeof(CalculatePriceResponse))]
    ///<summary>Executes CalculatePriceRequest SDK message.</summary>
    public class MeasureDataversePriceCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ParentId parameter")]
        public Guid ParentId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CalculatePriceRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            request.ParentId = ParentId;
            var response = (CalculatePriceResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
