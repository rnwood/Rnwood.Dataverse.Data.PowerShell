using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "DataverseKnowledgeArticleVersion", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CreateKnowledgeArticleVersionResponse))]
    ///<summary>Executes CreateKnowledgeArticleVersionRequest SDK message.</summary>
    public class NewDataverseKnowledgeArticleVersionCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Source parameter")]
        public object Source { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IsMajor parameter")]
        public Boolean IsMajor { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CreateKnowledgeArticleVersionRequest();
            if (Source != null)
            {
                request.Source = DataverseTypeConverter.ToEntityReference(Source, null, "Source");
            }
            request.IsMajor = IsMajor;
            if (ShouldProcess("Executing CreateKnowledgeArticleVersionRequest", "CreateKnowledgeArticleVersionRequest"))
            {
                var response = (CreateKnowledgeArticleVersionResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
