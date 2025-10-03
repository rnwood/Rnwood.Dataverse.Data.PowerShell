using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseParentGroupsResourceGroup")]
    [OutputType(typeof(RetrieveParentGroupsResourceGroupResponse))]
    ///<summary>Executes RetrieveParentGroupsResourceGroupRequest SDK message.</summary>
    public class GetDataverseParentGroupsResourceGroupCmdlet : OrganizationServiceCmdlet
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

            var request = new RetrieveParentGroupsResourceGroupRequest();
            request.ResourceGroupId = ResourceGroupId;            request.Query = Query;
            var response = (RetrieveParentGroupsResourceGroupResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
