using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Import, "DataverseSolutions", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ImportSolutionsResponse))]
    ///<summary>Executes ImportSolutionsRequest SDK message.</summary>
    public class ImportDataverseSolutionsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OverwriteUnmanagedCustomizations parameter")]
        public Boolean OverwriteUnmanagedCustomizations { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "PublishWorkflows parameter")]
        public Boolean PublishWorkflows { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CustomizationFiles parameter")]
        public Byte[] CustomizationFiles { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ImportJobId parameter")]
        public Guid ImportJobId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ConvertToManaged parameter")]
        public Boolean ConvertToManaged { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SkipProductUpdateDependencies parameter")]
        public Boolean SkipProductUpdateDependencies { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "HoldingSolution parameter")]
        public Boolean HoldingSolution { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ImportSolutionsRequest();
            request.OverwriteUnmanagedCustomizations = OverwriteUnmanagedCustomizations;            request.PublishWorkflows = PublishWorkflows;            request.CustomizationFiles = CustomizationFiles;            request.ImportJobId = ImportJobId;            request.ConvertToManaged = ConvertToManaged;            request.SkipProductUpdateDependencies = SkipProductUpdateDependencies;            request.HoldingSolution = HoldingSolution;
            if (ShouldProcess("Executing ImportSolutionsRequest", "ImportSolutionsRequest"))
            {
                var response = (ImportSolutionsResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
