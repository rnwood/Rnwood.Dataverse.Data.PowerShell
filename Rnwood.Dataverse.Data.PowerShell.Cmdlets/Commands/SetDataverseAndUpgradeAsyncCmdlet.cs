using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseAndUpgradeAsync", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(StageAndUpgradeAsyncResponse))]
    ///<summary>Executes StageAndUpgradeAsyncRequest SDK message.</summary>
    public class SetDataverseAndUpgradeAsyncCmdlet : OrganizationServiceCmdlet
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
        [Parameter(Mandatory = false, HelpMessage = "SkipQueueRibbonJob parameter")]
        public Boolean SkipQueueRibbonJob { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AsyncRibbonProcessing parameter")]
        public Boolean AsyncRibbonProcessing { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ComponentParameters parameter")]
        public Microsoft.Xrm.Sdk.EntityCollection ComponentParameters { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SolutionParameters parameter")]
        public Microsoft.Xrm.Sdk.SolutionParameters SolutionParameters { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "LayerDesiredOrder parameter")]
        public Microsoft.Xrm.Sdk.LayerDesiredOrder LayerDesiredOrder { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new StageAndUpgradeAsyncRequest();
            request.OverwriteUnmanagedCustomizations = OverwriteUnmanagedCustomizations;            request.PublishWorkflows = PublishWorkflows;            request.CustomizationFile = CustomizationFile;            request.ImportJobId = ImportJobId;            request.ConvertToManaged = ConvertToManaged;            request.SkipProductUpdateDependencies = SkipProductUpdateDependencies;            request.SkipQueueRibbonJob = SkipQueueRibbonJob;            request.AsyncRibbonProcessing = AsyncRibbonProcessing;            request.ComponentParameters = ComponentParameters;            request.SolutionParameters = SolutionParameters;            request.LayerDesiredOrder = LayerDesiredOrder;
            if (ShouldProcess("Executing StageAndUpgradeAsyncRequest", "StageAndUpgradeAsyncRequest"))
            {
                var response = (StageAndUpgradeAsyncResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
