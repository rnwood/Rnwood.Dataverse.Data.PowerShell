using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseProcessInstances")]
    [OutputType(typeof(RetrieveProcessInstancesResponse))]
    ///<summary>Executes RetrieveProcessInstancesRequest SDK message.</summary>
    public class GetDataverseProcessInstancesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityId parameter")]
        public Guid EntityId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityLogicalName parameter")]
        public String EntityLogicalName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveProcessInstancesRequest();
            request.EntityId = EntityId;            request.EntityLogicalName = EntityLogicalName;
            var response = (RetrieveProcessInstancesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
