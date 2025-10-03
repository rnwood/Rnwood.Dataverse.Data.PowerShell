using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseExchangeAppointments")]
    [OutputType(typeof(RetrieveExchangeAppointmentsResponse))]
    ///<summary>Executes RetrieveExchangeAppointmentsRequest SDK message.</summary>
    public class GetDataverseExchangeAppointmentsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "StartDate parameter")]
        public DateTime StartDate { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EndDate parameter")]
        public DateTime EndDate { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveExchangeAppointmentsRequest();
            request.StartDate = StartDate;            request.EndDate = EndDate;
            var response = (RetrieveExchangeAppointmentsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
