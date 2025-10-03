using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Group, "DataverseUpdateDuplicateDetectionRule")]
    [OutputType(typeof(CompoundUpdateDuplicateDetectionRuleResponse))]
    ///<summary>Executes CompoundUpdateDuplicateDetectionRuleRequest SDK message.</summary>
    public class CompoundDataverseUpdateDuplicateDetectionRuleCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Entity parameter")]
        public PSObject Entity { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ChildEntities parameter")]
        public Microsoft.Xrm.Sdk.EntityCollection ChildEntities { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new CompoundUpdateDuplicateDetectionRuleRequest();
            if (Entity != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in Entity.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.Entity = entity;
            }
            request.ChildEntities = ChildEntities;
            var response = (CompoundUpdateDuplicateDetectionRuleResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
