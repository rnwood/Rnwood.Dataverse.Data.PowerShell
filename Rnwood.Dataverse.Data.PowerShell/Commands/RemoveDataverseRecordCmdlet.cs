


using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Remove, "DataverseRecord", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    ///<summary>Deletes records from a Dataverse organization.</summary>
    public class RemoveDataverseRecordCmdlet : OrganizationServiceCmdlet
    {
        [Parameter(Mandatory = true)]
        public override ServiceClient Connection { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of table")]
        [Alias("EntityName")]
        public string TableName { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Id of record to process")]
        public Guid Id { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Delete(TableName, Id);
        }

        private void Delete(string entityName, Guid id)
        {
            EntityMetadata metadata = metadataFactory.GetMetadata(entityName);

            if (metadata.IsIntersect.GetValueOrDefault())
            {
                if (ShouldProcess(string.Format("Delete intersect record {0}:{1}", entityName, id)))
                {
                    ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = metadata.ManyToManyRelationships[0];

                    QueryExpression getRecordWithMMColumns = new QueryExpression(TableName);
                    getRecordWithMMColumns.ColumnSet = new ColumnSet(manyToManyRelationshipMetadata.Entity1IntersectAttribute, manyToManyRelationshipMetadata.Entity2IntersectAttribute);
                    getRecordWithMMColumns.Criteria.AddCondition(metadata.PrimaryIdAttribute, ConditionOperator.Equal, id);

                    Entity record = Connection.RetrieveMultiple(getRecordWithMMColumns).Entities.Single();

                    Connection.Execute(new DisassociateRequest()
                    {
                        Target = new EntityReference(manyToManyRelationshipMetadata.Entity1LogicalName,
                                                     record.GetAttributeValue<Guid>(
                                                         manyToManyRelationshipMetadata.Entity1IntersectAttribute)),
                        RelatedEntities =
                            new EntityReferenceCollection()
                                {
                                    new EntityReference(manyToManyRelationshipMetadata.Entity2LogicalName,
                                                        record.GetAttributeValue<Guid>(
                                                            manyToManyRelationshipMetadata.Entity2IntersectAttribute))
                                },
                        Relationship = new Relationship(manyToManyRelationshipMetadata.SchemaName) { PrimaryEntityRole = EntityRole.Referencing }
                    });

                    WriteVerbose(string.Format("Deleted intersect record {0}:{1}", entityName, id));
                }
            }
            else
            {
                if (ShouldProcess(string.Format("Delete record {0}:{1}", entityName, id)))
                {
                    Connection.Delete(entityName, id);
                }
                WriteVerbose(string.Format("Deleted record {0}:{1}", entityName, id));
            }
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            metadataFactory = new EntityMetadataFactory(Connection);
        }

        private EntityMetadataFactory metadataFactory;

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }
    }
}