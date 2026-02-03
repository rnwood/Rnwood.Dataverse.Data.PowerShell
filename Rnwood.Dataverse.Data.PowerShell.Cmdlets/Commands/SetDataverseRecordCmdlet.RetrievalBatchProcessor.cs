using Azure;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Handles batched retrieval of existing records before processing them.
    /// Supports retrieval by ID, by MatchOn criteria, and for intersect entities.
    /// </summary>
    internal partial class RetrievalBatchProcessor
    {
        private readonly IOrganizationService _connection;
        private readonly Action<string> _writeVerbose;
        private readonly Action<ErrorRecord> _writeError;
        private readonly Func<bool> _isStopping;
        private readonly int _retrievalBatchSize;
        private readonly int _retries;
        private readonly int _initialRetryDelay;
        private readonly Guid _id;
        private readonly string[][] _matchOn;
        private readonly bool _allowMultipleMatches;

        public RetrievalBatchProcessor(
            IOrganizationService connection,
            Action<string> writeVerbose,
            Action<ErrorRecord> writeError,
            Func<bool> isStopping,
            int retrievalBatchSize,
            int retries,
            int initialRetryDelay,
            Guid id,
            string[][] matchOn,
            bool allowMultipleMatches)
        {
            _connection = connection;
            _writeVerbose = writeVerbose;
            _writeError = writeError;
            _isStopping = isStopping;
            _retrievalBatchSize = retrievalBatchSize;
            _retries = retries;
            _initialRetryDelay = initialRetryDelay;
            _id = id;
            _matchOn = matchOn;
            _allowMultipleMatches = allowMultipleMatches;
            _retrievalBatchQueue = new List<RecordProcessingItem>();
            _pendingRetries = new List<RetryRecord>();
        }

        /// <summary>
        /// Adds a record to the retrieval queue.
        /// </summary>
        public void QueueForRetrieval(RecordProcessingItem item)
        {
            _retrievalBatchQueue.Add(item);
        }

        /// <summary>
        /// Gets the count of records waiting for retrieval.
        /// </summary>
        public int QueuedCount => _retrievalBatchQueue.Count;

        /// <summary>
        /// Gets the count of records waiting for retry.
        /// </summary>
        public int PendingRetryCount => _pendingRetries.Count;

        /// <summary>
        /// Processes all queued records by retrieving existing records and calling the callback.
        /// </summary>
        public void ProcessQueuedRecords(Action<PSObject, string, Guid?, Entity, EntityMetadata, Entity> processRecordWithExistingRecord)
        {
            if (_retrievalBatchQueue.Count == 0)
            {
                return;
            }

            _writeVerbose($"Processing retrieval batch of {_retrievalBatchQueue.Count} record(s)");

            // Group records by retrieval type for efficient batching
            var recordsById = _retrievalBatchQueue.Where(r =>
                !r.EntityMetadata.IsIntersect.GetValueOrDefault() &&
                (_id != Guid.Empty || r.Target.Id != Guid.Empty) &&
                _matchOn == null).ToList();

            var recordsByMatchOn = _retrievalBatchQueue.Where(r =>
                !r.EntityMetadata.IsIntersect.GetValueOrDefault() &&
                _matchOn != null).ToList();

            var recordsIntersect = _retrievalBatchQueue.Where(r =>
                r.EntityMetadata.IsIntersect.GetValueOrDefault()).ToList();

            // Batch retrieve by ID
            if (recordsById.Any())
            {
                try
                {
                    RetrieveRecordsBatchById(recordsById);
                }
                catch (Exception e)
                {
                    // Retrieval failed - schedule all records in this batch for retry
                    if (_retries > 0)
                    {
                        _writeVerbose($"Retrieval batch by ID failed, scheduling {recordsById.Count} record(s) for retry: {e.Message}");
                        foreach (var item in recordsById)
                        {
                            ScheduleRecordRetry(item.InputObject, item.TableName, item.CallerId, e);
                            // Remove from main queue so they're not processed below
                            _retrievalBatchQueue.Remove(item);
                        }
                        // Clear local list
                        recordsById.Clear();
                    }
                    else
                    {
                        // No retries - write errors and remove from queue
                        foreach (var item in recordsById)
                        {
                            RecordRetryDone(item.InputObject);
                            _writeError(new ErrorRecord(new Exception($"Error retrieving existing record: {e.Message}", e), null, ErrorCategory.InvalidOperation, item.InputObject));
                            // Remove from main queue so they're not processed below
                            _retrievalBatchQueue.Remove(item);
                        }
                        recordsById.Clear();
                    }
                }
            }

            // Batch retrieve by MatchOn
            if (recordsByMatchOn.Any())
            {
                try
                {
                    RetrieveRecordsBatchByMatchOn(recordsByMatchOn);
                }
                catch (Exception e)
                {
                    // Retrieval failed - schedule all records in this batch for retry
                    if (_retries > 0)
                    {
                        _writeVerbose($"Retrieval batch by MatchOn failed, scheduling {recordsByMatchOn.Count} record(s) for retry: {e.Message}");
                        foreach (var item in recordsByMatchOn)
                        {
                            ScheduleRecordRetry(item.InputObject, item.TableName, item.CallerId, e);
                            // Remove from main queue so they're not processed below
                            _retrievalBatchQueue.Remove(item);
                        }
                        // Clear local list
                        recordsByMatchOn.Clear();
                    }
                    else
                    {
                        // No retries - write errors and remove from queue
                        foreach (var item in recordsByMatchOn)
                        {
                            RecordRetryDone(item.InputObject);
                            _writeError(new ErrorRecord(new Exception($"Error retrieving existing record: {e.Message}", e), null, ErrorCategory.InvalidOperation, item.InputObject));
                            // Remove from main queue so they're not processed below
                            _retrievalBatchQueue.Remove(item);
                        }
                        recordsByMatchOn.Clear();
                    }
                }
            }

            // Batch retrieve intersect entities
            if (recordsIntersect.Any())
            {
                try
                {
                    RetrieveRecordsBatchIntersect(recordsIntersect);
                }
                catch (Exception e)
                {
                    // Retrieval failed - schedule all records in this batch for retry
                    if (_retries > 0)
                    {
                        _writeVerbose($"Retrieval batch for intersect entities failed, scheduling {recordsIntersect.Count} record(s) for retry: {e.Message}");
                        foreach (var item in recordsIntersect)
                        {
                            ScheduleRecordRetry(item.InputObject, item.TableName, item.CallerId, e);
                            // Remove from main queue so they're not processed below
                            _retrievalBatchQueue.Remove(item);
                        }
                        // Clear local list
                        recordsIntersect.Clear();
                    }
                    else
                    {
                        // No retries - write errors and remove from queue
                        foreach (var item in recordsIntersect)
                        {
                            RecordRetryDone(item.InputObject);
                            _writeError(new ErrorRecord(new Exception($"Error retrieving existing record: {e.Message}", e), null, ErrorCategory.InvalidOperation, item.InputObject));
                            // Remove from main queue so they're not processed below
                            _retrievalBatchQueue.Remove(item);
                        }
                        recordsIntersect.Clear();
                    }
                }
            }

            // Process all successfully queued records with their retrieved existing records
            foreach (var item in _retrievalBatchQueue)
            {
                processRecordWithExistingRecord(item.InputObject, item.TableName, item.CallerId, item.Target, item.EntityMetadata, item.ExistingRecord);
            }

            _retrievalBatchQueue.Clear();
        }

        /// <summary>
        /// Processes all pending retries that are ready.
        /// </summary>
        public void ProcessRetries(Action<PSObject, string, Guid?> processSingleRecord, Action flushBatchProcessor)
        {
            while (!_isStopping() && _pendingRetries.Where(r => !r.RetryInProgress).Any())
            {
                DateTime now = DateTime.UtcNow;
                var readyForRetry = _pendingRetries.Where(r => !r.RetryInProgress && r.NextRetryTime <= now).ToList();

                if (readyForRetry.Count == 0)
                {
                    // Calculate wait time for next retry
                    var nextRetryTime = _pendingRetries.Where(r => !r.RetryInProgress).Min(r => r.NextRetryTime);
                    var waitTime = (nextRetryTime - now).TotalSeconds;

                    if (waitTime > 0)
                    {
                        // Write verbose message once and sleep for the full duration
                        _writeVerbose($"Waiting {waitTime:F0}s for next retry...");
                        
                        // Sleep for the full wait time, checking stopping condition periodically
                        int waitTimeMs = (int)(waitTime * 1000);
                        int sleptMs = 0;
                        while (sleptMs < waitTimeMs && !_isStopping())
                        {
                            int sleepChunkMs = Math.Min(100, waitTimeMs - sleptMs);
                            Thread.Sleep(sleepChunkMs);
                            sleptMs += sleepChunkMs;
                        }
                    }

                    continue;
                }

                // Remove from pending and reprocess
                foreach (var item in readyForRetry)
                {
                    item.RetryInProgress = true;
                    _writeVerbose($"Retrying record processing...");
                    processSingleRecord(item.InputObject, item.TableName, item.CallerId);
                }

                // Process any accumulated batches after retries
                if (_retrievalBatchQueue.Count > 0)
                {
                    // Note: This will be called with processRecordWithExistingRecord callback
                    // We can't call it here directly, so we rely on the caller to handle this
                }

                // Flush batch processor if it was used
                flushBatchProcessor();
            }
        }

        /// <summary>
        /// Schedules a record for retry after a failure.
        /// </summary>
        public void ScheduleRecordRetry(PSObject inputObject, string tableName, Guid? callerId, Exception error)
        {
            // Check if this record is already scheduled for retry
            var existing = _pendingRetries.FirstOrDefault(r => ReferenceEquals(r.InputObject, inputObject));

            if (existing != null)
            {
                // Already retrying - decrement remaining count
                if (existing.RetriesRemaining > 0)
                {
                    int attemptNumber = _retries - existing.RetriesRemaining + 1;
                    int delayS = _initialRetryDelay * (int)Math.Pow(2, attemptNumber - 1);
                    existing.NextRetryTime = DateTime.UtcNow.AddSeconds(delayS);
                    existing.RetriesRemaining--;
                    existing.LastError = error;
                    existing.RetryInProgress = false;

                    _writeVerbose($"Record processing failed, will retry in {delayS}s (attempt {attemptNumber + 1} of {_retries + 1})");
                }
                else
                {
                    // No more retries - write final error
                    _pendingRetries.Remove(existing);
                    _writeError(new ErrorRecord(existing.LastError, null, ErrorCategory.InvalidResult, inputObject));
                }
            }
            else
            {
                // First failure - schedule for retry
                int delayS = _initialRetryDelay;
                _pendingRetries.Add(new RetryRecord
                {
                    InputObject = inputObject,
                    RetriesRemaining = _retries - 1,
                    NextRetryTime = DateTime.UtcNow.AddSeconds(delayS),
                    LastError = error,
                    TableName = tableName,
                    CallerId = callerId
                });

                _writeVerbose($"Record processing failed, will retry in {delayS}s (attempt 2 of {_retries + 1})");
            }
        }

        /// <summary>
        /// Marks a record as done retrying (removes from retry queue).
        /// </summary>
        public void RecordRetryDone(PSObject inputObject)
        {
            _pendingRetries.RemoveAll(r => r.InputObject == inputObject);
        }

        private void RetrieveRecordsBatchById(List<RecordProcessingItem> records)
        {
            if (records.Count == 0) return;

            var entityName = records[0].Target.LogicalName;
            var primaryIdAttribute = records[0].EntityMetadata.PrimaryIdAttribute;

            // Get all IDs to retrieve
            var ids = records.Select(r => r.Target.Id != Guid.Empty ? r.Target.Id : _id).Distinct().ToList();

            if (ids.Count == 0) return;

            // Validate that all columns are readable before building the query
            var columnSet = BuildColumnSetWithValidation(records[0].Target.Attributes.Select(a => a.Key).ToArray(), records[0].EntityMetadata);

            // Build query with In operator for efficient batching
            var query = new QueryExpression(entityName)
            {
                ColumnSet = columnSet
            };

            if (ids.Count == 1)
            {
                query.Criteria.AddCondition(primaryIdAttribute, ConditionOperator.Equal, ids[0]);
            }
            else
            {
                query.Criteria.AddCondition(primaryIdAttribute, ConditionOperator.In, ids.Cast<object>().ToArray());
            }

            _writeVerbose($"Retrieving {ids.Count} record(s) by ID in retrieval batch");

            var retrievedRecords = _connection.RetrieveMultiple(query).Entities;
            var retrievedDict = retrievedRecords.ToDictionary(e => e.Id);

            // Match retrieved records back to processing items
            foreach (var item in records)
            {
                var recordId = item.Target.Id != Guid.Empty ? item.Target.Id : _id;
                if (retrievedDict.TryGetValue(recordId, out var existingRecord))
                {
                    item.ExistingRecord = existingRecord;
                }
            }
        }

        private void RetrieveRecordsBatchByMatchOn(List<RecordProcessingItem> records)
        {
            if (records.Count == 0) return;

            var entityName = records[0].Target.LogicalName;

            foreach (string[] matchOnColumnList in _matchOn)
            {
                var recordsNeedingMatch = records.Where(r => r.ExistingRecord == null).ToList();
                if (recordsNeedingMatch.Count == 0) break;

                if (matchOnColumnList.Length == 1)
                {
                    // Single column - use In operator for efficiency
                    var matchColumn = matchOnColumnList[0];
                    var matchValues = recordsNeedingMatch.Select(r =>
                    {
                        var val = r.Target.GetAttributeValue<object>(matchColumn);
                        if (val is EntityReference er) return (object)er.Id;
                        if (val is OptionSetValue osv) return (object)osv.Value;
                        return val;
                    }).Distinct().ToList();

                    // Validate that all columns are readable before building the query
                    var columnSet = BuildColumnSetWithValidation(recordsNeedingMatch[0].Target.Attributes.Select(a => a.Key).ToArray(), recordsNeedingMatch[0].EntityMetadata);

                    var query = new QueryExpression(entityName)
                    {
                        ColumnSet = columnSet
                    };

                    if (matchValues.Count == 1)
                    {
                        query.Criteria.AddCondition(matchColumn, ConditionOperator.Equal, matchValues[0]);
                    }
                    else
                    {
                        query.Criteria.AddCondition(matchColumn, ConditionOperator.In, matchValues.ToArray());
                    }

                    _writeVerbose($"Retrieving records by MatchOn ({matchColumn}) in retrieval batch");

                    var retrievedRecords = _connection.RetrieveMultiple(query).Entities;

                    // Match back to items
                    foreach (var item in recordsNeedingMatch)
                    {
                        var itemValue = item.Target.GetAttributeValue<object>(matchColumn);
                        if (itemValue is EntityReference er) itemValue = er.Id;
                        if (itemValue is OptionSetValue osv) itemValue = osv.Value;

                        var matches = retrievedRecords.Where(e =>
                        {
                            var recValue = e.GetAttributeValue<object>(matchColumn);
                            if (recValue is EntityReference er2) recValue = er2.Id;
                            if (recValue is OptionSetValue osv2) recValue = osv2.Value;
                            return QueryHelpers.AreValuesEqual(itemValue, recValue);
                        }).ToList();

                        if (matches.Count == 1)
                        {
                            item.ExistingRecord = matches[0];
                        }
                        else if (matches.Count > 1)
                        {
                            if (_allowMultipleMatches)
                            {
                                // When multiple matches are allowed, set the first match and add duplicates for the rest
                                item.ExistingRecord = matches[0];
                                
                                // Add additional items to the queue for remaining matches
                                // Each needs a clone of the Target entity to avoid shared state
                                for (int i = 1; i < matches.Count; i++)
                                {
                                    var clonedTarget = new Entity(item.Target.LogicalName, item.Target.Id);
                                    foreach (var attr in item.Target.Attributes)
                                    {
                                        clonedTarget[attr.Key] = attr.Value;
                                    }
                                    
                                    _retrievalBatchQueue.Add(new RecordProcessingItem
                                    {
                                        InputObject = item.InputObject,
                                        Target = clonedTarget,
                                        EntityMetadata = item.EntityMetadata,
                                        ExistingRecord = matches[i],
                                        TableName = item.TableName,
                                        CallerId = item.CallerId
                                    });
                                }
                            }
                            else
                            {
                                _writeError(new ErrorRecord(new Exception($"Match on {matchColumn} resulted in more than one record. Use -AllowMultipleMatches to update all matching records."), null, ErrorCategory.InvalidOperation, item.InputObject));
                            }
                        }
                    }
                }
                else
                {
                    // Validate that all columns are readable before building the query
                    var columnSet = BuildColumnSetWithValidation(recordsNeedingMatch[0].Target.Attributes.Select(a => a.Key).ToArray(), recordsNeedingMatch[0].EntityMetadata);

                    // Multi-column - use Or with And conditions
                    var query = new QueryExpression(entityName)
                    {
                        ColumnSet = columnSet
                    };

                    var orFilter = new FilterExpression(LogicalOperator.Or);

                    foreach (var item in recordsNeedingMatch)
                    {
                        var andFilter = new FilterExpression(LogicalOperator.And);
                        foreach (var matchColumn in matchOnColumnList)
                        {
                            var queryValue = item.Target.GetAttributeValue<object>(matchColumn);
                            if (queryValue is EntityReference er1) queryValue = er1.Id;
                            if (queryValue is OptionSetValue osv1) queryValue = osv1.Value;

                            andFilter.AddCondition(matchColumn, ConditionOperator.Equal, queryValue);
                        }
                        orFilter.AddFilter(andFilter);
                    }

                    query.Criteria.AddFilter(orFilter);

                    _writeVerbose($"Retrieving records by MatchOn ({string.Join(",", matchOnColumnList)}) in retrieval batch");

                    var retrievedRecords = _connection.RetrieveMultiple(query).Entities;

                    // Match back to items
                    foreach (var item in recordsNeedingMatch)
                    {
                        var matches = retrievedRecords.Where(e =>
                        {
                            return matchOnColumnList.All(col =>
                            {
                                var itemValue = item.Target.GetAttributeValue<object>(col);
                                var recValue = e.GetAttributeValue<object>(col);

                                if (itemValue is EntityReference er1) itemValue = er1.Id;
                                if (itemValue is OptionSetValue osv1) itemValue = osv1.Value;
                                if (recValue is EntityReference er2) recValue = er2.Id;
                                if (recValue is OptionSetValue osv2) recValue = osv2.Value;

                                return QueryHelpers.AreValuesEqual(itemValue, recValue);
                            });
                        }).ToList();

                        if (matches.Count == 1)
                        {
                            item.ExistingRecord = matches[0];
                        }
                        else if (matches.Count > 1)
                        {
                            if (_allowMultipleMatches)
                            {
                                // When multiple matches are allowed, set the first match and add duplicates for the rest
                                item.ExistingRecord = matches[0];
                                
                                // Add additional items to the queue for remaining matches
                                // Each needs a clone of the Target entity to avoid shared state
                                for (int i = 1; i < matches.Count; i++)
                                {
                                    var clonedTarget = new Entity(item.Target.LogicalName, item.Target.Id);
                                    foreach (var attr in item.Target.Attributes)
                                    {
                                        clonedTarget[attr.Key] = attr.Value;
                                    }
                                    
                                    _retrievalBatchQueue.Add(new RecordProcessingItem
                                    {
                                        InputObject = item.InputObject,
                                        Target = clonedTarget,
                                        EntityMetadata = item.EntityMetadata,
                                        ExistingRecord = matches[i],
                                        TableName = item.TableName,
                                        CallerId = item.CallerId
                                    });
                                }
                            }
                            else
                            {
                                var matchOnSummary = string.Join(", ", matchOnColumnList.Select(c => $"{c}='{item.Target.GetAttributeValue<object>(c)}'"));
                                _writeError(new ErrorRecord(new Exception($"Match on values {matchOnSummary} resulted in more than one record. Use -AllowMultipleMatches to update all matching records."), null, ErrorCategory.InvalidOperation, item.InputObject));
                            }
                        }
                    }
                }
            }
        }

        private void RetrieveRecordsBatchIntersect(List<RecordProcessingItem> records)
        {
            if (records.Count == 0) return;

            var entityName = records[0].Target.LogicalName;
            var manyToManyRelationshipMetadata = records[0].EntityMetadata.ManyToManyRelationships[0];

            // Validate that all columns are readable before building the query
            var columnSet = BuildColumnSetWithValidation(records[0].Target.Attributes.Select(a => a.Key).ToArray(), records[0].EntityMetadata);

            var query = new QueryExpression(entityName)
            {
                ColumnSet = columnSet
            };

            var orFilter = new FilterExpression(LogicalOperator.Or);

            foreach (var item in records)
            {
                var entity1Value = item.Target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity1IntersectAttribute);
                var entity2Value = item.Target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity2IntersectAttribute);

                if (entity1Value.HasValue && entity2Value.HasValue)
                {
                    var andFilter = new FilterExpression(LogicalOperator.And);
                    andFilter.AddCondition(manyToManyRelationshipMetadata.Entity1IntersectAttribute, ConditionOperator.Equal, entity1Value.Value);
                    andFilter.AddCondition(manyToManyRelationshipMetadata.Entity2IntersectAttribute, ConditionOperator.Equal, entity2Value.Value);
                    orFilter.AddFilter(andFilter);
                }
            }

            if (orFilter.Filters.Count > 0)
            {
                query.Criteria.AddFilter(orFilter);

                _writeVerbose($"Retrieving {records.Count} intersect record(s) in retrieval batch");

                var retrievedRecords = _connection.RetrieveMultiple(query).Entities;

                // Match back to items
                foreach (var item in records)
                {
                    var entity1Value = item.Target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity1IntersectAttribute);
                    var entity2Value = item.Target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity2IntersectAttribute);

                    var match = retrievedRecords.FirstOrDefault(e =>
                        e.GetAttributeValue<Guid>(manyToManyRelationshipMetadata.Entity1IntersectAttribute) == entity1Value &&
                        e.GetAttributeValue<Guid>(manyToManyRelationshipMetadata.Entity2IntersectAttribute) == entity2Value);

                    if (match != null)
                    {
                        item.ExistingRecord = match;
                    }
                }
            }
        }

        /// <summary>
        /// Builds a ColumnSet with validation that all requested columns are readable.
        /// Throws an exception with helpful guidance if non-readable columns are detected.
        /// </summary>
        private ColumnSet BuildColumnSetWithValidation(string[] columnNames, EntityMetadata entityMetadata)
        {
            // Check if any of the requested columns are not valid for read
            var nonReadableColumns = new List<string>();
            
            foreach (var columnName in columnNames)
            {
                var attributeMetadata = entityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == columnName);
                
                // If the column is not valid for read, add it to the list
                if (attributeMetadata != null && !attributeMetadata.IsValidForRead.GetValueOrDefault())
                {
                    nonReadableColumns.Add(columnName);
                }
            }

            // If any non-readable columns were found, throw a clear error message
            if (nonReadableColumns.Any())
            {
                var columnList = string.Join(", ", nonReadableColumns.Select(c => $"'{c}'"));
                var message = $"Cannot retrieve existing record for comparison because the following column(s) are not valid for read: {columnList}. " +
                              $"Entity: {entityMetadata.LogicalName}. " +
                              $"To update records with non-readable columns, use one of these alternatives: " +
                              $"-Upsert (create if not exists, update if exists without comparison), " +
                              $"-NoUpdate (only create new records), " +
                              $"or -Create (fail if record exists).";
                
                throw new InvalidOperationException(message);
            }

            return new ColumnSet(columnNames);
        }

    }
}
