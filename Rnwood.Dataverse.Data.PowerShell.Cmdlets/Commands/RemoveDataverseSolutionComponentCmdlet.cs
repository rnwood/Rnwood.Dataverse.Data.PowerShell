using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseSolutionComponent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RemoveSolutionComponentResponse))]
    ///<summary>Executes RemoveSolutionComponentRequest SDK message.</summary>
    public class RemoveDataverseSolutionComponentCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ComponentId parameter")]
        public Guid ComponentId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ComponentType parameter")]
        public Int32 ComponentType { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SolutionUniqueName parameter")]
        public String SolutionUniqueName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RemoveSolutionComponentRequest();
            request.ComponentId = ComponentId;            request.ComponentType = ComponentType;            request.SolutionUniqueName = SolutionUniqueName;
            if (ShouldProcess("Executing RemoveSolutionComponentRequest", "RemoveSolutionComponentRequest"))
            {
                var response = (RemoveSolutionComponentResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
