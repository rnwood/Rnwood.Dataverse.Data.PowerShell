using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseByTopIncidentSubjectKbArticle")]
    [OutputType(typeof(RetrieveByTopIncidentSubjectKbArticleResponse))]
    ///<summary>Executes RetrieveByTopIncidentSubjectKbArticleRequest SDK message.</summary>
    public class GetDataverseByTopIncidentSubjectKbArticleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SubjectId parameter")]
        public Guid SubjectId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveByTopIncidentSubjectKbArticleRequest();
            request.SubjectId = SubjectId;
            var response = (RetrieveByTopIncidentSubjectKbArticleResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
