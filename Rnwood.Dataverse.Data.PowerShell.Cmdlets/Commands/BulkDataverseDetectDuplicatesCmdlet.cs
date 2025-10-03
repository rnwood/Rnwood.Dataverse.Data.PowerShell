using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Group, "DataverseDetectDuplicates")]
    [OutputType(typeof(BulkDetectDuplicatesResponse))]
    ///<summary>Executes BulkDetectDuplicatesRequest SDK message.</summary>
    public class BulkDataverseDetectDuplicatesCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "Query parameter")]
        public Microsoft.Xrm.Sdk.Query.QueryBase Query { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "JobName parameter")]
        public String JobName { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "SendEmailNotification parameter")]
        public Boolean SendEmailNotification { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "TemplateId parameter")]
        public Guid TemplateId { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "ToRecipients parameter")]
        public Guid[] ToRecipients { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "CCRecipients parameter")]
        public Guid[] CCRecipients { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RecurrencePattern parameter")]
        public String RecurrencePattern { get; set; }
        [Parameter(Mandatory = false, HelpMessage = "RecurrenceStartTime parameter")]
        public DateTime RecurrenceStartTime { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var request = new BulkDetectDuplicatesRequest();
            request.Query = Query;            request.JobName = JobName;            request.SendEmailNotification = SendEmailNotification;            request.TemplateId = TemplateId;            request.ToRecipients = ToRecipients;            request.CCRecipients = CCRecipients;            request.RecurrencePattern = RecurrencePattern;            request.RecurrenceStartTime = RecurrenceStartTime;
            var response = (BulkDetectDuplicatesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
