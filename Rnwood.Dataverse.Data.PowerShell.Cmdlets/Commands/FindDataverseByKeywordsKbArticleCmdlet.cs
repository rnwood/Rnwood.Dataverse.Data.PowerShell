using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Find, "DataverseByKeywordsKbArticle")]
    [OutputType(typeof(SearchByKeywordsKbArticleResponse))]
    ///<summary>Executes SearchByKeywordsKbArticleRequest SDK message.</summary>
    public class FindDataverseByKeywordsKbArticleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SearchText parameter")]
        public String SearchText { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SubjectId parameter")]
        public Guid SubjectId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UseInflection parameter")]
        public Boolean UseInflection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "QueryExpression parameter")]
        public Microsoft.Xrm.Sdk.Query.QueryBase QueryExpression { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SearchByKeywordsKbArticleRequest();
            request.SearchText = SearchText;            request.SubjectId = SubjectId;            request.UseInflection = UseInflection;            request.QueryExpression = QueryExpression;
            var response = (SearchByKeywordsKbArticleResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
