using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Unpublish, "DataverseDuplicateRule")]
    [OutputType(typeof(UnpublishDuplicateRuleResponse))]
    ///<summary>Executes UnpublishDuplicateRuleRequest SDK message.</summary>
    public class UnpublishDataverseDuplicateRuleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "DuplicateRuleId parameter")]
        public Guid DuplicateRuleId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new UnpublishDuplicateRuleRequest();
            request.DuplicateRuleId = DuplicateRuleId;
            var response = (UnpublishDuplicateRuleResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
