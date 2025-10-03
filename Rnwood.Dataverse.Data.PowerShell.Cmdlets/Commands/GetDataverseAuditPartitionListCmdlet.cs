using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseAuditPartitionList")]
    [OutputType(typeof(RetrieveAuditPartitionListResponse))]
    ///<summary>Executes RetrieveAuditPartitionListRequest SDK message.</summary>
    public class GetDataverseAuditPartitionListCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveAuditPartitionListRequest();

            var response = (RetrieveAuditPartitionListResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
