using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsDiagnostic.Measure, "DataverseTotalTimeIncident")]
    [OutputType(typeof(CalculateTotalTimeIncidentResponse))]
    ///<summary>Executes CalculateTotalTimeIncidentRequest SDK message.</summary>
    public class MeasureDataverseTotalTimeIncidentCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IncidentId parameter")]
        public Guid IncidentId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CalculateTotalTimeIncidentRequest();
            request.IncidentId = IncidentId;
            var response = (CalculateTotalTimeIncidentResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
