using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Find, "DataverseMultipleSchedules")]
    [OutputType(typeof(QueryMultipleSchedulesResponse))]
    ///<summary>Executes QueryMultipleSchedulesRequest SDK message.</summary>
    public class FindDataverseMultipleSchedulesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ResourceIds parameter")]
        public Guid[] ResourceIds { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Start parameter")]
        public DateTime Start { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "End parameter")]
        public DateTime End { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TimeCodes parameter")]
        public Microsoft.Crm.Sdk.Messages.TimeCode[] TimeCodes { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new QueryMultipleSchedulesRequest();
            request.ResourceIds = ResourceIds;            request.Start = Start;            request.End = End;            request.TimeCodes = TimeCodes;
            var response = (QueryMultipleSchedulesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
