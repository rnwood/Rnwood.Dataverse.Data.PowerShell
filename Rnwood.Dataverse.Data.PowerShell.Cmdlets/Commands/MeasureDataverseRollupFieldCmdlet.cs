using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsDiagnostic.Measure, "DataverseRollupField")]
    [OutputType(typeof(CalculateRollupFieldResponse))]
    ///<summary>Executes CalculateRollupFieldRequest SDK message.</summary>
    public class MeasureDataverseRollupFieldCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FieldName parameter")]
        public String FieldName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CalculateRollupFieldRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            request.FieldName = FieldName;
            var response = (CalculateRollupFieldResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
