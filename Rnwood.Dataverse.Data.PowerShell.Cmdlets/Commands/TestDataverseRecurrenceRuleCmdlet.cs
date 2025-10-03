using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsDiagnostic.Test, "DataverseRecurrenceRule")]
    [OutputType(typeof(ValidateRecurrenceRuleResponse))]
    ///<summary>Executes ValidateRecurrenceRuleRequest SDK message.</summary>
    public class TestDataverseRecurrenceRuleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Target parameter")]
        public PSObject Target { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new ValidateRecurrenceRuleRequest();
            if (Target != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in Target.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.Target = entity;
            }

            var response = (ValidateRecurrenceRuleResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
