using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseMembersBulkOperation")]
    [OutputType(typeof(RetrieveMembersBulkOperationResponse))]
    ///<summary>Executes RetrieveMembersBulkOperationRequest SDK message.</summary>
    public class GetDataverseMembersBulkOperationCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "BulkOperationId parameter")]
        public Guid BulkOperationId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "BulkOperationSource parameter")]
        public Int32 BulkOperationSource { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntitySource parameter")]
        public Int32 EntitySource { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Query parameter")]
        public Microsoft.Xrm.Sdk.Query.QueryBase Query { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveMembersBulkOperationRequest();
            request.BulkOperationId = BulkOperationId;            request.BulkOperationSource = BulkOperationSource;            request.EntitySource = EntitySource;            request.Query = Query;
            var response = (RetrieveMembersBulkOperationResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
