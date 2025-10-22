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
    internal class SetOperationContext : ISetOperationParameters
    {
        private readonly Action<string> _writeVerbose;
        private readonly Action<ErrorRecord> _writeError;
        private readonly Action<object> _writeObject;
        private readonly Func<string, bool> _shouldProcess;

        public SetOperationContext(
            PSObject inputObject,
            string tableName,
            Guid? callerId,
            ISetOperationParameters parameters,
            EntityMetadataFactory metadataFactory,
            DataverseEntityConverter entityConverter,
            IOrganizationService connection,
            ConvertToDataverseEntityOptions conversionOptions,
            Action<string> writeVerbose,
            Action<ErrorRecord> writeError,
            Action<object> writeObject,
            Func<string, bool> shouldProcess)
        {
            InputObject = inputObject;
            TableName = tableName;
            CallerId = callerId;
            BypassBusinessLogicExecution = parameters.BypassBusinessLogicExecution;
            BypassBusinessLogicExecutionStepIds = parameters.BypassBusinessLogicExecutionStepIds;
            NoUpdate = parameters.NoUpdate;
            NoCreate = parameters.NoCreate;
            CreateOnly = parameters.CreateOnly;
            Upsert = parameters.Upsert;
            PassThru = parameters.PassThru;
            UpdateAllColumns = parameters.UpdateAllColumns;
            NoUpdateColumns = parameters.NoUpdateColumns;
            MatchOn = parameters.MatchOn;
            Id = parameters.Id;
            AllowMultipleMatches = parameters.AllowMultipleMatches;
            Retries = parameters.Retries;
            InitialRetryDelay = parameters.InitialRetryDelay;
            RetriesRemaining = parameters.Retries;
            NextRetryTime = DateTime.MinValue;
            MetadataFactory = metadataFactory;
            EntityConverter = entityConverter;
            Connection = connection;
            ConversionOptions = conversionOptions;
            _writeVerbose = writeVerbose;
            _writeError = writeError;
            _writeObject = writeObject;
            _shouldProcess = shouldProcess;
            Requests = new List<OrganizationRequest>();
            ResponseCompletions = new List<Action<OrganizationResponse>>();
        }

        public PSObject InputObject { get; }
        public string TableName { get; }
        public Guid? CallerId { get; }
        public CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] BypassBusinessLogicExecution { get; }
        public Guid[] BypassBusinessLogicExecutionStepIds { get; }
        public bool NoUpdate { get; }
        public bool NoCreate { get; }
        public bool CreateOnly { get; }
        public bool Upsert { get; }
        public bool PassThru { get; }
        public bool UpdateAllColumns { get; }
        public string[] NoUpdateColumns { get; }
        public string[][] MatchOn { get; }
        public Guid Id { get; }
        public bool AllowMultipleMatches { get; }
        public int Retries { get; }
        public int InitialRetryDelay { get; }
        public int RetriesRemaining { get; set; }
        public DateTime NextRetryTime { get; set; }
        public List<OrganizationRequest> Requests { get; set; }
        public EntityMetadataFactory MetadataFactory { get; }
        public DataverseEntityConverter EntityConverter { get; }
        public IOrganizationService Connection { get; }
        public ConvertToDataverseEntityOptions ConversionOptions { get; }
        public Entity Target { get; set; }
        public EntityMetadata EntityMetadata { get; set; }
        public Entity ExistingRecord { get; set; }
        
        /// <summary>
        /// Callbacks to invoke when each request completes successfully.
        /// One callback per request in the Requests list.
        /// </summary>
        public List<Action<OrganizationResponse>> ResponseCompletions { get; set; }
        
        /// <summary>
        /// Callback to invoke when the operation encounters a fault.
        /// Returns true if the fault was handled, false otherwise.
        /// </summary>
        public Func<OrganizationServiceFault, bool> ResponseExceptionCompletion { get; set; }
        
        /// <summary>
        /// Callback to invoke when the operation completes successfully.
        /// This is a convenience property for single-request contexts.
        /// Setting this property adds the callback to ResponseCompletions.
        /// </summary>
        public Action<OrganizationResponse> ResponseCompletion 
        { 
            get => ResponseCompletions.Count > 0 ? ResponseCompletions[0] : null;
            set 
            {
                if (ResponseCompletions.Count == 0)
                {
                    ResponseCompletions.Add(value);
                }
                else
                {
                    ResponseCompletions[0] = value;
                }
            }
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

        /// <summary>
        /// Gets a summary of the key for an entity (ID or alternate keys).
        /// </summary>
        public static string GetKeySummary(Entity record)
        {
            if (record.Id != Guid.Empty)
            {
                return record.Id.ToString();
            }

            if (record.KeyAttributes.Any())
            {
                return string.Join(",", record.KeyAttributes.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            }

            return "<No ID>";
        }

        /// <summary>
        /// Truncates a string value to 100 characters with ellipsis.
        /// </summary>
        public static string Ellipsis(string value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Length <= 100)
            {
                return value;
            }

            return value.Substring(0, 100) + "...";
        }

        /// <summary>
        /// Gets a summary of values, handling collections.
        /// </summary>
        public static object GetValueSummary(object value)
        {
            if ((!(value is string)) && value is IEnumerable enumerable)
            {
                return "[" + string.Join(", ", enumerable.Cast<object>().Select(i => GetValueSummary(i))) + "]";
            }

            return value ?? "<null>";
        }

        /// <summary>
        /// Gets a formatted summary of all columns in an entity.
        /// </summary>
        public static string GetColumnSummary(Entity entity, DataverseEntityConverter converter)
        {
            PSObject psObject = converter.ConvertToPSObject(entity, new ColumnSet(entity.Attributes.Select(a => a.Key).ToArray()), a => ValueType.Raw);
            return string.Join("\n", psObject.Properties.Select(a => a.Name + " = " + Ellipsis((GetValueSummary(a.Value)).ToString())));
        }

        /// <summary>
        /// Column names that should not be updated directly - they require special handling.
        /// </summary>
        public static readonly string[] DontUpdateDirectlyColumnNames = new[] { "statuscode", "statecode", "ownerid" };

        /// <summary>
        /// Removes unchanged columns from target entity by comparing with existing record.
        /// Also sets target.Id to existing record's Id.
        /// </summary>
        public static void RemoveUnchangedColumns(Entity target, Entity existingRecord)
        {
            foreach (KeyValuePair<string, object> column in target.Attributes.ToArray())
            {
                if ((existingRecord.Contains(column.Key) && Equals(column.Value, existingRecord[column.Key]))
                    ||
                    //Dataverse seems to consider that null and "" string are equal and doesn't include the attribute in retrieve records if the value is either
                    ((column.Value == null || column.Value as string == "") && !existingRecord.Contains(column.Key)))
                {
                    target.Attributes.Remove(column.Key);
                }
                else if (existingRecord.GetAttributeValue<object>(column.Key) is OptionSetValueCollection existingCollection && target.GetAttributeValue<object>(column.Key) is OptionSetValueCollection targetCollection)
                {
                    // Compare OptionSetValueCollection by values, not by reference
                    // Collections are considered equal if they have the same values with same frequencies, regardless of order
                    if (existingCollection.Count == targetCollection.Count)
                    {
                        // Create sorted lists of values for comparison
                        var existingValues = existingCollection.Select(o => o.Value).OrderBy(v => v).ToList();
                        var targetValues = targetCollection.Select(o => o.Value).OrderBy(v => v).ToList();
                        
                        // Compare sorted lists - this handles duplicates and order correctly
                        if (existingValues.SequenceEqual(targetValues))
                        {
                            target.Attributes.Remove(column.Key);
                        }
                    }
                }
            }

            target.Id = existingRecord.Id;
        }

        /// <summary>
        /// Applies bypass business logic execution parameters to a request.
        /// </summary>
        public void ApplyBypassBusinessLogicExecution(OrganizationRequest request)
        {
            QueryHelpers.ApplyBypassBusinessLogicExecution(request, BypassBusinessLogicExecution, BypassBusinessLogicExecutionStepIds);
        }

        /// <summary>
        /// Sets the Id property on a PSObject, removing any existing Id property first.
        /// </summary>
        public static void SetIdProperty(PSObject inputObject, Guid id)
        {
            if (inputObject.Properties.Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
            {
                inputObject.Properties.Remove("Id");
            }
            inputObject.Properties.Add(new PSNoteProperty("Id", id));
        }

        /// <summary>
        /// Converts an input object to a PSObject with the Id property set for PassThru output.
        /// Handles hashtables, Entity objects, and PSObjects uniformly.
        /// </summary>
        public static PSObject ConvertInputToPSObjectForPassThru(PSObject inputObject, Guid id)
        {
            PSObject result = new PSObject();

            // Add Id first
            result.Properties.Add(new PSNoteProperty("Id", id));

            // Handle different input types
            if (inputObject.ImmediateBaseObject is Hashtable ht)
            {
                // Input is a hashtable - copy all properties from hashtable
                foreach (DictionaryEntry entry in ht)
                {
                    string propName = (string)entry.Key;
                    // Skip Id if it exists in the hashtable - we've already set it
                    if (!propName.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Properties.Add(new PSNoteProperty(propName, entry.Value));
                    }
                }
            }
            else if (inputObject.ImmediateBaseObject is Entity entity)
            {
                // Input is an Entity object - copy logical name and attributes
                result.Properties.Add(new PSNoteProperty("TableName", entity.LogicalName));
                foreach (KeyValuePair<string, object> attr in entity.Attributes)
                {
                    // Skip the Id attribute - we've already set it
                    if (!attr.Key.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                        !attr.Key.Equals(entity.LogicalName + "id", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Properties.Add(new PSNoteProperty(attr.Key, attr.Value));
                    }
                }
            }
            else
            {
                // Input is already a PSObject or other type - copy all properties
                foreach (PSPropertyInfo prop in inputObject.Properties)
                {
                    // Skip Id if it exists - we've already set it
                    if (!prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            result.Properties.Add(new PSNoteProperty(prop.Name, prop.Value));
                        }
                        catch
                        {
                            // Some properties might not be copyable (e.g., script properties)
                            // Skip them silently
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Determines if a record needs to be retrieved from the server before processing.
        /// </summary>
        public bool NeedsRetrieval(EntityMetadata entityMetadata, Entity target)
        {
            if (CreateOnly || Upsert)
            {
                return false;
            }

            if (!entityMetadata.IsIntersect.GetValueOrDefault())
            {
                if (Id != Guid.Empty && UpdateAllColumns)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets existing records from the server using Id or MatchOn criteria.
        /// Returns a list of matching records. When AllowMultipleMatches is false, list contains at most one record.
        /// </summary>
        public List<Entity> GetExistingRecords(EntityMetadata entityMetadata, Entity target)
        {
            List<Entity> existingRecords = new List<Entity>();

            if (CreateOnly || Upsert)
            {
                return existingRecords;
            }

            if (!entityMetadata.IsIntersect.GetValueOrDefault())
            {
                if (Id != Guid.Empty)
                {
                    if (UpdateAllColumns)
                    {
                        Entity existingRecord = new Entity(target.LogicalName) { Id = Id };
                        existingRecord[entityMetadata.PrimaryIdAttribute] = Id;
                        existingRecords.Add(existingRecord);
                    }
                    else
                    {
                        QueryByAttribute existingRecordQuery = new QueryByAttribute(TableName);
                        existingRecordQuery.AddAttributeValue(entityMetadata.PrimaryIdAttribute, Id);
                        existingRecordQuery.ColumnSet = target.LogicalName.Equals("calendar", StringComparison.OrdinalIgnoreCase) ? new ColumnSet(true) : new ColumnSet(target.Attributes.Select(a => a.Key).ToArray());

                        var record = Connection.RetrieveMultiple(existingRecordQuery).Entities.FirstOrDefault();
                        if (record != null)
                        {
                            existingRecords.Add(record);
                        }
                    }
                }

                if (existingRecords.Count == 0 && MatchOn != null)
                {
                    foreach (string[] matchOnColumnList in MatchOn)
                    {
                        QueryByAttribute matchOnQuery = new QueryByAttribute(TableName);
                        if (!AllowMultipleMatches)
                        {
                            matchOnQuery.TopCount = 2; // Get 2 to detect multiple matches
                        }

                        foreach (string matchOnColumn in matchOnColumnList)
                        {
                            object queryValue = target.GetAttributeValue<object>(matchOnColumn);

                            if (queryValue is EntityReference)
                            {
                                queryValue = ((EntityReference)queryValue).Id;
                            }

                            if (queryValue is OptionSetValue)
                            {
                                queryValue = ((OptionSetValue)queryValue).Value;
                            }

                            matchOnQuery.AddAttributeValue(matchOnColumn, queryValue);
                        }

                        matchOnQuery.ColumnSet = new ColumnSet(target.Attributes.Select(a => a.Key).ToArray());

                        var matchedRecords = Connection.RetrieveMultiple(matchOnQuery).Entities;

                        if (matchedRecords.Count == 1)
                        {
                            existingRecords.Add(matchedRecords[0]);
                            break;
                        }
                        else if (matchedRecords.Count > 1)
                        {
                            if (AllowMultipleMatches)
                            {
                                // Add all matching records when multiple matches are allowed
                                existingRecords.AddRange(matchedRecords);
                                break;
                            }
                            else
                            {
                                string matchOnSummary = string.Join("\n", matchOnColumnList.Select(c => c + "='" +
                                matchOnQuery.Values[matchOnQuery.Attributes.IndexOf(c)] + "'" ?? "<null>").ToArray());

                                throw new Exception(string.Format("Match on values {0} resulted in more than one record to update. Use -AllowMultipleMatches to update all matching records. Match on values:\n", matchOnSummary));
                            }
                        }
                    }
                }
            }
            else
            {
                if (MatchOn != null)
                {
                    throw new ArgumentException("MatchOn not supported for intersect entities", "MatchOn");
                }

                ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = entityMetadata.ManyToManyRelationships[0];

                Guid? entity1Value =
                    target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity1IntersectAttribute);
                Guid? entity2Value =
                    target.GetAttributeValue<Guid?>(manyToManyRelationshipMetadata.Entity2IntersectAttribute);

                if (entity1Value == null || entity2Value == null)
                {
                    throw new Exception("For intersect entities (many to many relationships), The input object must contain values for both attributes involved in the relationship.");
                }

                QueryByAttribute existingRecordQuery = new QueryByAttribute(TableName);
                existingRecordQuery.AddAttributeValue(manyToManyRelationshipMetadata.Entity1IntersectAttribute, entity1Value.Value);
                existingRecordQuery.AddAttributeValue(manyToManyRelationshipMetadata.Entity2IntersectAttribute, entity2Value.Value);
                existingRecordQuery.ColumnSet = new ColumnSet(target.Attributes.Select(a => a.Key).ToArray());

                var record = Connection.RetrieveMultiple(existingRecordQuery).Entities.FirstOrDefault();
                if (record != null)
                {
                    existingRecords.Add(record);
                }
            }
            return existingRecords;
        }

        /// <summary>
        /// Completion handler for create operations.
        /// </summary>
        public void CreateCompletion(Entity target, Entity targetCreate, string columnSummary, CreateResponse response)
        {
            SetIdProperty(InputObject, response.id);
            _writeVerbose(string.Format("Created new record {0}:{1} columns:\n{2}", target.LogicalName, response.id, columnSummary));

            if (PassThru)
            {
                _writeObject(ConvertInputToPSObjectForPassThru(InputObject, response.id));
            }
        }

        /// <summary>
        /// Completion handler for update operations.
        /// </summary>
        public void UpdateCompletion(Entity target, Entity existingRecord, string updatedColumnSummary)
        {
            _writeVerbose(string.Format("Updated existing record {0}:{1} columns:\n{2}", target.LogicalName, existingRecord.Id, updatedColumnSummary));

            if (PassThru)
            {
                _writeObject(ConvertInputToPSObjectForPassThru(InputObject, existingRecord.Id));
            }
        }

        /// <summary>
        /// Completion handler for upsert operations.
        /// </summary>
        public void UpsertCompletion(Entity targetUpdate, UpsertResponse response)
        {
            targetUpdate.Id = response.Target.Id;

            SetIdProperty(InputObject, targetUpdate.Id);

            string columnSummary = GetColumnSummary(targetUpdate, EntityConverter);

            if (response.RecordCreated)
            {
                _writeVerbose(string.Format("Upsert created new record {0}:{1} columns:\n{2}", TableName, GetKeySummary(targetUpdate), columnSummary));
            }
            else
            {
                _writeVerbose(string.Format("Upsert updated existing record {0}:{1} columns:\n{2}", TableName, GetKeySummary(targetUpdate), columnSummary));
            }

            if (PassThru)
            {
                _writeObject(ConvertInputToPSObjectForPassThru(InputObject, targetUpdate.Id));
            }
        }

        /// <summary>
        /// Completion handler for M:M association operations.
        /// </summary>
        public void AssociateCompletion(Entity target, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
        {
            QueryExpression getIdQuery = new QueryExpression(TableName);
            getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity1IntersectAttribute, ConditionOperator.Equal, record1.Id);
            getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity2IntersectAttribute, ConditionOperator.Equal, record2.Id);
            Guid id = Connection.RetrieveMultiple(getIdQuery).Entities.Single().Id;

            SetIdProperty(InputObject, id);
            _writeVerbose(string.Format("Created new intersect record {0}:{1}", target.LogicalName, id));

            if (PassThru)
            {
                _writeObject(ConvertInputToPSObjectForPassThru(InputObject, id));
            }
        }

        /// <summary>
        /// Completion handler for M:M association with upsert semantics.
        /// </summary>
        public void AssociateUpsertCompletion(bool recordWasCreated, Entity target, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
        {
            if (recordWasCreated)
            {
                _writeVerbose(string.Format("Created intersect record {0}:{1}:{2}", target.LogicalName, record1.Id, record2.Id));
            }
            else
            {
                _writeVerbose(string.Format("Skipped creating (upsert) intersect record as already exists {0}:{1}:{2}", target.LogicalName, record1.Id, record2.Id));
            }
        }

        /// <summary>
        /// Error handler for M:M association with upsert semantics.
        /// Returns true if the error was handled (record already exists).
        /// </summary>
        public bool AssociateUpsertError(OrganizationServiceFault fault, Entity target, ManyToManyRelationshipMetadata manyToManyRelationshipMetadata, EntityReference record1, EntityReference record2)
        {
            if (fault.ErrorCode != -2147220937)
            {
                return false;
            }

            AssociateUpsertCompletion(false, target, manyToManyRelationshipMetadata, record1, record2);

            return true;
        }

        /// <summary>
        /// Completion handler for getting the ID of a newly associated record.
        /// </summary>
        public void AssociateUpsertGetIdCompletion(OrganizationResponse response)
        {
            SetIdProperty(InputObject, ((UpsertResponse)response).Target.Id);
        }

        /// <summary>
        /// Upserts a record (creates if doesn't exist, updates if exists).
        /// Sets up the upsert request and completion callbacks.
        /// </summary>
        public void UpsertRecord()
        {
            if (NoCreate || NoUpdate)
            {
                throw new ArgumentException("-NoCreate and -NoUpdate are not supported with -Upsert");
            }

            if (EntityMetadata.IsIntersect.GetValueOrDefault())
            {
                if (MatchOn != null)
                {
                    throw new ArgumentException("-MatchOn is not supported for -Upsert of M:M");
                }

                ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = EntityMetadata.ManyToManyRelationships[0];

                EntityReference record1 = new EntityReference(manyToManyRelationshipMetadata.Entity1LogicalName,
                         Target.GetAttributeValue<Guid>(
                             manyToManyRelationshipMetadata.Entity1IntersectAttribute));
                EntityReference record2 = new EntityReference(manyToManyRelationshipMetadata.Entity2LogicalName,
                         Target.GetAttributeValue<Guid>(
                             manyToManyRelationshipMetadata.Entity2IntersectAttribute));

                AssociateRequest request = new AssociateRequest()
                {
                    Target = record1,
                    RelatedEntities = new EntityReferenceCollection() { record2 },
                    Relationship = new Relationship(manyToManyRelationshipMetadata.SchemaName)
                    {
                        PrimaryEntityRole = EntityRole.Referencing
                    },
                };
                ApplyBypassBusinessLogicExecution(request);
                Requests.Add(request);

                _writeVerbose($"Added create of new intersect record {TableName}:{record1.Id},{record2.Id} to batch");

                // Set up completion callback and error handler
                ResponseCompletion = (response) => {
                    AssociateUpsertCompletion(true, Target, manyToManyRelationshipMetadata, record1, record2);
                };
                ResponseExceptionCompletion = (fault) => {
                    return AssociateUpsertError(fault, Target, manyToManyRelationshipMetadata, record1, record2);
                };

                if (PassThru)
                {
                    QueryExpression getIdQuery = new QueryExpression(TableName);
                    getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity1IntersectAttribute, ConditionOperator.Equal, record1.Id);
                    getIdQuery.Criteria.AddCondition(manyToManyRelationshipMetadata.Entity2IntersectAttribute, ConditionOperator.Equal, record2.Id);
                    Requests.Add(new RetrieveMultipleRequest() { Query = getIdQuery });
                    
                    // Add completion for the second request to set the Id on the InputObject
                    ResponseCompletions.Add((response) => {
                        AssociateUpsertGetIdCompletion(response);
                    });
                }
            }
            else
            {
                Entity targetUpdate = new Entity(Target.LogicalName) { Id = Target.Id };

                if (MatchOn != null)
                {
                    if (MatchOn.Length > 1)
                    {
                        throw new NotSupportedException("MatchOn must only have a single array when used with Upsert");
                    }

                    var key = MetadataFactory.GetMetadata(Target.LogicalName).Keys.FirstOrDefault(k => 
                        k.KeyAttributes.Length == MatchOn[0].Length && k.KeyAttributes.All(a => MatchOn[0].Contains(a)));
                    if (key == null)
                    {
                        throw new ArgumentException($"MatchOn must match a key that is defined on the table");
                    }

                    targetUpdate.KeyAttributes = new KeyAttributeCollection();

                    foreach (var matchOnField in MatchOn[0])
                    {
                        targetUpdate.KeyAttributes.Add(matchOnField, Target.GetAttributeValue<object>(matchOnField));
                    }
                }

                targetUpdate.Attributes.AddRange(Target.Attributes.Where(a => 
                    !DontUpdateDirectlyColumnNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)));

                string columnSummary = GetColumnSummary(targetUpdate, EntityConverter);

                UpsertRequest request = new UpsertRequest() { Target = targetUpdate };
                Requests.Add(request);

                _writeVerbose($"Added upsert of new record {TableName}:{GetKeySummary(targetUpdate)} to batch - columns:\n{columnSummary}");

                // Set up completion callback
                ResponseCompletion = (response) => {
                    UpsertCompletion(targetUpdate, (UpsertResponse)response);
                };
            }
        }

        /// <summary>
        /// Updates an existing record.
        /// Sets up the update request and completion callbacks.
        /// </summary>
        public void UpdateExistingRecord()
        {
            if (NoUpdate)
            {
                _writeVerbose($"Skipped updated existing record {TableName}:{Id} - NoUpdate enabled");
                return;
            }

            Target.Id = ExistingRecord.Id;
            Target[EntityMetadata.PrimaryIdAttribute] = ExistingRecord[EntityMetadata.PrimaryIdAttribute];

            SetIdProperty(InputObject, ExistingRecord.Id);

            RemoveUnchangedColumns(Target, ExistingRecord);

            if (NoUpdateColumns != null)
            {
                foreach (string noUpdateColumn in NoUpdateColumns)
                {
                    Target.Attributes.Remove(noUpdateColumn);
                }
            }

            Entity targetUpdate = new Entity(Target.LogicalName);
            targetUpdate.Attributes.AddRange(Target.Attributes.Where(a => 
                !DontUpdateDirectlyColumnNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)));

            if (EntityMetadata.IsIntersect.GetValueOrDefault())
            {
                // Intersect entities don't support update, just return with PassThru
                if (PassThru)
                {
                    _writeObject(ConvertInputToPSObjectForPassThru(InputObject, ExistingRecord.Id));
                }
            }
            else if (targetUpdate.Attributes.Any())
            {
                UpdateRequest request = new UpdateRequest() { Target = Target };
                ApplyBypassBusinessLogicExecution(request);
                string updatedColumnSummary = GetColumnSummary(targetUpdate, EntityConverter);

                Requests.Add(request);

                _writeVerbose($"Added updated of existing record {TableName}:{ExistingRecord.Id} to batch - columns:\n{updatedColumnSummary}");

                // Set up completion callback
                ResponseCompletion = (response) => {
                    UpdateCompletion(Target, ExistingRecord, updatedColumnSummary);
                };
            }
            else
            {
                _writeVerbose($"Skipped updated existing record {TableName}:{Id} - nothing changed");

                if (PassThru)
                {
                    _writeObject(ConvertInputToPSObjectForPassThru(InputObject, ExistingRecord.Id));
                }
            }
        }

        /// <summary>
        /// Creates a new record (regular entity or M:M association).
        /// Sets up the create request and completion callbacks.
        /// </summary>
        public void CreateNewRecord()
        {
            if (NoCreate)
            {
                _writeVerbose($"Skipped creating new record {TableName}:{Id} - NoCreate enabled");
                return;
            }

            if (EntityMetadata.IsIntersect.GetValueOrDefault())
            {
                // Handle M:M association creation
                ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = EntityMetadata.ManyToManyRelationships[0];

                EntityReference record1 = new EntityReference(manyToManyRelationshipMetadata.Entity1LogicalName,
                         Target.GetAttributeValue<Guid>(
                             manyToManyRelationshipMetadata.Entity1IntersectAttribute));
                EntityReference record2 = new EntityReference(manyToManyRelationshipMetadata.Entity2LogicalName,
                         Target.GetAttributeValue<Guid>(
                             manyToManyRelationshipMetadata.Entity2IntersectAttribute));

                AssociateRequest request = new AssociateRequest()
                {
                    Target = record1,
                    RelatedEntities = new EntityReferenceCollection() { record2 },
                    Relationship = new Relationship(manyToManyRelationshipMetadata.SchemaName)
                    {
                        PrimaryEntityRole = EntityRole.Referencing
                    },
                };
                ApplyBypassBusinessLogicExecution(request);
                Requests.Add(request);

                _writeVerbose($"Added create of new intersect record {TableName}:{record1.Id},{record2.Id} to batch");
                
                // Set up completion callback
                ResponseCompletion = (response) => {
                    AssociateCompletion(Target, manyToManyRelationshipMetadata, record1, record2);
                };
            }
            else
            {
                // Handle regular entity creation
                Entity targetCreate = new Entity(Target.LogicalName) { Id = Target.Id };
                targetCreate.Attributes.AddRange(Target.Attributes.Where(a => 
                    !DontUpdateDirectlyColumnNames.Contains(a.Key, StringComparer.OrdinalIgnoreCase)));

                string columnSummary = GetColumnSummary(targetCreate, EntityConverter);

                CreateRequest request = new CreateRequest() { Target = targetCreate };
                ApplyBypassBusinessLogicExecution(request);
                Requests.Add(request);

                _writeVerbose($"Added created of new record {TableName}:{targetCreate.Id} to batch - columns:\n{columnSummary}");
                
                // Set up completion callback
                ResponseCompletion = (response) => {
                    CreateCompletion(Target, targetCreate, columnSummary, (CreateResponse)response);
                };
            }
        }

        public override string ToString()
        {
            return $"Set {TableName}:{Id}";
        }
    }
}
