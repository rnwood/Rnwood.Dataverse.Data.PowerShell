using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Search, "DataverseTextSearchKnowledgeArticle")]
    [OutputType(typeof(FullTextSearchKnowledgeArticleResponse))]
    ///<summary>Executes FullTextSearchKnowledgeArticleRequest SDK message.</summary>
    public class SearchDataverseTextSearchKnowledgeArticleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SearchText parameter")]
        public String SearchText { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UseInflection parameter")]
        public Boolean UseInflection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RemoveDuplicates parameter")]
        public Boolean RemoveDuplicates { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "StateCode parameter")]
        public Int32 StateCode { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QueryExpression parameter")]
        public Microsoft.Xrm.Sdk.Query.QueryBase QueryExpression { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new FullTextSearchKnowledgeArticleRequest();
            request.SearchText = SearchText;            request.UseInflection = UseInflection;            request.RemoveDuplicates = RemoveDuplicates;            request.StateCode = StateCode;            request.QueryExpression = QueryExpression;
            var response = (FullTextSearchKnowledgeArticleResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
