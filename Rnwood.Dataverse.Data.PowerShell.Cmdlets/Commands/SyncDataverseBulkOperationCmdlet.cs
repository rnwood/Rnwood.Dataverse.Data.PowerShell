using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Sync, "DataverseBulkOperation")]
    [OutputType(typeof(SyncBulkOperationResponse))]
    ///<summary>Executes SyncBulkOperationRequest SDK message.</summary>
    public class SyncDataverseBulkOperationCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QuerySet parameter")]
        public Microsoft.Xrm.Sdk.Query.QueryExpression[] QuerySet { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OperationType parameter")]
        public Int32 OperationType { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SyncBulkOperationRequest();
            request.QuerySet = QuerySet;            request.OperationType = OperationType;
            var response = (SyncBulkOperationResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
