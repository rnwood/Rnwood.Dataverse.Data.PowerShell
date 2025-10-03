using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Update, "DataverseSolutionComponent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(UpdateSolutionComponentResponse))]
    ///<summary>Executes UpdateSolutionComponentRequest SDK message.</summary>
    public class UpdateDataverseSolutionComponentCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ComponentId parameter")]
        public Guid ComponentId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ComponentType parameter")]
        public Int32 ComponentType { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SolutionUniqueName parameter")]
        public String SolutionUniqueName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IncludedComponentSettingsValues parameter")]
        public String[] IncludedComponentSettingsValues { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new UpdateSolutionComponentRequest();
            request.ComponentId = ComponentId;            request.ComponentType = ComponentType;            request.SolutionUniqueName = SolutionUniqueName;            request.IncludedComponentSettingsValues = IncludedComponentSettingsValues;
            if (ShouldProcess("Executing UpdateSolutionComponentRequest", "UpdateSolutionComponentRequest"))
            {
                var response = (UpdateSolutionComponentResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
