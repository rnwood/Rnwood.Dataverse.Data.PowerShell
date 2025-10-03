using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseByTopIncidentProductKbArticle")]
    [OutputType(typeof(RetrieveByTopIncidentProductKbArticleResponse))]
    ///<summary>Executes RetrieveByTopIncidentProductKbArticleRequest SDK message.</summary>
    public class GetDataverseByTopIncidentProductKbArticleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ProductId parameter")]
        public Guid ProductId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveByTopIncidentProductKbArticleRequest();
            request.ProductId = ProductId;
            var response = (RetrieveByTopIncidentProductKbArticleResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
