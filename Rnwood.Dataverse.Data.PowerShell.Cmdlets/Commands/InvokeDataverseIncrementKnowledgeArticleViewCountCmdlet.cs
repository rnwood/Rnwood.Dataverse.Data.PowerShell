using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "DataverseIncrementKnowledgeArticleViewCount")]
    [OutputType(typeof(IncrementKnowledgeArticleViewCountResponse))]
    ///<summary>Executes IncrementKnowledgeArticleViewCountRequest SDK message.</summary>
    public class InvokeDataverseIncrementKnowledgeArticleViewCountCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Source parameter")]
        public object Source { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ViewDate parameter")]
        public DateTime ViewDate { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Location parameter")]
        public Int32 Location { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Count parameter")]
        public Int32 Count { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new IncrementKnowledgeArticleViewCountRequest();
            if (Source != null)
            {
                request.Source = DataverseTypeConverter.ToEntityReference(Source, null, "Source");
            }
            request.ViewDate = ViewDate;            request.Location = Location;            request.Count = Count;
            var response = (IncrementKnowledgeArticleViewCountResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
