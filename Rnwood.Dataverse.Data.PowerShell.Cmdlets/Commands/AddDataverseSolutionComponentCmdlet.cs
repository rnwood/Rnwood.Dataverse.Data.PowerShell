using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Add, "DataverseSolutionComponent", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(AddSolutionComponentResponse))]
    ///<summary>Executes AddSolutionComponentRequest SDK message.</summary>
    public class AddDataverseSolutionComponentCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ComponentId parameter")]
        public Guid ComponentId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ComponentType parameter")]
        public Int32 ComponentType { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SolutionUniqueName parameter")]
        public String SolutionUniqueName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AddRequiredComponents parameter")]
        public Boolean AddRequiredComponents { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DoNotIncludeSubcomponents parameter")]
        public Boolean DoNotIncludeSubcomponents { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IncludedComponentSettingsValues parameter")]
        public String[] IncludedComponentSettingsValues { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new AddSolutionComponentRequest();
            request.ComponentId = ComponentId;            request.ComponentType = ComponentType;            request.SolutionUniqueName = SolutionUniqueName;            request.AddRequiredComponents = AddRequiredComponents;            request.DoNotIncludeSubcomponents = DoNotIncludeSubcomponents;            request.IncludedComponentSettingsValues = IncludedComponentSettingsValues;
            if (ShouldProcess("Executing AddSolutionComponentRequest", "AddSolutionComponentRequest"))
            {
                var response = (AddSolutionComponentResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
