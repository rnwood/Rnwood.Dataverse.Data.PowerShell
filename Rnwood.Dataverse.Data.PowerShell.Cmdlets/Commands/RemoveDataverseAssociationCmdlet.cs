using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseAssociation", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(DisassociateResponse))]
    ///<summary>Removes a many-to-many relationship between records.</summary>
    public class RemoveDataverseAssociationCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true, HelpMessage = "DataverseConnection instance obtained from Get-DataverseConnection cmdlet")]
        public override ServiceClient Connection { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Reference to the primary record. Can be an EntityReference, a PSObject with Id and TableName properties, or a Guid (requires TableName parameter).")]
        public object Target { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the table when Target is specified as a Guid")]
        [Alias("EntityName", "LogicalName")]
        public string TableName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Name of the many-to-many relationship schema name (e.g., 'systemuserroles_association')")]
        public string RelationshipName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "References to the related records to disassociate. Can be EntityReferences, PSObjects with Id and TableName properties, or Guids (requires RelatedTableName parameter).")]
        public object[] RelatedRecords { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Logical name of the related table when RelatedRecords are specified as Guids")]
        public string RelatedTableName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            DisassociateRequest request = new DisassociateRequest();

            // Convert Target to EntityReference
            request.Target = ConvertToEntityReference(Target, TableName, "Target");

            // Set relationship
            request.Relationship = new Relationship(RelationshipName);

            // Convert RelatedRecords to EntityReferenceCollection
            request.RelatedEntities = new EntityReferenceCollection();
            foreach (var record in RelatedRecords)
            {
                var entityRef = ConvertToEntityReference(record, RelatedTableName, "RelatedRecords");
                request.RelatedEntities.Add(entityRef);
            }

            if (ShouldProcess($"Record {request.Target.LogicalName} {request.Target.Id}", $"Disassociate {request.RelatedEntities.Count} related record(s) via {RelationshipName}"))
            {
                DisassociateResponse response = (DisassociateResponse)Connection.Execute(request);
                WriteObject(response);
            }
        }

        private EntityReference ConvertToEntityReference(object value, string tableName, string parameterName)
        {
            if (value is EntityReference entityRef)
            {
                return entityRef;
            }
            else if (value is PSObject psObj)
            {
                var idProp = psObj.Properties["Id"];
                var tableNameProp = psObj.Properties["TableName"] ?? psObj.Properties["LogicalName"];

                if (idProp != null && tableNameProp != null)
                {
                    return new EntityReference((string)tableNameProp.Value, (Guid)idProp.Value);
                }
            }
            else if (value is Guid guid)
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    throw new ArgumentException($"TableName parameter is required when {parameterName} is specified as a Guid");
                }
                return new EntityReference(tableName, guid);
            }
            else if (value is string strValue && Guid.TryParse(strValue, out Guid parsedGuid))
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    throw new ArgumentException($"TableName parameter is required when {parameterName} is specified as a string Guid");
                }
                return new EntityReference(tableName, parsedGuid);
            }

            throw new ArgumentException($"Unable to convert {parameterName} to EntityReference. Expected EntityReference, PSObject with Id and TableName properties, or Guid with TableName parameter.");
        }
    }
}
