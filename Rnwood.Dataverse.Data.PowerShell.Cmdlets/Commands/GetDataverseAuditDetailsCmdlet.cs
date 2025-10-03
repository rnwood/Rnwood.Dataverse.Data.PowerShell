using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseAuditDetails")]
    [OutputType(typeof(RetrieveAuditDetailsResponse))]
    ///<summary>Executes RetrieveAuditDetailsRequest SDK message.</summary>
    public class GetDataverseAuditDetailsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AuditId parameter")]
        public Guid AuditId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveAuditDetailsRequest();
            request.AuditId = AuditId;
            var response = (RetrieveAuditDetailsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
