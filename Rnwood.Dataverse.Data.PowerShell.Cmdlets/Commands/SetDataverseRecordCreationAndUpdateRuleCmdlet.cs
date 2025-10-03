using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseRecordCreationAndUpdateRule", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(ApplyRecordCreationAndUpdateRuleResponse))]
    ///<summary>Executes ApplyRecordCreationAndUpdateRuleRequest SDK message.</summary>
    public class SetDataverseRecordCreationAndUpdateRuleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Target parameter")]
        public object Target { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ApplyRecordCreationAndUpdateRuleRequest();
            if (Target != null)
            {
                request.Target = DataverseTypeConverter.ToEntityReference(Target, null, "Target");
            }

            if (ShouldProcess("Executing ApplyRecordCreationAndUpdateRuleRequest", "ApplyRecordCreationAndUpdateRuleRequest"))
            {
                var response = (ApplyRecordCreationAndUpdateRuleResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
