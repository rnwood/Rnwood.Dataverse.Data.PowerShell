using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseUtcTimeFromLocalTime")]
    [OutputType(typeof(UtcTimeFromLocalTimeResponse))]
    ///<summary>Executes UtcTimeFromLocalTimeRequest SDK message.</summary>
    public class InvokeDataverseUtcTimeFromLocalTimeCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TimeZoneCode parameter")]
        public Int32 TimeZoneCode { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "LocalTime parameter")]
        public DateTime LocalTime { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new UtcTimeFromLocalTimeRequest();
            request.TimeZoneCode = TimeZoneCode;            request.LocalTime = LocalTime;
            var response = (UtcTimeFromLocalTimeResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
