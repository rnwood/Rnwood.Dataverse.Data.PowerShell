using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsLifecycle.Approve, "DataverseLead")]
    [OutputType(typeof(QualifyLeadResponse))]
    ///<summary>Executes QualifyLeadRequest SDK message.</summary>
    public class ApproveDataverseLeadCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "LeadId parameter")]
        public object LeadId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CreateAccount parameter")]
        public Boolean CreateAccount { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CreateContact parameter")]
        public Boolean CreateContact { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CreateOpportunity parameter")]
        public Boolean CreateOpportunity { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "OpportunityCurrencyId parameter")]
        public object OpportunityCurrencyId { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "OpportunityCustomerId parameter")]
        public object OpportunityCustomerId { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "SourceCampaignId parameter")]
        public object SourceCampaignId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Status parameter")]
        public object Status { get; set; }
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "ProcessInstanceId parameter")]
        public object ProcessInstanceId { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new QualifyLeadRequest();
            if (LeadId != null)
            {
                request.LeadId = DataverseTypeConverter.ToEntityReference(LeadId, null, "LeadId");
            }
            request.CreateAccount = CreateAccount;            request.CreateContact = CreateContact;            request.CreateOpportunity = CreateOpportunity;            if (OpportunityCurrencyId != null)
            {
                request.OpportunityCurrencyId = DataverseTypeConverter.ToEntityReference(OpportunityCurrencyId, null, "OpportunityCurrencyId");
            }
            if (OpportunityCustomerId != null)
            {
                request.OpportunityCustomerId = DataverseTypeConverter.ToEntityReference(OpportunityCustomerId, null, "OpportunityCustomerId");
            }
            if (SourceCampaignId != null)
            {
                request.SourceCampaignId = DataverseTypeConverter.ToEntityReference(SourceCampaignId, null, "SourceCampaignId");
            }
            if (Status != null)
            {
                request.Status = DataverseTypeConverter.ToOptionSetValue(Status, "Status");
            }
            if (ProcessInstanceId != null)
            {
                request.ProcessInstanceId = DataverseTypeConverter.ToEntityReference(ProcessInstanceId, null, "ProcessInstanceId");
            }

            var response = (QualifyLeadResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
