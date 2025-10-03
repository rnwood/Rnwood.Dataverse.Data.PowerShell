using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Find, "DataverseSearch")]
    [OutputType(typeof(SearchResponse))]
    ///<summary>Executes SearchRequest SDK message.</summary>
    public class FindDataverseSearchCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AppointmentRequest parameter")]
        public Microsoft.Crm.Sdk.Messages.AppointmentRequest AppointmentRequest { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SearchRequest();
            request.AppointmentRequest = AppointmentRequest;
            var response = (SearchResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
