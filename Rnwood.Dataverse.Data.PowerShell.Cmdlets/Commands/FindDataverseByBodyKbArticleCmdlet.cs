using System;
using System.Collections;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Find, "DataverseByBodyKbArticle")]
    [OutputType(typeof(SearchByBodyKbArticleResponse))]
    ///<summary>Executes SearchByBodyKbArticleRequest SDK message.</summary>
    public class FindDataverseByBodyKbArticleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SearchText parameter")]
        public String SearchText { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SubjectId parameter")]
        public Guid SubjectId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "UseInflection parameter")]
        public Boolean UseInflection { get; set; }
        [Parameter(ParameterSetName = "QueryObject", Mandatory = false, HelpMessage = "QueryBase SDK object for complex queries")]
        public Microsoft.Xrm.Sdk.Query.QueryBase QueryExpression { get; set; }

        [Parameter(ParameterSetName = "FetchXml", Mandatory = true, HelpMessage = "FetchXML query string for filtering records")]
        public string FetchXml { get; set; }

        [Parameter(ParameterSetName = "Filter", Mandatory = true, HelpMessage = "Hashtable filter for simple queries. Use @{column='value'} or @{column=@{operator='eq';value='value'}} for complex filters")]
        public Hashtable Filter { get; set; }

        [Parameter(ParameterSetName = "Filter", Mandatory = true, HelpMessage = "Logical name of the table to query when using Filter parameter")]
        [Parameter(ParameterSetName = "FetchXml", Mandatory = false)]
        public string TableName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SearchByBodyKbArticleRequest();
            request.SearchText = SearchText;            request.SubjectId = SubjectId;            request.UseInflection = UseInflection;            
            
            // Handle PowerShell-friendly parameter sets
            if (ParameterSetName == "FetchXml" || ParameterSetName == "Filter")
            {
                request.QueryExpression = DataverseComplexTypeConverter.ToQueryBase(FetchXml, Filter, TableName);
            }
            else
            {
                request.QueryExpression = QueryExpression;
            }
            
            var response = (SearchByBodyKbArticleResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
