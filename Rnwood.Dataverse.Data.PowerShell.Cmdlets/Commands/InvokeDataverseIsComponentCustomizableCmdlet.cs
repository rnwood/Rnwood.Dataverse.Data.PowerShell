using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseIsComponentCustomizable")]
    [OutputType(typeof(IsComponentCustomizableResponse))]
    ///<summary>Executes IsComponentCustomizableRequest SDK message.</summary>
    public class InvokeDataverseIsComponentCustomizableCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ComponentId parameter")]
        public Guid ComponentId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ComponentType parameter")]
        public Int32 ComponentType { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new IsComponentCustomizableRequest();
            request.ComponentId = ComponentId;            request.ComponentType = ComponentType;
            var response = (IsComponentCustomizableResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
