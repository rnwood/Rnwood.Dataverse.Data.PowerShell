using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "DataverseLocLabels")]
    [OutputType(typeof(RetrieveLocLabelsResponse))]
    ///<summary>Executes RetrieveLocLabelsRequest SDK message.</summary>
    public class GetDataverseLocLabelsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "EntityMoniker parameter")]
        public object EntityMoniker { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AttributeName parameter")]
        public String AttributeName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "IncludeUnpublished parameter")]
        public Boolean IncludeUnpublished { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new RetrieveLocLabelsRequest();
            if (EntityMoniker != null)
            {
                request.EntityMoniker = DataverseTypeConverter.ToEntityReference(EntityMoniker, null, "EntityMoniker");
            }
            request.AttributeName = AttributeName;            request.IncludeUnpublished = IncludeUnpublished;
            var response = (RetrieveLocLabelsResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
