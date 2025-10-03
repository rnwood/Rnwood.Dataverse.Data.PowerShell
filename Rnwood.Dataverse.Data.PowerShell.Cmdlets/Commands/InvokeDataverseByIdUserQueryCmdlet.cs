using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseByIdUserQuery")]
    [OutputType(typeof(ExecuteByIdUserQueryResponse))]
    ///<summary>Executes ExecuteByIdUserQueryRequest SDK message.</summary>
    public class InvokeDataverseByIdUserQueryCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "EntityId parameter")]
        public object EntityId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ExecuteByIdUserQueryRequest();
            if (EntityId != null)
            {
                request.EntityId = DataverseTypeConverter.ToEntityReference(EntityId, null, "EntityId");
            }

            var response = (ExecuteByIdUserQueryResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
