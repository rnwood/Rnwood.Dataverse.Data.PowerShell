using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Start, "DataverseServiceEndpointCheck")]
    [OutputType(typeof(TriggerServiceEndpointCheckResponse))]
    ///<summary>Executes TriggerServiceEndpointCheckRequest SDK message.</summary>
    public class StartDataverseServiceEndpointCheckCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Entity parameter")]
        public object Entity { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new TriggerServiceEndpointCheckRequest();
            if (Entity != null)
            {
                request.Entity = DataverseTypeConverter.ToEntityReference(Entity, null, "Entity");
            }

            var response = (TriggerServiceEndpointCheckResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
