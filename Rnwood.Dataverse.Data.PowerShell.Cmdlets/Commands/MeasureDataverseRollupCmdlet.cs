using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsDiagnostic.Measure, "DataverseRollup")]
    [OutputType(typeof(RollupResponse))]
    ///<summary>Executes RollupRequest SDK message.</summary>
    public class MeasureDataverseRollupCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Query parameter")]
        public Microsoft.Xrm.Sdk.Query.QueryBase Query { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RollupType parameter")]
        public Microsoft.Crm.Sdk.Messages.RollupType RollupType { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RollupRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            request.Query = Query;            request.RollupType = RollupType;
            var response = (RollupResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
