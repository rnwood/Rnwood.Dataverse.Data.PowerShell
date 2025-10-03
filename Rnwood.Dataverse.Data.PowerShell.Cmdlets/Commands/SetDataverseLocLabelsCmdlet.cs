using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseLocLabels", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(SetLocLabelsResponse))]
    ///<summary>Executes SetLocLabelsRequest SDK message.</summary>
    public class SetDataverseLocLabelsCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "EntityMoniker parameter")]
        public object EntityMoniker { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "AttributeName parameter")]
        public String AttributeName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Labels parameter")]
        public Microsoft.Xrm.Sdk.LocalizedLabel[] Labels { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new SetLocLabelsRequest();
            if (EntityMoniker != null)
            {
                request.EntityMoniker = DataverseTypeConverter.ToEntityReference(EntityMoniker, null, "EntityMoniker");
            }
            request.AttributeName = AttributeName;            request.Labels = Labels;
            if (ShouldProcess("Executing SetLocLabelsRequest", "SetLocLabelsRequest"))
            {
                var response = (SetLocLabelsResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
