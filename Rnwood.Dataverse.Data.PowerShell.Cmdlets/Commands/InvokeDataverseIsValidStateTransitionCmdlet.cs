using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseIsValidStateTransition")]
    [OutputType(typeof(IsValidStateTransitionResponse))]
    ///<summary>Executes IsValidStateTransitionRequest SDK message.</summary>
    public class InvokeDataverseIsValidStateTransitionCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Entity parameter")]
        public object Entity { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "NewState parameter")]
        public String NewState { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "NewStatus parameter")]
        public Int32 NewStatus { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new IsValidStateTransitionRequest();
            if (Entity != null)
            {
                request.Entity = DataverseTypeConverter.ToEntityReference(Entity, null, "Entity");
            }
            request.NewState = NewState;            request.NewStatus = NewStatus;
            var response = (IsValidStateTransitionResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
