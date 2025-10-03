using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "DataverseKnowledgeArticleTranslation", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(CreateKnowledgeArticleTranslationResponse))]
    ///<summary>Executes CreateKnowledgeArticleTranslationRequest SDK message.</summary>
    public class NewDataverseKnowledgeArticleTranslationCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Source parameter")]
        public object Source { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Language parameter")]
        public object Language { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IsMajor parameter")]
        public Boolean IsMajor { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CreateKnowledgeArticleTranslationRequest();
            if (Source != null)
            {
                request.Source = DataverseTypeConverter.ToEntityReference(Source, null, "Source");
            }
            if (Language != null)
            {
                request.Language = DataverseTypeConverter.ToEntityReference(Language, null, "Language");
            }
            request.IsMajor = IsMajor;
            if (ShouldProcess("Executing CreateKnowledgeArticleTranslationRequest", "CreateKnowledgeArticleTranslationRequest"))
            {
                var response = (CreateKnowledgeArticleTranslationResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
