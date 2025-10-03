using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Find, "DataverseParentResourceGroup")]
    [OutputType(typeof(FindParentResourceGroupResponse))]
    ///<summary>Executes FindParentResourceGroupRequest SDK message.</summary>
    public class FindDataverseParentResourceGroupCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ParentId parameter")]
        public Guid ParentId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ChildrenIds parameter")]
        public Guid[] ChildrenIds { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new FindParentResourceGroupRequest();
            request.ParentId = ParentId;            request.ChildrenIds = ChildrenIds;
            var response = (FindParentResourceGroupResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
