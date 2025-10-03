using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "DataverseRecordOwner", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(AssignResponse))]
    ///<summary>Assigns a record to a different user or team.</summary>
    public class SetDataverseRecordOwnerCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Reference to the record to assign. Can be an EntityReference, a PSObject with Id and TableName properties, or a Guid (requires TableName parameter).")]
        public object Target { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the table when Target is specified as a Guid")]
        [Alias("EntityName", "LogicalName")]
        public string TableName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Reference to the user or team to assign the record to. Can be an EntityReference, a PSObject with Id and TableName properties, or a Guid (requires AssigneeTableName parameter).")]
        public object Assignee { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Logical name of the assignee table (systemuser or team) when Assignee is specified as a Guid")]
        public string AssigneeTableName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            AssignRequest request = new AssignRequest();

            // Convert Target to EntityReference
            request.Target = DataverseTypeConverter.ToEntityReference(Target, TableName, "Target");

            // Convert Assignee to EntityReference
            request.Assignee = DataverseTypeConverter.ToEntityReference(Assignee, AssigneeTableName, "Assignee");

            if (ShouldProcess($"Record {request.Target.LogicalName} {request.Target.Id}", $"Assign to {request.Assignee.LogicalName} {request.Assignee.Id}"))
            {
                AssignResponse response = (AssignResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
