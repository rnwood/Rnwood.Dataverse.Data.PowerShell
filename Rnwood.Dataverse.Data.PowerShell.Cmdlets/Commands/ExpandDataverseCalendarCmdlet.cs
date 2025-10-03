using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Expand, "DataverseCalendar")]
    [OutputType(typeof(ExpandCalendarResponse))]
    ///<summary>Executes ExpandCalendarRequest SDK message.</summary>
    public class ExpandDataverseCalendarCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CalendarId parameter")]
        public Guid CalendarId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Start parameter")]
        public DateTime Start { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "End parameter")]
        public DateTime End { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ExpandCalendarRequest();
            request.CalendarId = CalendarId;            request.Start = Start;            request.End = End;
            var response = (ExpandCalendarResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
