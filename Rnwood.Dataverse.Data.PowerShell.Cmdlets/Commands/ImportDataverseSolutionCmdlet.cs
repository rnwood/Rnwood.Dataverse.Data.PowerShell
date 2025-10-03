using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Import, "DataverseSolution", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ImportSolutionResponse))]
    ///<summary>Executes ImportSolutionRequest SDK message.</summary>
    public class ImportDataverseSolutionCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OverwriteUnmanagedCustomizations parameter")]
        public Boolean OverwriteUnmanagedCustomizations { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PublishWorkflows parameter")]
        public Boolean PublishWorkflows { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CustomizationFile parameter")]
        public Byte[] CustomizationFile { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ImportJobId parameter")]
        public Guid ImportJobId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ConvertToManaged parameter")]
        public Boolean ConvertToManaged { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SkipProductUpdateDependencies parameter")]
        public Boolean SkipProductUpdateDependencies { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "HoldingSolution parameter")]
        public Boolean HoldingSolution { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SkipQueueRibbonJob parameter")]
        public Boolean SkipQueueRibbonJob { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "LayerDesiredOrder parameter")]
        public Microsoft.Xrm.Sdk.LayerDesiredOrder LayerDesiredOrder { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AsyncRibbonProcessing parameter")]
        public Boolean AsyncRibbonProcessing { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ComponentParameters parameter")]
        public Microsoft.Xrm.Sdk.EntityCollection ComponentParameters { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IsTemplateMode parameter")]
        public Boolean IsTemplateMode { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TemplateSuffix parameter")]
        public String TemplateSuffix { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SolutionParameters parameter")]
        public Microsoft.Xrm.Sdk.SolutionParameters SolutionParameters { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TemplateDisplayNamePrefix parameter")]
        public String TemplateDisplayNamePrefix { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ImportSolutionRequest();
            request.OverwriteUnmanagedCustomizations = OverwriteUnmanagedCustomizations;            request.PublishWorkflows = PublishWorkflows;            request.CustomizationFile = CustomizationFile;            request.ImportJobId = ImportJobId;            request.ConvertToManaged = ConvertToManaged;            request.SkipProductUpdateDependencies = SkipProductUpdateDependencies;            request.HoldingSolution = HoldingSolution;            request.SkipQueueRibbonJob = SkipQueueRibbonJob;            request.LayerDesiredOrder = LayerDesiredOrder;            request.AsyncRibbonProcessing = AsyncRibbonProcessing;            request.ComponentParameters = ComponentParameters;            request.IsTemplateMode = IsTemplateMode;            request.TemplateSuffix = TemplateSuffix;            request.SolutionParameters = SolutionParameters;            request.TemplateDisplayNamePrefix = TemplateDisplayNamePrefix;
            if (ShouldProcess("Executing ImportSolutionRequest", "ImportSolutionRequest"))
            {
                var response = (ImportSolutionResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
