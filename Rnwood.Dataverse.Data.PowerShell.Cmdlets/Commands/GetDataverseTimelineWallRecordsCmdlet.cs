using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseTimelineWallRecords")]
    [OutputType(typeof(RetrieveTimelineWallRecordsResponse))]
    ///<summary>Executes RetrieveTimelineWallRecordsRequest SDK message.</summary>
    public class GetDataverseTimelineWallRecordsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "FetchXml parameter")]
        public String FetchXml { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RollupType parameter")]
        public Int32 RollupType { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveTimelineWallRecordsRequest();
            request.FetchXml = FetchXml;            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }
            request.RollupType = RollupType;
            var response = (RetrieveTimelineWallRecordsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
