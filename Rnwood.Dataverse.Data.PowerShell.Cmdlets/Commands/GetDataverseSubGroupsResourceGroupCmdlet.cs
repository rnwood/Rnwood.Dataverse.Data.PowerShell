using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseSubGroupsResourceGroup")]
    [OutputType(typeof(RetrieveSubGroupsResourceGroupResponse))]
    ///<summary>Executes RetrieveSubGroupsResourceGroupRequest SDK message.</summary>
    public class GetDataverseSubGroupsResourceGroupCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ResourceGroupId parameter")]
        public Guid ResourceGroupId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Query parameter")]
        public Microsoft.Xrm.Sdk.Query.QueryBase Query { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveSubGroupsResourceGroupRequest();
            request.ResourceGroupId = ResourceGroupId;            request.Query = Query;
            var response = (RetrieveSubGroupsResourceGroupResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
