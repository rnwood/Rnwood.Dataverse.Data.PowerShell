using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseByIdSavedQuery")]
    [OutputType(typeof(ExecuteByIdSavedQueryResponse))]
    ///<summary>Executes ExecuteByIdSavedQueryRequest SDK message.</summary>
    public class InvokeDataverseByIdSavedQueryCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "EntityId parameter")]
        public Guid EntityId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ExecuteByIdSavedQueryRequest();
            request.EntityId = EntityId;
            var response = (ExecuteByIdSavedQueryResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
