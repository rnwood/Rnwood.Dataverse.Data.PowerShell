using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Stop, "DataverseOpportunity")]
    [OutputType(typeof(LoseOpportunityResponse))]
    ///<summary>Executes LoseOpportunityRequest SDK message.</summary>
    public class StopDataverseOpportunityCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "OpportunityClose parameter")]
        public PSObject OpportunityClose { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Status parameter")]
        public object Status { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new LoseOpportunityRequest();
            if (OpportunityClose != null)
            {
                var entity = new Entity();
                foreach (PSPropertyInfo prop in OpportunityClose.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        entity[prop.Name] = prop.Value;
                    }
                }
                request.OpportunityClose = entity;
            }
            if (Status != null)
            {
                request.Status = DataverseTypeConverter.ToOptionSetValue(Status, "Status");
            }

            var response = (LoseOpportunityResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
