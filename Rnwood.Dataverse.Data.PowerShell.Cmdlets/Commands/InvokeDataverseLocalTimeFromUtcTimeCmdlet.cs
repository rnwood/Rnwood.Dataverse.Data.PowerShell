using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseLocalTimeFromUtcTime")]
    [OutputType(typeof(LocalTimeFromUtcTimeResponse))]
    ///<summary>Executes LocalTimeFromUtcTimeRequest SDK message.</summary>
    public class InvokeDataverseLocalTimeFromUtcTimeCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TimeZoneCode parameter")]
        public Int32 TimeZoneCode { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UtcTime parameter")]
        public DateTime UtcTime { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new LocalTimeFromUtcTimeRequest();
            request.TimeZoneCode = TimeZoneCode;            request.UtcTime = UtcTime;
            var response = (LocalTimeFromUtcTimeResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
