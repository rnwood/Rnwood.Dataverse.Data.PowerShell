using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fake4Dataverse.Metadata;
using Fake4Dataverse.Pipeline;
using Fake4Dataverse.Security;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse
{
    /// <summary>
    /// A lightweight per-user session connected to a shared <see cref="FakeDataverseEnvironment"/>.
    /// Implements <see cref="IOrganizationService"/> and <see cref="IOrganizationServiceAsync2"/>
    /// for unit testing Dataverse / Dynamics 365 applications without a live connection.
    /// </summary>
    public sealed class FakeOrganizationService : IOrganizationService, IOrganizationServiceAsync2
    {
        private readonly FakeDataverseEnvironment _environment;

        /// <summary>
        /// Gets the shared environment this session is connected to.
        /// </summary>
        public FakeDataverseEnvironment Environment => _environment;

        /// <summary>
        /// Gets or sets the caller identity used for auto-generated createdby/modifiedby fields.
        /// </summary>
        public Guid CallerId { get; set; } = new Guid("00000000-0000-0000-0000-000000000001");

        /// <summary>
        /// Gets or sets the initiating user ID. In impersonation scenarios this differs from <see cref="CallerId"/>.
        /// Defaults to the same value as <see cref="CallerId"/>.
        /// </summary>
        public Guid InitiatingUserId { get; set; } = new Guid("00000000-0000-0000-0000-000000000001");

        /// <summary>
        /// Gets or sets the business unit ID of the caller.
        /// </summary>
        public Guid BusinessUnitId { get; set; } = new Guid("00000000-0000-0000-0000-000000000003");

        /// <summary>
        /// When <c>true</c>, all security checks are bypassed (system context).
        /// </summary>
        public bool UseSystemContext { get; set; }

        /// <summary>
        /// Gets or sets whether the current user (<see cref="CallerId"/>) is an Application User,
        /// surfaced to plugins via <c>IPluginExecutionContext7</c>.
        /// </summary>
        public bool IsApplicationUser { get; set; }

        /// <summary>
        /// Gets or sets the HTTP user-agent string of the caller, surfaced to plugins via
        /// <c>IPluginExecutionContext5</c>. Defaults to <c>"Fake4Dataverse"</c>.
        /// </summary>
        public string InitiatingUserAgent { get; set; } = "Fake4Dataverse";

        /// <summary>
        /// Gets or sets whether the call should be treated as originating from Power Pages / portals,
        /// surfaced to plugins via <c>IPluginExecutionContext2</c>.
        /// </summary>
        public bool IsPortalsClientCall { get; set; }

        /// <summary>
        /// Gets or sets the portals contact ID, surfaced to plugins via <c>IPluginExecutionContext2</c>.
        /// Only relevant when <see cref="IsPortalsClientCall"/> is <c>true</c>.
        /// </summary>
        public Guid PortalsContactId { get; set; }

        /// <summary>
        /// Gets or sets the Azure Active Directory object ID of the calling user (<see cref="CallerId"/>),
        /// surfaced to plugins via <c>IPluginExecutionContext2</c>.
        /// </summary>
        public Guid UserAzureActiveDirectoryObjectId { get; set; }

        /// <summary>
        /// Gets or sets the Azure Active Directory object ID of the initiating user (<see cref="InitiatingUserId"/>),
        /// surfaced to plugins via <c>IPluginExecutionContext2</c>.
        /// </summary>
        public Guid InitiatingUserAzureActiveDirectoryObjectId { get; set; }

        /// <summary>
        /// Gets or sets the Application ID of the initiating user, surfaced to plugins via
        /// <c>IPluginExecutionContext2</c>. Empty when not an application user.
        /// </summary>
        public Guid InitiatingUserApplicationId { get; set; }

        /// <summary>
        /// Gets the per-session operation log that records this session's service calls for post-hoc assertions.
        /// </summary>
        public OperationLog OperationLog { get; } = new OperationLog();

        /// <summary>
        /// Creates a new <see cref="FakeOrganizationService"/> session connected to the specified environment
        /// with the default caller ID (<c>00000000-0000-0000-0000-000000000001</c>).
        /// </summary>
        /// <param name="environment">The shared environment to connect to.</param>
        public FakeOrganizationService(FakeDataverseEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        /// <summary>
        /// Creates a new <see cref="FakeOrganizationService"/> session connected to the specified environment
        /// for the specified user.
        /// </summary>
        /// <param name="environment">The shared environment to connect to.</param>
        /// <param name="callerId">The caller identity for this session.</param>
        public FakeOrganizationService(FakeDataverseEnvironment environment, Guid callerId)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            CallerId = callerId;
            InitiatingUserId = callerId;
        }

        private FakePipelineContextSettings BuildPipelineContextSettings() =>
            new FakePipelineContextSettings
            {
                EnvironmentId = _environment.EnvironmentId,
                TenantId = _environment.TenantId,
                IsApplicationUser = IsApplicationUser,
                InitiatingUserAgent = InitiatingUserAgent,
                IsPortalsClientCall = IsPortalsClientCall,
                PortalsContactId = PortalsContactId,
                UserAzureActiveDirectoryObjectId = UserAzureActiveDirectoryObjectId,
                InitiatingUserAzureActiveDirectoryObjectId = InitiatingUserAzureActiveDirectoryObjectId,
                InitiatingUserApplicationId = InitiatingUserApplicationId,
                AuthenticatedUserId = CallerId,
            };

        /// <summary>
        /// Starts an implicit transaction scope for a top-level operation.
        /// If an outer transaction (e.g. ExecuteTransactionRequest) is already active,
        /// the existing transaction state is reused and <c>ownsTransaction</c> is <c>false</c>.
        /// </summary>
        private (TransactionCopyOnWriteState transaction, bool ownsTransaction) BeginImplicitTransaction()
        {
            var existing = _environment.Store.ActiveTransaction;
            if (existing != null)
                return (existing, false);

            var transaction = new TransactionCopyOnWriteState();
            _environment.Store.ActiveTransaction = transaction;
            return (transaction, true);
        }

        /// <summary>
        /// Ends an implicit transaction scope.
        /// On success, staged copy-on-write mutations are atomically committed to the shared store.
        /// On rollback, staged mutations are discarded.
        /// </summary>
        private void EndImplicitTransaction(TransactionCopyOnWriteState transaction, bool rollback)
        {
            var ownsActiveTransaction = ReferenceEquals(_environment.Store.ActiveTransaction, transaction);
            _environment.Store.ActiveTransaction = null;

            if (!ownsActiveTransaction)
                return;

            if (!rollback)
                _environment.Store.CommitTransaction(transaction);
        }

        private T RunInTransaction<T>(Func<T> action)
        {
            var (transaction, ownsTransaction) = BeginImplicitTransaction();
            try
            {
                return action();
            }
            catch when (ownsTransaction)
            {
                EndImplicitTransaction(transaction, rollback: true);
                throw;
            }
            finally
            {
                if (ownsTransaction)
                    EndImplicitTransaction(transaction, rollback: false);
            }
        }

        private void RunInTransaction(Action action) => RunInTransaction<object?>(() => { action(); return null!; });

        private void LogOperation(OperationRecord record)
        {
            if (_environment.Options.EnableOperationLog)
            {
                OperationLog.Add(record);
                _environment.OperationLog.Add(record);
            }
        }

        // ── IOrganizationService ─────────────────────────────────────────────

        /// <inheritdoc />
        public Guid Create(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.LogicalName)) throw new ArgumentException("Entity logical name must be specified.", nameof(entity));

            var id = RunInTransaction(() =>
            {
                if (!_environment.Options.EnablePipeline || !_environment.Pipeline.HasSteps)
                    return CreateCore(entity);

                var inputParams = new ParameterCollection { { "Target", entity } };
                var context = _environment.Pipeline.Execute("Create", entity.LogicalName, inputParams, ctx =>
                {
                    var target = (Entity)ctx.InputParameters["Target"];
                    var resultId = CreateCore(target);
                    return new ParameterCollection { { "id", resultId } };
                }, CallerId, InitiatingUserId, BusinessUnitId, _environment.OrganizationId, _environment.OrganizationName, _environment.Clock.UtcNow,
                    BuildPipelineContextSettings());
                return (Guid)context.OutputParameters["id"];
            });
            LogOperation(new OperationRecord("Create", entity.LogicalName, id, _environment.Clock.UtcNow, InMemoryEntityStore.CloneEntity(entity), null));
            return id;
        }

        private Guid CreateCore(Entity entity)
        {
            entity = InMemoryEntityStore.CloneEntity(entity);
            StripEmptyStrings(entity);

            if (_environment.Options.ValidateWithMetadata)
                _environment.MetadataStore.ValidateOnCreate(entity);

            if (!UseSystemContext)
                _environment.Security.CheckPrivilege(CallerId, entity.LogicalName, PrivilegeType.Create);

            _environment.MetadataStore.AutoDiscover(entity);

            var now = _environment.Clock.UtcNow;
            var callerRef = new EntityReference("systemuser", CallerId);

            if (_environment.Options.AutoSetOwner)
            {
                if (!entity.Contains("ownerid")) entity["ownerid"] = callerRef;
                if (!entity.Contains("createdby")) entity["createdby"] = callerRef;
                if (!entity.Contains("modifiedby")) entity["modifiedby"] = callerRef;
            }

            if (_environment.Options.AutoSetTimestamps)
            {
                if (entity.Contains("overriddencreatedon"))
                {
                    entity["createdon"] = entity["overriddencreatedon"];
                }
                else if (!entity.Contains("createdon"))
                {
                    entity["createdon"] = now;
                }
                if (!entity.Contains("modifiedon")) entity["modifiedon"] = now;
            }

            if (_environment.Options.AutoSetStateCode)
            {
                if (!entity.Contains("statecode"))
                {
                    var defaultState = 0;
                    var defaultStatus = 1;
                    var defaults = _environment.GetDefaultStatusCodes(entity.LogicalName);
                    if (defaults.HasValue)
                    {
                        defaultState = defaults.Value.StateCode;
                        defaultStatus = defaults.Value.StatusCode;
                    }
                    entity["statecode"] = new OptionSetValue(defaultState);
                    if (!entity.Contains("statuscode"))
                        entity["statuscode"] = new OptionSetValue(defaultStatus);
                }
            }

            if (_environment.Options.AutoSetVersionNumber)
                entity["versionnumber"] = _environment.IncrementVersion();

            if (_environment.Currency.IsConfigured)
                _environment.Currency.ComputeBaseCurrencyFields(entity);

            if (_environment.IsSolutionAwareEntity(entity.LogicalName))
                return _environment.UnpublishedRecords.CreateUnpublished(entity);

            return _environment.Store.Create(entity);
        }

        /// <inheritdoc />
        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            if (!UseSystemContext)
                _environment.Security.CheckPrivilege(CallerId, entityName, PrivilegeType.Read);
            Entity entity;
            try
            {
                entity = _environment.Store.Retrieve(entityName, id, columnSet);
            }
            catch (System.ServiceModel.FaultException<OrganizationServiceFault> ex)
                when (ex.Detail?.ErrorCode == DataverseFault.ObjectDoesNotExist
                      && _environment.IsSolutionAwareEntity(entityName)
                      && _environment.UnpublishedRecords.Store.Exists(entityName, id))
            {
                // In real Dataverse, a newly created solution-aware record is always retrievable.
                // Fall back to the unpublished store when the published store has no record.
                entity = _environment.UnpublishedRecords.Store.Retrieve(entityName, id, columnSet);
            }
            if (!UseSystemContext)
            {
                var ownerId = entity.GetAttributeValue<EntityReference>("ownerid")?.Id;
                _environment.Security.CheckRecordPrivilege(CallerId, entityName, id, PrivilegeType.Read, ownerId);
            }
            if (_environment.CalculatedFields.HasFields)
                _environment.CalculatedFields.ApplyCalculatedFields(entity, _environment.Store);
            PopulateEntityReferenceNames(entity);
            PopulateFormattedValues(entity);
            if (_environment.Options.EnableOperationLog)
            {
                var record = new OperationRecord("Retrieve", entityName, id, _environment.Clock.UtcNow, null, null);
                OperationLog.Add(record);
                _environment.OperationLog.Add(record);
            }
            return entity;
        }

        /// <summary>
        /// Retrieves an entity by alternate key.
        /// </summary>
        internal Entity RetrieveByAlternateKey(string entityName, KeyAttributeCollection keyAttributes, ColumnSet columnSet)
        {
            var entity = _environment.Store.RetrieveByAlternateKey(entityName, keyAttributes, columnSet, _environment.MetadataStore);
            if (_environment.CalculatedFields.HasFields)
                _environment.CalculatedFields.ApplyCalculatedFields(entity, _environment.Store);
            PopulateEntityReferenceNames(entity);
            PopulateFormattedValues(entity);
            return entity;
        }

        /// <inheritdoc />
        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            EntityCollection result;

            if (query is QueryExpression qe)
            {
                _environment.QueryEvaluator.Clock = _environment.Clock;
                _environment.QueryEvaluator.CallerId = CallerId;
                result = _environment.QueryEvaluator.Evaluate(qe, _environment.Store);
            }
            else if (query is FetchExpression fe)
            {
                _environment.QueryEvaluator.Clock = _environment.Clock;
                _environment.QueryEvaluator.CallerId = CallerId;
                result = _environment.FetchXmlEvaluator.Evaluate(fe.Query, _environment.Store);
            }
            else if (query is QueryByAttribute qba)
            {
                result = EvaluateQueryByAttribute(qba);
            }
            else
            {
                throw new NotSupportedException($"Query type '{query.GetType().Name}' is not supported.");
            }

            // Filter results by row-level security when enforcement is enabled
            if (!UseSystemContext && _environment.Security.EnforceSecurityRoles)
            {
                var queryEntityName2 = (query as QueryExpression)?.EntityName
                    ?? (query as QueryByAttribute)?.EntityName;
                if (queryEntityName2 != null)
                {
                    for (int i = result.Entities.Count - 1; i >= 0; i--)
                    {
                        var e = result.Entities[i];
                        var ownerId = e.GetAttributeValue<EntityReference>("ownerid")?.Id;
                        if (!ownerId.HasValue)
                        {
                            try
                            {
                                var full = _environment.Store.Retrieve(queryEntityName2, e.Id, new ColumnSet("ownerid"));
                                ownerId = full.GetAttributeValue<EntityReference>("ownerid")?.Id;
                            }
                            catch { /* entity may not have ownerid */ }
                        }
                        if (!_environment.Security.CanAccessRecord(CallerId, queryEntityName2, e.Id, PrivilegeType.Read, ownerId))
                            result.Entities.RemoveAt(i);
                    }
                }
            }

            if (_environment.CalculatedFields.HasFields)
            {
                foreach (var entity in result.Entities)
                    _environment.CalculatedFields.ApplyCalculatedFields(entity, _environment.Store);
            }

            foreach (var entity in result.Entities)
            {
                PopulateEntityReferenceNames(entity);
                PopulateFormattedValues(entity);
            }

            var queryEntityName = (query as QueryExpression)?.EntityName ?? (query as QueryByAttribute)?.EntityName;
            if (_environment.Options.EnableOperationLog)
            {
                var record = new OperationRecord("RetrieveMultiple", queryEntityName, null, _environment.Clock.UtcNow, null, null);
                OperationLog.Add(record);
                _environment.OperationLog.Add(record);
            }

            return result;
        }

        /// <inheritdoc />
        public void Update(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.LogicalName)) throw new ArgumentException("Entity logical name must be specified.", nameof(entity));

            RunInTransaction(() =>
            {
                if (!_environment.Options.EnablePipeline || !_environment.Pipeline.HasSteps)
                {
                    UpdateCore(entity);
                    return;
                }
                var inputParams = new ParameterCollection { { "Target", entity } };
                _environment.Pipeline.Execute("Update", entity.LogicalName, inputParams, ctx =>
                {
                    var target = (Entity)ctx.InputParameters["Target"];
                    UpdateCore(target);
                    return new ParameterCollection();
                }, CallerId, InitiatingUserId, BusinessUnitId, _environment.OrganizationId, _environment.OrganizationName, _environment.Clock.UtcNow,
                    BuildPipelineContextSettings());
            });
            LogOperation(new OperationRecord("Update", entity.LogicalName, entity.Id, _environment.Clock.UtcNow, InMemoryEntityStore.CloneEntity(entity), null));
        }

        private void UpdateCore(Entity entity)
        {
            entity = InMemoryEntityStore.CloneEntity(entity);
            StripEmptyStrings(entity);

            if (_environment.Options.ValidateWithMetadata)
                _environment.MetadataStore.ValidateOnUpdate(entity);

            ResolveAlternateKey(entity);
            if (!UseSystemContext)
                _environment.Security.CheckPrivilege(CallerId, entity.LogicalName, PrivilegeType.Write);

            var now = _environment.Clock.UtcNow;
            var callerRef = new EntityReference("systemuser", CallerId);

            if (_environment.Options.AutoSetTimestamps)
                entity["modifiedon"] = now;

            if (_environment.Options.AutoSetOwner)
                entity["modifiedby"] = callerRef;

            if (_environment.Options.AutoSetVersionNumber)
                entity["versionnumber"] = _environment.IncrementVersion();

            if (_environment.Currency.IsConfigured)
                _environment.Currency.ComputeBaseCurrencyFields(entity);

            if (_environment.IsSolutionAwareEntity(entity.LogicalName))
            {
                _environment.UnpublishedRecords.UpdateUnpublished(entity, _environment.Store);
                return;
            }

            _environment.Store.Update(entity);

            if (entity.Contains("ownerid") && entity["ownerid"] is EntityReference newOwner)
            {
                ApplyCascadeAssign(entity.LogicalName, entity.Id, newOwner);
            }
        }

        /// <summary>
        /// Performs an update with optimistic concurrency checking. The update only succeeds
        /// if the stored <c>versionnumber</c> matches <paramref name="expectedVersion"/>.
        /// </summary>
        internal void UpdateWithConcurrencyCheck(Entity entity, long expectedVersion)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.LogicalName)) throw new ArgumentException("Entity logical name must be specified.", nameof(entity));

            RunInTransaction(() =>
            {
                if (!_environment.Options.EnablePipeline || !_environment.Pipeline.HasSteps)
                {
                    UpdateCoreWithConcurrencyCheck(entity, expectedVersion);
                    return;
                }
                var inputParams = new ParameterCollection { { "Target", entity } };
                _environment.Pipeline.Execute("Update", entity.LogicalName, inputParams, ctx =>
                {
                    var target = (Entity)ctx.InputParameters["Target"];
                    UpdateCoreWithConcurrencyCheck(target, expectedVersion);
                    return new ParameterCollection();
                }, CallerId, InitiatingUserId, BusinessUnitId, _environment.OrganizationId, _environment.OrganizationName, _environment.Clock.UtcNow,
                    BuildPipelineContextSettings());
            });
            LogOperation(new OperationRecord("Update", entity.LogicalName, entity.Id, _environment.Clock.UtcNow, InMemoryEntityStore.CloneEntity(entity), null));
        }

        private void UpdateCoreWithConcurrencyCheck(Entity entity, long expectedVersion)
        {
            entity = InMemoryEntityStore.CloneEntity(entity);
            StripEmptyStrings(entity);

            if (_environment.Options.ValidateWithMetadata)
                _environment.MetadataStore.ValidateOnUpdate(entity);

            ResolveAlternateKey(entity);
            if (!UseSystemContext)
                _environment.Security.CheckPrivilege(CallerId, entity.LogicalName, PrivilegeType.Write);

            var now = _environment.Clock.UtcNow;
            var callerRef = new EntityReference("systemuser", CallerId);

            if (_environment.Options.AutoSetTimestamps)
                entity["modifiedon"] = now;

            if (_environment.Options.AutoSetOwner)
                entity["modifiedby"] = callerRef;

            if (_environment.Options.AutoSetVersionNumber)
                entity["versionnumber"] = _environment.IncrementVersion();

            if (_environment.Currency.IsConfigured)
                _environment.Currency.ComputeBaseCurrencyFields(entity);

            _environment.Store.Update(entity, expectedVersion);

            if (entity.Contains("ownerid") && entity["ownerid"] is EntityReference newOwner)
            {
                ApplyCascadeAssign(entity.LogicalName, entity.Id, newOwner);
            }
        }

        /// <summary>
        /// Performs a delete with optimistic concurrency checking. The delete only succeeds
        /// if the stored <c>versionnumber</c> matches <paramref name="expectedVersion"/>.
        /// </summary>
        internal void DeleteWithConcurrencyCheck(string entityName, Guid id, long expectedVersion)
        {
            if (!UseSystemContext)
                _environment.Security.CheckPrivilege(CallerId, entityName, PrivilegeType.Delete);

            RunInTransaction(() =>
            {
                if (!_environment.Options.EnablePipeline || !_environment.Pipeline.HasSteps)
                {
                    ApplyCascadeDelete(entityName, id);
                    _environment.Store.Delete(entityName, id, expectedVersion);
                    return;
                }
                var inputParams = new ParameterCollection
                {
                    { "Target", new EntityReference(entityName, id) }
                };
                _environment.Pipeline.Execute("Delete", entityName, inputParams, ctx =>
                {
                    var target = (EntityReference)ctx.InputParameters["Target"];
                    ApplyCascadeDelete(target.LogicalName, target.Id);
                    _environment.Store.Delete(target.LogicalName, target.Id, expectedVersion);
                    return new ParameterCollection();
                }, CallerId, InitiatingUserId, BusinessUnitId, _environment.OrganizationId, _environment.OrganizationName, _environment.Clock.UtcNow,
                    BuildPipelineContextSettings());
            });
            LogOperation(new OperationRecord("Delete", entityName, id, _environment.Clock.UtcNow, null, null));
        }

        /// <inheritdoc />
        public void Delete(string entityName, Guid id)
        {
            if (!UseSystemContext)
                _environment.Security.CheckPrivilege(CallerId, entityName, PrivilegeType.Delete);

            RunInTransaction(() =>
            {
                if (_environment.IsSolutionAwareEntity(entityName))
                {
                    bool existsPublished = _environment.Store.Exists(entityName, id);
                    bool existsUnpublished = _environment.UnpublishedRecords.Store.Exists(entityName, id);

                    if (!existsPublished && !existsUnpublished)
                        throw DataverseFault.EntityNotFound(entityName, id);

                    if (existsPublished)
                    {
                        ApplyCascadeDelete(entityName, id);
                        _environment.Store.Delete(entityName, id);
                    }
                    _environment.UnpublishedRecords.DeleteUnpublished(entityName, id);
                    return;
                }

                if (!_environment.Options.EnablePipeline || !_environment.Pipeline.HasSteps)
                {
                    ApplyCascadeDelete(entityName, id);
                    _environment.Store.Delete(entityName, id);
                    return;
                }
                var inputParams = new ParameterCollection
                {
                    { "Target", new EntityReference(entityName, id) }
                };
                _environment.Pipeline.Execute("Delete", entityName, inputParams, ctx =>
                {
                    var target = (EntityReference)ctx.InputParameters["Target"];
                    ApplyCascadeDelete(target.LogicalName, target.Id);
                    _environment.Store.Delete(target.LogicalName, target.Id);
                    return new ParameterCollection();
                }, CallerId, InitiatingUserId, BusinessUnitId, _environment.OrganizationId, _environment.OrganizationName, _environment.Clock.UtcNow,
                    BuildPipelineContextSettings());
            });
            LogOperation(new OperationRecord("Delete", entityName, id, _environment.Clock.UtcNow, null, null));
        }

        /// <inheritdoc />
        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));
            if (relatedEntities == null) throw new ArgumentNullException(nameof(relatedEntities));

            if (!_environment.Store.Exists(entityName, entityId))
                throw DataverseFault.EntityNotFound(entityName, entityId);

            foreach (var related in relatedEntities)
            {
                if (!_environment.Store.Exists(related.LogicalName, related.Id))
                    throw DataverseFault.EntityNotFound(related.LogicalName, related.Id);
            }

            if (_environment.Options.ValidateWithMetadata)
                _environment.MetadataStore.ValidateRelationship(entityName, relationship, relatedEntities);

            var associationName = $"association_{relationship.SchemaName}";
            var existingAssociations = _environment.Store.GetAll(associationName);
            foreach (var related in relatedEntities)
            {
                var relatedId = related.Id;
                var isDuplicate = existingAssociations.Any(a =>
                {
                    var src = a.GetAttributeValue<EntityReference>("sourceid");
                    var tgt = a.GetAttributeValue<EntityReference>("targetid");
                    return (src?.Id == entityId && tgt?.Id == relatedId) ||
                           (src?.Id == relatedId && tgt?.Id == entityId);
                });
                if (isDuplicate)
                    throw DataverseFault.Create(DataverseFault.DuplicateRecord, "A record with the specified key values already exists.");
            }

            RunInTransaction(() =>
            {
                foreach (var related in relatedEntities)
                {
                    var association = new Entity(associationName);
                    association["sourceid"] = new EntityReference(entityName, entityId);
                    association["targetid"] = related;
                    _environment.Store.Create(association);
                }
            });
            LogOperation(new OperationRecord("Associate", entityName, entityId, _environment.Clock.UtcNow, null, null));
        }

        /// <inheritdoc />
        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));
            if (relatedEntities == null) throw new ArgumentNullException(nameof(relatedEntities));

            if (_environment.Options.ValidateWithMetadata)
                _environment.MetadataStore.ValidateRelationship(entityName, relationship, relatedEntities);

            var associationEntity = $"association_{relationship.SchemaName}";
            RunInTransaction(() => _environment.Store.RemoveAssociations(associationEntity, entityName, entityId, relatedEntities));
            LogOperation(new OperationRecord("Disassociate", entityName, entityId, _environment.Clock.UtcNow, null, null));
        }

        /// <inheritdoc />
        public OrganizationResponse Execute(OrganizationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var response = RunInTransaction(() => _environment.HandlerRegistry.Execute(request, this));
            LogOperation(new OperationRecord("Execute", null, null, _environment.Clock.UtcNow, null, request));
            return response;
        }

        // ── IOrganizationServiceAsync2 ───────────────────────────────────────

        /// <inheritdoc />
        public Task<Guid> CreateAsync(Entity entity)
        {
            return CreateAsync(entity, CancellationToken.None);
        }

        /// <inheritdoc />
        public Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet)
        {
            return RetrieveAsync(entityName, id, columnSet, CancellationToken.None);
        }

        /// <inheritdoc />
        public Task<EntityCollection> RetrieveMultipleAsync(QueryBase query)
        {
            return RetrieveMultipleAsync(query, CancellationToken.None);
        }

        /// <inheritdoc />
        public Task UpdateAsync(Entity entity)
        {
            return UpdateAsync(entity, CancellationToken.None);
        }

        /// <inheritdoc />
        public Task DeleteAsync(string entityName, Guid id)
        {
            return DeleteAsync(entityName, id, CancellationToken.None);
        }

        /// <inheritdoc />
        public Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            return AssociateAsync(entityName, entityId, relationship, relatedEntities, CancellationToken.None);
        }

        /// <inheritdoc />
        public Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            return DisassociateAsync(entityName, entityId, relationship, relatedEntities, CancellationToken.None);
        }

        /// <inheritdoc />
        public Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request)
        {
            return ExecuteAsync(request, CancellationToken.None);
        }

        /// <inheritdoc />
        public Task<Guid> CreateAsync(Entity entity, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Create(entity));
        }

        /// <inheritdoc />
        public Task<Entity> CreateAndReturnAsync(Entity entity, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = Create(entity);
            return Task.FromResult(Retrieve(entity.LogicalName, id, new ColumnSet(true)));
        }

        /// <inheritdoc />
        public Task<Entity> RetrieveAsync(string entityName, Guid id, ColumnSet columnSet, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Retrieve(entityName, id, columnSet));
        }

        /// <inheritdoc />
        public Task<EntityCollection> RetrieveMultipleAsync(QueryBase query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(RetrieveMultiple(query));
        }

        /// <inheritdoc />
        public Task UpdateAsync(Entity entity, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Update(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteAsync(string entityName, Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Delete(entityName, id);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task AssociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Associate(entityName, entityId, relationship, relatedEntities);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DisassociateAsync(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Disassociate(entityName, entityId, relationship, relatedEntities);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<OrganizationResponse> ExecuteAsync(OrganizationRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Execute(request));
        }

        // ── Saved Query ──────────────────────────────────────────────────────

        /// <summary>
        /// Executes a saved query (userquery or savedquery) by its entity ID.
        /// Retrieves the query entity from the store, extracts its <c>fetchxml</c> attribute,
        /// and evaluates it via <see cref="FetchExpression"/>.
        /// </summary>
        /// <param name="queryId">The unique identifier of the <c>userquery</c> or <c>savedquery</c> record.</param>
        /// <returns>The <see cref="EntityCollection"/> result of executing the saved query's FetchXml.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the saved query has no <c>fetchxml</c> attribute.</exception>
        public EntityCollection ExecuteSavedQuery(Guid queryId)
        {
            Entity? queryEntity = null;
            foreach (var entityName in new[] { "userquery", "savedquery" })
            {
                try
                {
                    queryEntity = _environment.Store.Retrieve(entityName, queryId, new ColumnSet(true));
                    break;
                }
                catch (System.ServiceModel.FaultException<OrganizationServiceFault>)
                {
                    // Not found in this entity type, try the next
                }
            }

            if (queryEntity == null)
                throw DataverseFault.EntityNotFound("userquery", queryId);

            var fetchXml = queryEntity.GetAttributeValue<string>("fetchxml");
            if (string.IsNullOrEmpty(fetchXml))
                throw new InvalidOperationException($"Saved query '{queryId}' does not contain a fetchxml attribute.");

            return RetrieveMultiple(new FetchExpression(fetchXml));
        }

        // ── Private Helpers ──────────────────────────────────────────────────

        private EntityCollection EvaluateQueryByAttribute(QueryByAttribute qba)
        {
            var query = new QueryExpression(qba.EntityName)
            {
                ColumnSet = qba.ColumnSet ?? new ColumnSet(true)
            };

            for (int i = 0; i < qba.Attributes.Count; i++)
            {
                if (i < qba.Values.Count)
                    query.Criteria.AddCondition(qba.Attributes[i], ConditionOperator.Equal, qba.Values[i]);
                else
                    query.Criteria.AddCondition(qba.Attributes[i], ConditionOperator.Null);
            }

            foreach (var order in qba.Orders)
                query.Orders.Add(order);

            if (qba.TopCount.HasValue)
                query.TopCount = qba.TopCount;

            if (qba.PageInfo != null)
                query.PageInfo = qba.PageInfo;

            _environment.QueryEvaluator.Clock = _environment.Clock;
            _environment.QueryEvaluator.CallerId = CallerId;
            return _environment.QueryEvaluator.Evaluate(query, _environment.Store);
        }

        private void ResolveAlternateKey(Entity entity)
        {
            if (entity.Id != Guid.Empty) return;
            if (entity.KeyAttributes == null || entity.KeyAttributes.Count == 0) return;

            var found = _environment.Store.FindByAlternateKey(entity.LogicalName, entity.KeyAttributes, _environment.MetadataStore);
            entity.Id = found;
        }

        private static void StripEmptyStrings(Entity entity)
        {
            var keysToNull = new List<string>();
            foreach (var attr in entity.Attributes)
            {
                if (attr.Value is string s && s.Length == 0)
                    keysToNull.Add(attr.Key);
            }
            foreach (var key in keysToNull)
            {
                entity[key] = null;
            }
        }

        private void PopulateEntityReferenceNames(Entity entity)
        {
            foreach (var attr in entity.Attributes.ToArray())
            {
                if (attr.Value is EntityReference er && er.Id != Guid.Empty && string.IsNullOrEmpty(er.Name))
                {
                    var primaryNameAttr = GetPrimaryNameAttribute(er.LogicalName);
                    if (primaryNameAttr != null && _environment.Store.Exists(er.LogicalName, er.Id))
                    {
                        var related = _environment.Store.Retrieve(er.LogicalName, er.Id, new ColumnSet(primaryNameAttr));
                        if (related.Contains(primaryNameAttr))
                            er.Name = related.GetAttributeValue<string>(primaryNameAttr);
                    }
                }
            }
        }

        private static void PopulateFormattedValues(Entity entity)
        {
            foreach (var attr in entity.Attributes)
            {
                if (entity.FormattedValues.ContainsKey(attr.Key))
                    continue;

                string? formattedValue;
                if (TryFormatValue(attr.Value, out formattedValue))
                    entity.FormattedValues[attr.Key] = formattedValue!;
            }
        }

        private static bool TryFormatValue(object? value, out string? formattedValue)
        {
            switch (value)
            {
                case AliasedValue aliasedValue:
                    return TryFormatValue(aliasedValue.Value, out formattedValue);
                case OptionSetValue optionSetValue:
                    formattedValue = optionSetValue.Value.ToString(CultureInfo.InvariantCulture);
                    return true;
                case Money money:
                    formattedValue = money.Value.ToString("N2", CultureInfo.InvariantCulture);
                    return true;
                case bool booleanValue:
                    formattedValue = booleanValue ? "Yes" : "No";
                    return true;
                case DateTime dateTimeValue:
                    formattedValue = dateTimeValue.ToString("M/d/yyyy h:mm tt", CultureInfo.InvariantCulture);
                    return true;
                default:
                    formattedValue = null;
                    return false;
            }
        }

        private static string? GetPrimaryNameAttribute(string entityName)
        {
            return entityName switch
            {
                "account" => "name",
                "contact" => "fullname",
                "lead" => "fullname",
                "opportunity" => "name",
                "incident" => "title",
                "systemuser" => "fullname",
                "team" => "name",
                "businessunit" => "name",
                _ => "name"
            };
        }

        private void ApplyCascadeDelete(string entityName, Guid id)
        {
            var childRelationships = _environment.MetadataStore.GetChildRelationships(entityName);
            foreach (var rel in childRelationships)
            {
                var childEntities = _environment.Store.GetAll(rel.ReferencingEntity);
                foreach (var child in childEntities)
                {
                    var fk = child.GetAttributeValue<EntityReference>(rel.ReferencingAttribute);
                    if (fk == null || fk.Id != id) continue;

                    switch (rel.Cascade.Delete)
                    {
                        case CascadeType.Cascade:
                            Delete(child.LogicalName, child.Id);
                            break;
                        case CascadeType.RemoveLink:
                            var update = new Entity(child.LogicalName, child.Id);
                            update[rel.ReferencingAttribute] = null;
                            _environment.Store.Update(update);
                            break;
                        case CascadeType.Restrict:
                            throw DataverseFault.Create(DataverseFault.InvalidArgument,
                                $"Cannot delete '{entityName}' record '{id}' because related '{rel.ReferencingEntity}' records exist (relationship '{rel.SchemaName}' has Restrict delete).");
                    }
                }
            }
        }

        private void ApplyCascadeAssign(string entityName, Guid id, EntityReference newOwner)
        {
            var childRelationships = _environment.MetadataStore.GetChildRelationships(entityName);
            foreach (var rel in childRelationships)
            {
                if (rel.Cascade.Assign != CascadeType.Cascade) continue;

                var childEntities = _environment.Store.GetAll(rel.ReferencingEntity);
                foreach (var child in childEntities)
                {
                    var fk = child.GetAttributeValue<EntityReference>(rel.ReferencingAttribute);
                    if (fk == null || fk.Id != id) continue;

                    var update = new Entity(child.LogicalName, child.Id);
                    update["ownerid"] = newOwner;
                    _environment.Store.Update(update);
                }
            }
        }
    }
}
