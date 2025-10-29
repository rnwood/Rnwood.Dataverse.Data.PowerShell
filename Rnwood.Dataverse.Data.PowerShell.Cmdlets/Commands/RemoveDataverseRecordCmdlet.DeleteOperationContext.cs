using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Handles the complete lifecycle of a delete operation for a single record, including
    /// request creation, execution, error handling, and retry logic.
    /// </summary>
    internal class DeleteOperationContext : IDeleteOperationParameters
        {
            private readonly Action<string> _writeVerbose;
            private readonly Action<ErrorRecord> _writeError;
            private readonly Func<string, bool> _shouldProcess;

            public DeleteOperationContext(
                PSObject inputObject,
                string tableName,
                Guid id,
                IDeleteOperationParameters parameters,
                EntityMetadataFactory metadataFactory,
                IOrganizationService connection,
                Action<string> writeVerbose,
                Action<ErrorRecord> writeError,
                Func<string, bool> shouldProcess)
            {
                InputObject = inputObject;
                TableName = tableName;
                Id = id;
                BypassBusinessLogicExecution = parameters.BypassBusinessLogicExecution;
                BypassBusinessLogicExecutionStepIds = parameters.BypassBusinessLogicExecutionStepIds;
                IfExists = parameters.IfExists;
                Retries = parameters.Retries;
                InitialRetryDelay = parameters.InitialRetryDelay;
                MatchOn = parameters.MatchOn;
                AllowMultipleMatches = parameters.AllowMultipleMatches;
                RetriesRemaining = parameters.Retries;
                NextRetryTime = DateTime.MinValue;
                MetadataFactory = metadataFactory;
                Connection = connection;
                _writeVerbose = writeVerbose;
                _writeError = writeError;
                _shouldProcess = shouldProcess;
            }

            public PSObject InputObject { get; }
            public string TableName { get; }
            public Guid Id { get; }
            public CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] BypassBusinessLogicExecution { get; }
            public Guid[] BypassBusinessLogicExecutionStepIds { get; }
            public bool IfExists { get; }
            public int Retries { get; }
            public int InitialRetryDelay { get; }
            public string[][] MatchOn { get; }
            public bool AllowMultipleMatches { get; }
            public int RetriesRemaining { get; set; }
            public DateTime NextRetryTime { get; set; }
            public OrganizationRequest Request { get; set; }
            public EntityMetadataFactory MetadataFactory { get; }
            public IOrganizationService Connection { get; }

            /// <summary>
            /// Creates the delete request for this operation, handling both regular and intersect records.
            /// </summary>
            public void CreateRequest()
            {
                EntityMetadata metadata = MetadataFactory.GetMetadata(TableName);

                if (metadata.IsIntersect.GetValueOrDefault())
                {
                    // For intersect (many-to-many) records, we need to use DisassociateRequest
                    ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = metadata.ManyToManyRelationships[0];

                    QueryExpression getRecordWithMMColumns = new QueryExpression(TableName);
                    getRecordWithMMColumns.ColumnSet = new ColumnSet(
                        manyToManyRelationshipMetadata.Entity1IntersectAttribute,
                        manyToManyRelationshipMetadata.Entity2IntersectAttribute);
                    getRecordWithMMColumns.Criteria.AddCondition(metadata.PrimaryIdAttribute, ConditionOperator.Equal, Id);

                    Entity record = Connection.RetrieveMultiple(getRecordWithMMColumns).Entities.Single();

                    DisassociateRequest request = new DisassociateRequest()
                    {
                        Target = new EntityReference(manyToManyRelationshipMetadata.Entity1LogicalName,
                            record.GetAttributeValue<Guid>(manyToManyRelationshipMetadata.Entity1IntersectAttribute)),
                        RelatedEntities = new EntityReferenceCollection()
                        {
                            new EntityReference(manyToManyRelationshipMetadata.Entity2LogicalName,
                                record.GetAttributeValue<Guid>(manyToManyRelationshipMetadata.Entity2IntersectAttribute))
                        },
                        Relationship = new Relationship(manyToManyRelationshipMetadata.SchemaName) { PrimaryEntityRole = EntityRole.Referencing }
                    };
                    QueryHelpers.ApplyBypassBusinessLogicExecution(request, BypassBusinessLogicExecution, BypassBusinessLogicExecutionStepIds);
                    Request = request;
                }
                else
                {
                    DeleteRequest request = new DeleteRequest { Target = new EntityReference(TableName, Id) };
                    QueryHelpers.ApplyBypassBusinessLogicExecution(request, BypassBusinessLogicExecution, BypassBusinessLogicExecutionStepIds);
                    Request = request;
                }
            }

            /// <summary>
            /// Executes the delete operation without batching.
            /// </summary>
            public void ExecuteNonBatched()
            {
                if (_shouldProcess(string.Format("Delete record {0}:{1}", TableName, Id)))
                {
                    try
                    {
                        Connection.Execute(Request);
                        _writeVerbose(string.Format("Deleted record {0}:{1}", TableName, Id));
                    }
                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        // Check for "record not found" error code OR if message contains "Does Not Exist"
                        // Different versions of FakeXrmEasy may set ErrorCode differently (sometimes 0, sometimes -2147220969)
                        if (IfExists && (ex.Detail.ErrorCode == -2147220969 || ex.Message.Contains("Does Not Exist")))
                        {
                            _writeVerbose(string.Format("Record {0}:{1} was not present", TableName, Id));
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch (FaultException ex) when (IfExists)
                    {
                        // FakeXrmEasy may throw non-generic FaultException in some cases
                        // Check if message indicates "Does Not Exist"
                        if (ex.Message.Contains("Does Not Exist"))
                        {
                            _writeVerbose(string.Format("Record {0}:{1} was not present", TableName, Id));
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            /// <summary>
            /// Handles a fault that occurred during batch execution.
            /// </summary>
            /// <returns>True if the fault was handled and should not be reported as an error.</returns>
            public bool HandleFault(OrganizationServiceFault fault)
            {
                // Handle specific error codes that should be ignored
                if (IfExists && fault.ErrorCode == -2147220969)
                {
                    _writeVerbose(string.Format("Record {0}:{1} was not present", TableName, Id));
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Called when the delete operation completes successfully.
            /// </summary>
            public void Complete()
            {
                _writeVerbose(string.Format("Deleted record {0}:{1}", TableName, Id));
            }

            /// <summary>
            /// Schedules this operation for retry after a failure.
            /// </summary>
            public void ScheduleRetry(Exception e)
            {
                // Schedule for retry with exponential backoff
                int attemptNumber = Retries - RetriesRemaining + 1;
                int delayS = InitialRetryDelay * (int)Math.Pow(2, attemptNumber - 1);
                NextRetryTime = DateTime.UtcNow.AddSeconds(delayS);
                RetriesRemaining--;

                _writeVerbose($"Request failed, will retry in {delayS}s (attempt {attemptNumber} of {Retries + 1}): {this}\n{e}");
            }

            /// <summary>
            /// Reports an error for this operation.
            /// </summary>
            public void ReportError(Exception e)
            {
                _writeError(new ErrorRecord(e, null, ErrorCategory.InvalidResult, InputObject));
            }



            public override string ToString()
            {
                return $"Delete {TableName}:{Id}";
            }
    }
}
