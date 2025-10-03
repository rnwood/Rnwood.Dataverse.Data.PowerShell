using System;
using System.Collections;
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
        
        // Query parameter sets for PowerShell-friendly usage
        [Parameter(ParameterSetName = "QueryObject", Mandatory = false, HelpMessage = "QueryBase object from the SDK (QueryExpression or FetchExpression)")]
        public Microsoft.Xrm.Sdk.Query.QueryBase Query { get; set; }
        
        [Parameter(ParameterSetName = "FetchXml", Mandatory = true, HelpMessage = "FetchXML query string for duplicate detection")]
        public string FetchXml { get; set; }
        
        [Parameter(ParameterSetName = "Filter", Mandatory = true, HelpMessage = "Hashtable with filter conditions (e.g., @{firstname='John'; lastname='Doe'})")]
        public Hashtable Filter { get; set; }
        
        [Parameter(ParameterSetName = "Filter", Mandatory = true, HelpMessage = "Logical name of the Dataverse table to query")]
        [Parameter(ParameterSetName = "FetchXml", Mandatory = false, HelpMessage = "Logical name of the Dataverse table (optional with FetchXML)")]
        public string TableName { get; set; }
        
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
            
            // Handle query parameter conversion
            if (ParameterSetName == "FetchXml" || ParameterSetName == "Filter")
            {
                request.Query = DataverseComplexTypeConverter.ToQueryBase(FetchXml, Filter, TableName);
            }
            else
            {
                request.Query = Query;
            }
            
            request.JobName = JobName;
            request.SendEmailNotification = SendEmailNotification;
            request.TemplateId = TemplateId;
            request.ToRecipients = ToRecipients;
            request.CCRecipients = CCRecipients;
            request.RecurrencePattern = RecurrencePattern;
            request.RecurrenceStartTime = RecurrenceStartTime;
            
            var response = (BulkDetectDuplicatesResponse)Connection.Execute(request);
            WriteObject(response);
        }
    }
}
