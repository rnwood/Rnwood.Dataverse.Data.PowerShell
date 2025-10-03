using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseAuditData", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(DeleteAuditDataResponse))]
    ///<summary>Executes DeleteAuditDataRequest SDK message.</summary>
    public class RemoveDataverseAuditDataCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EndDate parameter")]
        public DateTime EndDate { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new DeleteAuditDataRequest();
            request.EndDate = EndDate;
            if (ShouldProcess("Executing DeleteAuditDataRequest", "DeleteAuditDataRequest"))
            {
                var response = (DeleteAuditDataResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
