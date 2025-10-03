using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseByResourceResourceGroup")]
    [OutputType(typeof(RetrieveByResourceResourceGroupResponse))]
    ///<summary>Executes RetrieveByResourceResourceGroupRequest SDK message.</summary>
    public class GetDataverseByResourceResourceGroupCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ResourceId parameter")]
        public Guid ResourceId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Query parameter")]
        public Microsoft.Xrm.Sdk.Query.QueryBase Query { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveByResourceResourceGroupRequest();
            request.ResourceId = ResourceId;            request.Query = Query;
            var response = (RetrieveByResourceResourceGroupResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
