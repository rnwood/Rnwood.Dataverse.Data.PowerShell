using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Publish, "DataverseDuplicateRule", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PublishDuplicateRuleResponse))]
    ///<summary>Executes PublishDuplicateRuleRequest SDK message.</summary>
    public class PublishDataverseDuplicateRuleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DuplicateRuleId parameter")]
        public Guid DuplicateRuleId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new PublishDuplicateRuleRequest();
            request.DuplicateRuleId = DuplicateRuleId;
            if (ShouldProcess("Executing PublishDuplicateRuleRequest", "PublishDuplicateRuleRequest"))
            {
                var response = (PublishDuplicateRuleResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
