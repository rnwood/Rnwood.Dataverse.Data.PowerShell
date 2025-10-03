using System;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsData.Merge, "DataverseRecord", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(MergeResponse))]
    ///<summary>Merges two records of the same entity type, preserving the target and removing the subordinate.</summary>
    public class MergeDataverseRecordCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Reference to the target record (master record that will remain). Can be an EntityReference, PSObject, or Guid with TableName.")]
        public object Target { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the table when Target is specified as a Guid")]
        [Alias("EntityName", "LogicalName")]
        public string TableName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "ID of the subordinate record to merge into the target (will be deleted after merge)")]
        public Guid SubordinateId { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Entity containing additional data to update the target record with during merge")]
        public PSObject UpdateContent { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Whether to perform parenting checks during the merge. Default is false.")]
        public bool PerformParentingChecks { get; set; } = false;

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            MergeRequest request = new MergeRequest();

            // Convert Target to EntityReference
            request.Target = DataverseTypeConverter.ToEntityReference(Target, TableName, "Target");
            request.SubordinateId = SubordinateId;
            request.PerformParentingChecks = PerformParentingChecks;

            // Convert UpdateContent if provided
            if (UpdateContent != null)
            {
                Entity updateEntity = new Entity(request.Target.LogicalName);
                foreach (PSPropertyInfo prop in UpdateContent.Properties)
                {
                    if (prop.Name != "Id" && prop.Name != "TableName" && prop.Name != "LogicalName")
                    {
                        updateEntity[prop.Name] = prop.Value;
                    }
                }
                request.UpdateContent = updateEntity;
            }

            if (ShouldProcess($"Merge subordinate record {SubordinateId} into target {request.Target.LogicalName} {request.Target.Id}", "Merge records (subordinate will be deleted)"))
            {
                MergeResponse response = (MergeResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }
    }
}
