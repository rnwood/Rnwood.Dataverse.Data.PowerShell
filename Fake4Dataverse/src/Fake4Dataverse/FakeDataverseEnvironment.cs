using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Fake4Dataverse.Pipeline;
using Fake4Dataverse.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Fake4Dataverse
{
    /// <summary>
    /// Represents a shared in-memory Dataverse environment that holds all data, metadata,
    /// security configuration, pipeline, and other shared state. Multiple
    /// <see cref="FakeOrganizationService"/> sessions can connect to the same environment,
    /// each with their own caller identity.
    /// </summary>
    public sealed class FakeDataverseEnvironment
    {
        private readonly InMemoryEntityStore _store = new InMemoryEntityStore();
        private readonly AttributeIndex _attributeIndex = new AttributeIndex();
        private readonly Dictionary<(string EntityName, Guid EntityId, string AttributeName), byte[]> _binaryStore =
            new Dictionary<(string, Guid, string), byte[]>();
        private readonly Dictionary<string, FileUploadSession> _uploadSessions =
            new Dictionary<string, FileUploadSession>();
        private readonly Dictionary<string, List<StatusTransition>> _statusTransitions =
            new Dictionary<string, List<StatusTransition>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, (int StateCode, int StatusCode)> _defaultStatusCodes =
            new Dictionary<string, (int, int)>(StringComparer.OrdinalIgnoreCase);
        private readonly UnpublishedRecordStore _unpublishedRecordStore = new UnpublishedRecordStore();
        private long _versionCounter;

        /// <summary>
        /// Gets the in-memory entity store used by this environment.
        /// </summary>
        internal InMemoryEntityStore Store => _store;

        /// <summary>
        /// Gets the query expression evaluator for this environment.
        /// </summary>
        internal QueryExpressionEvaluator QueryEvaluator { get; }

        /// <summary>
        /// Gets the FetchXml evaluator for this environment.
        /// </summary>
        internal FetchXmlEvaluator FetchXmlEvaluator { get; }

        /// <summary>
        /// Gets the metadata store for defining entity/attribute metadata and validation rules.
        /// </summary>
        public InMemoryMetadataStore MetadataStore { get; } = new InMemoryMetadataStore();

        /// <summary>
        /// Gets the security manager for configuring roles, privileges, and record sharing.
        /// </summary>
        public SecurityManager Security { get; } = new SecurityManager();

        /// <summary>
        /// Gets the pipeline manager for registering pre/post-operation steps and
        /// <see cref="IPlugin"/> instances.
        /// </summary>
        public PipelineManager Pipeline { get; }

        /// <summary>
        /// Gets the calculated field manager for registering calculated and rollup fields.
        /// </summary>
        public CalculatedFieldManager CalculatedFields { get; } = new CalculatedFieldManager();

        /// <summary>
        /// Gets the currency manager for configuring exchange rates and base currency computation.
        /// </summary>
        public CurrencyManager Currency { get; } = new CurrencyManager();

        /// <summary>
        /// Gets the handler registry for registering custom <see cref="OrganizationRequest"/> handlers.
        /// </summary>
        public OrganizationRequestHandlerRegistry HandlerRegistry { get; }

        /// <summary>
        /// Gets the operation log that records all service calls for post-hoc assertions.
        /// </summary>
        public OperationLog OperationLog { get; } = new OperationLog();

        /// <summary>
        /// Gets the configuration options controlling automatic behaviors.
        /// </summary>
        public FakeOrganizationServiceOptions Options { get; }

        /// <summary>
        /// Gets or sets the clock used for auto-generated timestamps.
        /// </summary>
        public IClock Clock { get; set; } = SystemClock.Instance;

        /// <summary>
        /// Gets the unpublished record store that manages draft copies of solution-aware entity records.
        /// </summary>
        internal UnpublishedRecordStore UnpublishedRecords => _unpublishedRecordStore;

        /// <summary>
        /// Gets or sets the organization ID.
        /// </summary>
        public Guid OrganizationId { get; set; } = new Guid("00000000-0000-0000-0000-000000000002");

        /// <summary>
        /// Gets or sets the organization name surfaced to plugins via <c>IPluginExecutionContext</c>.
        /// </summary>
        public string OrganizationName { get; set; } = "FakeOrganization";

        /// <summary>
        /// Gets or sets the Power Platform environment ID surfaced to plugins.
        /// </summary>
        public string EnvironmentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Azure AD / Entra tenant ID surfaced to plugins.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Creates a new <see cref="FakeDataverseEnvironment"/> with default options.
        /// </summary>
        public FakeDataverseEnvironment() : this(new FakeOrganizationServiceOptions())
        {
        }

        /// <summary>
        /// Creates a new <see cref="FakeDataverseEnvironment"/> configured with the specified options.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        public FakeDataverseEnvironment(FakeOrganizationServiceOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Security.EnforceSecurityRoles = options.EnforceSecurityRoles;
            _store.Index = _attributeIndex;
            QueryEvaluator = new QueryExpressionEvaluator();
            FetchXmlEvaluator = new FetchXmlEvaluator(QueryEvaluator);
            HandlerRegistry = new OrganizationRequestHandlerRegistry();
            Pipeline = new PipelineManager(
                userId => new FakeOrganizationService(this, userId ?? new Guid("00000000-0000-0000-0000-000000000001")),
                (entityName, id) =>
                    _store.Exists(entityName, id) ? _store.Retrieve(entityName, id, new ColumnSet(true)) : null);
            RegisterBuiltInHandlers();
        }

        /// <summary>
        /// Creates a new <see cref="FakeOrganizationService"/> session connected to this environment
        /// with the default caller ID (<c>00000000-0000-0000-0000-000000000001</c>).
        /// </summary>
        /// <returns>A new <see cref="FakeOrganizationService"/> session.</returns>
        public FakeOrganizationService CreateOrganizationService()
        {
            return new FakeOrganizationService(this);
        }

        /// <summary>
        /// Creates a new <see cref="FakeOrganizationService"/> session connected to this environment
        /// for the specified user.
        /// </summary>
        /// <param name="callerId">The caller identity for the new session.</param>
        /// <returns>A new <see cref="FakeOrganizationService"/> session.</returns>
        public FakeOrganizationService CreateOrganizationService(Guid callerId)
        {
            return new FakeOrganizationService(this, callerId);
        }

        // ── Seed Methods ─────────────────────────────────────────────────────

        /// <summary>
        /// Bulk-inserts entities directly into the store without triggering pipeline,
        /// security checks, auto-fields, or operation logging.
        /// </summary>
        /// <param name="entities">The entities to seed.</param>
        public void Seed(params Entity[] entities)
        {
            Seed((IEnumerable<Entity>)entities);
        }

        /// <summary>
        /// Bulk-inserts entities directly into the store without triggering pipeline,
        /// security checks, auto-fields, or operation logging.
        /// </summary>
        /// <param name="entities">The entities to seed.</param>
        public void Seed(IEnumerable<Entity> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            foreach (var entity in entities)
            {
                _store.Create(entity);
            }
        }

        /// <summary>
        /// Seeds entities from a JSON string. Expected format:
        /// <code>[{"logicalName":"account","id":"...","attributes":{"name":"Contoso"}}]</code>
        /// Inserts directly without pipeline, security, auto-fields, or operation logging.
        /// Supports string, integer, decimal, and boolean attribute values.
        /// </summary>
        /// <param name="json">A JSON array of entity objects.</param>
        public void SeedFromJson(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array)
                throw new ArgumentException("JSON must be an array of entity objects.", nameof(json));

            foreach (var element in root.EnumerateArray())
            {
                var entity = ParseEntityFromJson(element);
                _store.Create(entity);
            }
        }

        /// <summary>
        /// Seeds entities from a CSV string. The first line must be a header row with column names.
        /// The first column must be <c>logicalName</c> and the optional second column can be <c>id</c>.
        /// Remaining columns become entity attributes. Values are stored as strings.
        /// Inserts directly without pipeline, security, auto-fields, or operation logging.
        /// </summary>
        /// <param name="csv">A CSV string with header row.</param>
        public void SeedFromCsv(string csv)
        {
            if (csv == null) throw new ArgumentNullException(nameof(csv));

            var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length < 2)
                throw new ArgumentException("CSV must contain a header row and at least one data row.", nameof(csv));

            var headers = ParseCsvLine(lines[0]);
            if (headers.Length == 0 || !string.Equals(headers[0], "logicalName", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("First CSV column must be 'logicalName'.", nameof(csv));

            bool hasId = headers.Length > 1 && string.Equals(headers[1], "id", StringComparison.OrdinalIgnoreCase);
            int attrStart = hasId ? 2 : 1;

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var fields = ParseCsvLine(line);
                if (fields.Length == 0 || string.IsNullOrWhiteSpace(fields[0])) continue;

                var entity = new Entity(fields[0]);
                if (hasId && fields.Length > 1 && Guid.TryParse(fields[1], out var id))
                    entity.Id = id;

                for (int j = attrStart; j < headers.Length && j < fields.Length; j++)
                {
                    var value = fields[j];
                    if (!string.IsNullOrEmpty(value))
                        entity[headers[j]] = value;
                }

                _store.Create(entity);
            }
        }

        // ── Snapshot / Scope ─────────────────────────────────────────────────

        /// <summary>
        /// Takes a snapshot of the current store state (entities, binary data, and version counter).
        /// The returned object can be passed to <see cref="RestoreSnapshot"/> to revert changes.
        /// </summary>
        /// <returns>An opaque snapshot token.</returns>
        public object TakeSnapshot()
        {
            var unpublishedSnapshot = _unpublishedRecordStore.TakeSnapshot();
            return new Snapshot(
                _store.TakeSnapshot(),
                CloneBinaryStore(),
                Interlocked.Read(ref _versionCounter),
                unpublishedSnapshot.StoreData,
                unpublishedSnapshot.Entities);
        }

        /// <summary>
        /// Restores the store to a previously captured snapshot.
        /// </summary>
        /// <param name="snapshot">A snapshot token returned by <see cref="TakeSnapshot"/>.</param>
        public void RestoreSnapshot(object snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (!(snapshot is Snapshot s)) throw new ArgumentException("Invalid snapshot object.", nameof(snapshot));

            _store.RestoreSnapshot(s.EntityData);
            RestoreBinaryStore(s.BinaryData);
            Interlocked.Exchange(ref _versionCounter, s.VersionCounter);
            _unpublishedRecordStore.RestoreSnapshot(s.UnpublishedEntityData, s.UnpublishedEntities);
        }

        /// <summary>
        /// Creates a disposable scope that automatically restores the store to its current state
        /// when disposed. Useful for test isolation:
        /// <code>using (environment.Scope()) { /* modifications auto-reverted */ }</code>
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> that restores the snapshot on disposal.</returns>
        public IDisposable Scope()
        {
            return new EnvironmentScope(this);
        }

        // ── Index ────────────────────────────────────────────────────────────

        /// <summary>
        /// Registers an equality-based attribute index to accelerate queries that filter on the
        /// specified attribute using <see cref="ConditionOperator.Equal"/>. Existing entities are
        /// retroactively indexed.
        /// </summary>
        /// <param name="entityName">The logical name of the entity to index.</param>
        /// <param name="attributeName">The logical name of the attribute to index.</param>
        public void AddIndex(string entityName, string attributeName)
        {
            _attributeIndex.AddIndex(entityName, attributeName);

            var entities = _store.GetAll(entityName);
            foreach (var entity in entities)
            {
                if (entity.Contains(attributeName))
                    _attributeIndex.IndexAttribute(entity.LogicalName, entity.Id, attributeName, entity[attributeName]);
            }
        }

        // ── Reset ────────────────────────────────────────────────────────────

        /// <summary>
        /// Clears all data from the in-memory store and binary store.
        /// </summary>
        public void Reset()
        {
            _store.Clear();
            _binaryStore.Clear();
            _unpublishedRecordStore.Clear();
        }

        // ── Clock ────────────────────────────────────────────────────────────

        /// <summary>
        /// Advances the clock by the specified duration. Only works when <see cref="Clock"/>
        /// is a <see cref="FakeClock"/> instance; throws <see cref="InvalidOperationException"/> otherwise.
        /// </summary>
        /// <param name="duration">The time span to advance.</param>
        public void AdvanceTime(TimeSpan duration)
        {
            if (Clock is FakeClock fakeClock)
                fakeClock.Advance(duration);
            else
                throw new InvalidOperationException("AdvanceTime can only be used when Clock is a FakeClock instance.");
        }

        // ── Binary Store ─────────────────────────────────────────────────────

        /// <summary>
        /// Stores binary data (image or file) for a specific entity attribute.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <param name="entityId">The unique identifier of the entity record.</param>
        /// <param name="attributeName">The logical name of the image/file attribute.</param>
        /// <param name="data">The binary data to store.</param>
        public void SetBinaryAttribute(string entityName, Guid entityId, string attributeName, byte[] data)
        {
            if (string.IsNullOrEmpty(entityName)) throw new ArgumentException("Entity name is required.", nameof(entityName));
            if (entityId == Guid.Empty) throw new ArgumentException("Entity ID is required.", nameof(entityId));
            if (string.IsNullOrEmpty(attributeName)) throw new ArgumentException("Attribute name is required.", nameof(attributeName));
            if (data == null) throw new ArgumentNullException(nameof(data));

            _binaryStore[(entityName, entityId, attributeName)] = (byte[])data.Clone();
        }

        /// <summary>
        /// Retrieves binary data (image or file) for a specific entity attribute.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <param name="entityId">The unique identifier of the entity record.</param>
        /// <param name="attributeName">The logical name of the image/file attribute.</param>
        /// <returns>The binary data, or <c>null</c> if not set.</returns>
        public byte[]? GetBinaryAttribute(string entityName, Guid entityId, string attributeName)
        {
            if (_binaryStore.TryGetValue((entityName, entityId, attributeName), out var data))
                return (byte[])data.Clone();
            return null;
        }

        /// <summary>
        /// Removes binary data (image or file) for a specific entity attribute.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <param name="entityId">The unique identifier of the entity record.</param>
        /// <param name="attributeName">The logical name of the image/file attribute.</param>
        /// <returns><c>true</c> if the binary data was removed; <c>false</c> if it did not exist.</returns>
        public bool RemoveBinaryAttribute(string entityName, Guid entityId, string attributeName)
        {
            return _binaryStore.Remove((entityName, entityId, attributeName));
        }

        // ── File Upload Sessions (internal) ──────────────────────────────────

        internal void CreateUploadSession(string token, string entityName, Guid entityId, string attributeName)
        {
            _uploadSessions[token] = new FileUploadSession(entityName, entityId, attributeName);
        }

        internal void AppendUploadBlock(string token, byte[] blockData)
        {
            if (!_uploadSessions.TryGetValue(token, out var session))
                throw DataverseFault.InvalidArgumentFault($"Invalid file continuation token: '{token}'.");
            session.Blocks.Add((byte[])blockData.Clone());
        }

        internal void CommitUploadSession(string token, string fileName)
        {
            if (!_uploadSessions.TryGetValue(token, out var session))
                throw DataverseFault.InvalidArgumentFault($"Invalid file continuation token: '{token}'.");

            int totalLength = 0;
            foreach (var block in session.Blocks) totalLength += block.Length;
            var combined = new byte[totalLength];
            int offset = 0;
            foreach (var block in session.Blocks)
            {
                Array.Copy(block, 0, combined, offset, block.Length);
                offset += block.Length;
            }

            SetBinaryAttribute(session.EntityName, session.EntityId, session.AttributeName, combined);
        }

        internal long GetCommittedFileSize(string token)
        {
            if (!_uploadSessions.TryGetValue(token, out var session))
                return 0;
            var data = GetBinaryAttribute(session.EntityName, session.EntityId, session.AttributeName);
            return data?.Length ?? 0;
        }

        // ── Version Counter (internal) ───────────────────────────────────────

        /// <summary>
        /// Atomically increments and returns the next version number.
        /// </summary>
        internal long IncrementVersion()
        {
            return Interlocked.Increment(ref _versionCounter);
        }

        // ── Status Transitions ───────────────────────────────────────────────

        /// <summary>
        /// Registers a valid status transition for the specified entity.
        /// When transitions are registered, only those transitions will be allowed.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <param name="fromStateCode">The source state code.</param>
        /// <param name="fromStatusCode">The source status code.</param>
        /// <param name="toStateCode">The target state code.</param>
        /// <param name="toStatusCode">The target status code.</param>
        public void RegisterStatusTransition(string entityName, int fromStateCode, int fromStatusCode, int toStateCode, int toStatusCode)
        {
            if (!_statusTransitions.TryGetValue(entityName, out var transitions))
            {
                transitions = new List<StatusTransition>();
                _statusTransitions[entityName] = transitions;
            }
            transitions.Add(new StatusTransition(fromStateCode, fromStatusCode, toStateCode, toStatusCode));
        }

        /// <summary>
        /// Registers custom default state/status codes for an entity type.
        /// These are used when <see cref="FakeOrganizationServiceOptions.AutoSetStateCode"/> is enabled.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <param name="stateCode">The default state code.</param>
        /// <param name="statusCode">The default status code.</param>
        public void RegisterDefaultStatusCode(string entityName, int stateCode, int statusCode)
        {
            _defaultStatusCodes[entityName] = (stateCode, statusCode);
        }

        /// <summary>
        /// Checks if a state transition is valid based on registered transitions.
        /// Returns <c>true</c> if no transitions are registered (all transitions allowed).
        /// </summary>
        internal bool IsValidTransition(string entityName, int fromState, int fromStatus, int toState, int toStatus)
        {
            if (!_statusTransitions.TryGetValue(entityName, out var transitions) || transitions.Count == 0)
                return true;

            return transitions.Any(t =>
                t.FromStateCode == fromState && t.FromStatusCode == fromStatus &&
                t.ToStateCode == toState && t.ToStatusCode == toStatus);
        }

        /// <summary>
        /// Gets the default state and status codes for the specified entity, if registered.
        /// </summary>
        internal (int StateCode, int StatusCode)? GetDefaultStatusCodes(string entityName)
        {
            if (_defaultStatusCodes.TryGetValue(entityName, out var defaults))
                return defaults;
            return null;
        }

        // ── Custom API Registration ──────────────────────────────────────────

        /// <summary>
        /// Registers a custom API handler that matches requests by <see cref="OrganizationRequest.RequestName"/>.
        /// </summary>
        /// <param name="requestName">The request name to handle.</param>
        /// <param name="handler">A function that processes the request and returns a response.</param>
        public void RegisterCustomApi(string requestName, Func<OrganizationRequest, IOrganizationService, OrganizationResponse> handler)
        {
            HandlerRegistry.Register(new Handlers.CustomApiRequestHandler(requestName, handler));
        }

        // ── Solution-Aware Entity Registration ──────────────────────────────

        /// <summary>
        /// Registers an entity type as solution-aware. Solution-aware entities have a
        /// <c>componentstate</c> column; created and updated records are staged in the
        /// unpublished store until published via <c>PublishXmlRequest</c> or <c>PublishAllXmlRequest</c>.
        /// Normal <c>Retrieve</c> and <c>RetrieveMultiple</c> return only published records;
        /// <c>RetrieveUnpublishedRequest</c> and <c>RetrieveUnpublishedMultipleRequest</c> return
        /// only unpublished records. <c>Delete</c> removes from both stores.
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        public void RegisterSolutionAwareEntity(string entityName)
        {
            _unpublishedRecordStore.RegisterSolutionAwareEntity(entityName);
        }

        /// <summary>
        /// Returns <c>true</c> if the specified entity type is solution-aware. An entity is
        /// solution-aware when it has been explicitly registered via
        /// <see cref="RegisterSolutionAwareEntity"/> or when its registered metadata contains a
        /// <c>componentstate</c> attribute (which is the standard column for solution-aware tables).
        /// </summary>
        /// <param name="entityName">The logical name of the entity.</param>
        /// <returns><c>true</c> if the entity is solution-aware; otherwise, <c>false</c>.</returns>
        public bool IsSolutionAwareEntity(string entityName)
        {
            if (_unpublishedRecordStore.IsSolutionAware(entityName))
                return true;

            var meta = MetadataStore.GetEntityMetadataInfo(entityName);
            return meta != null && meta.Attributes.ContainsKey("componentstate");
        }

        // ── Metadata XML Loading ─────────────────────────────────────────────

        /// <summary>
        /// Loads entity metadata from a DataContract-serialized XML string produced by the Dataverse
        /// SDK (e.g. a serialized <c>RetrieveEntityResponse.EntityMetadata</c> or
        /// <c>RetrieveAllEntitiesResponse.EntityMetadata</c> array). The root element must be
        /// <c>&lt;EntityMetadata&gt;</c> (single entity) or <c>&lt;ArrayOfEntityMetadata&gt;</c>
        /// (array).
        /// </summary>
        /// <remarks>
        /// Any entity whose registered metadata includes a <c>componentstate</c> attribute is
        /// automatically treated as solution-aware: records created or updated for that entity are
        /// staged in the unpublished store until published.
        /// </remarks>
        /// <param name="xml">DataContract XML containing serialized entity metadata.</param>
        public void LoadMetadataFromXml(string xml)
        {
            MetadataStore.LoadFromXml(xml);
        }

        /// <summary>
        /// Loads entity metadata from a DataContract-serialized XML file produced by the Dataverse
        /// SDK. The file must contain either a single <c>&lt;EntityMetadata&gt;</c> element or an
        /// <c>&lt;ArrayOfEntityMetadata&gt;</c> root element.
        /// </summary>
        /// <remarks>
        /// Any entity whose registered metadata includes a <c>componentstate</c> attribute is
        /// automatically treated as solution-aware.
        /// </remarks>
        /// <param name="filePath">Path to the XML metadata file.</param>
        public void LoadMetadataFromXmlFile(string filePath)
        {
            MetadataStore.LoadFromXmlFile(filePath);
        }

        // ── Built-in Handler Registration ────────────────────────────────────

        private void RegisterBuiltInHandlers()
        {
            HandlerRegistry.Register(new Handlers.CreateRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveMultipleRequestHandler());
            HandlerRegistry.Register(new Handlers.UpdateRequestHandler());
            HandlerRegistry.Register(new Handlers.DeleteRequestHandler());
            HandlerRegistry.Register(new Handlers.WhoAmIRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveEntityRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveAllEntitiesRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveAttributeRequestHandler());
            HandlerRegistry.Register(new Handlers.SetStateRequestHandler());
            HandlerRegistry.Register(new Handlers.AssignRequestHandler());
            HandlerRegistry.Register(new Handlers.ExecuteMultipleRequestHandler());
            HandlerRegistry.Register(new Handlers.ExecuteTransactionRequestHandler());
            HandlerRegistry.Register(new Handlers.ExecuteFetchRequestHandler());
            HandlerRegistry.Register(new Handlers.ExecuteByIdSavedQueryRequestHandler());
            HandlerRegistry.Register(new Handlers.ExecuteByIdUserQueryRequestHandler());
            HandlerRegistry.Register(new Handlers.AssociateRequestHandler());
            HandlerRegistry.Register(new Handlers.DisassociateRequestHandler());
            HandlerRegistry.Register(new Handlers.UpsertRequestHandler());
            HandlerRegistry.Register(new Handlers.GrantAccessRequestHandler(Security));
            HandlerRegistry.Register(new Handlers.ModifyAccessRequestHandler(Security));
            HandlerRegistry.Register(new Handlers.RevokeAccessRequestHandler(Security));
            HandlerRegistry.Register(new Handlers.RetrievePrincipalAccessRequestHandler(Security, _store));
            HandlerRegistry.Register(new Handlers.RetrieveSharedPrincipalsAndAccessRequestHandler(Security));
            HandlerRegistry.Register(new Handlers.RetrieveUserPrivilegesRequestHandler(Security));
            HandlerRegistry.Register(new Handlers.AddMembersTeamRequestHandler());
            HandlerRegistry.Register(new Handlers.RemoveMembersTeamRequestHandler());
            HandlerRegistry.Register(new Handlers.AddListMembersListRequestHandler());
            HandlerRegistry.Register(new Handlers.RemoveMemberListRequestHandler());
            HandlerRegistry.Register(new Handlers.SendEmailRequestHandler());
            HandlerRegistry.Register(new Handlers.InitializeFromRequestHandler());
            HandlerRegistry.Register(new Handlers.CalculateRollupFieldRequestHandler());
            HandlerRegistry.Register(new Handlers.InitializeFileBlocksUploadRequestHandler());
            HandlerRegistry.Register(new Handlers.UploadBlockRequestHandler());
            HandlerRegistry.Register(new Handlers.CommitFileBlocksUploadRequestHandler());
            HandlerRegistry.Register(new Handlers.CreateMultipleRequestHandler());
            HandlerRegistry.Register(new Handlers.UpdateMultipleRequestHandler());
            HandlerRegistry.Register(new Handlers.DeleteMultipleRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveVersionRequestHandler());
            HandlerRegistry.Register(new Handlers.FetchXmlToQueryExpressionRequestHandler());
            HandlerRegistry.Register(new Handlers.QueryExpressionToFetchXmlRequestHandler());
            HandlerRegistry.Register(new Handlers.IsValidStateTransitionRequestHandler());
            HandlerRegistry.Register(new Handlers.MergeRequestHandler());
            HandlerRegistry.Register(new Handlers.UpsertMultipleRequestHandler());
            HandlerRegistry.Register(new Handlers.BulkDeleteRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveCurrentOrganizationRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveOptionSetRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveExchangeRateRequestHandler());
            HandlerRegistry.Register(new Handlers.InsertOptionValueRequestHandler());
            HandlerRegistry.Register(new Handlers.DownloadBlockRequestHandler());
            HandlerRegistry.Register(new Handlers.DeleteFileRequestHandler());
            HandlerRegistry.Register(new Handlers.QualifyLeadRequestHandler());
            HandlerRegistry.Register(new Handlers.CloseIncidentRequestHandler());
            HandlerRegistry.Register(new Handlers.CloseQuoteRequestHandler());
            HandlerRegistry.Register(new Handlers.ReviseQuoteRequestHandler());
            HandlerRegistry.Register(new Handlers.WinOpportunityRequestHandler());
            HandlerRegistry.Register(new Handlers.LoseOpportunityRequestHandler());
            HandlerRegistry.Register(new Handlers.WinQuoteRequestHandler());
            HandlerRegistry.Register(new Handlers.LockInvoicePricingRequestHandler());
            HandlerRegistry.Register(new Handlers.UnlockInvoicePricingRequestHandler());
            HandlerRegistry.Register(new Handlers.LockSalesOrderPricingRequestHandler());
            HandlerRegistry.Register(new Handlers.UnlockSalesOrderPricingRequestHandler());
            HandlerRegistry.Register(new Handlers.RemoveParentRequestHandler());
            HandlerRegistry.Register(new Handlers.RescheduleRequestHandler());
            HandlerRegistry.Register(new Handlers.RecalculateRequestHandler());
            HandlerRegistry.Register(new Handlers.RenewContractRequestHandler());
            HandlerRegistry.Register(new Handlers.PublishXmlRequestHandler(_unpublishedRecordStore, _store));
            HandlerRegistry.Register(new Handlers.AddToQueueRequestHandler());
            HandlerRegistry.Register(new Handlers.RemoveFromQueueRequestHandler());
            HandlerRegistry.Register(new Handlers.RouteToRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveMembersTeamRequestHandler());
            HandlerRegistry.Register(new Handlers.AddUserToRecordTeamRequestHandler());
            HandlerRegistry.Register(new Handlers.RemoveUserFromRecordTeamRequestHandler());
            HandlerRegistry.Register(new Handlers.InstantiateTemplateRequestHandler());
            HandlerRegistry.Register(new Handlers.SendEmailFromTemplateRequestHandler());
            HandlerRegistry.Register(new Handlers.SendFaxRequestHandler());
            HandlerRegistry.Register(new Handlers.SendTemplateRequestHandler());
            HandlerRegistry.Register(new Handlers.BackgroundSendEmailRequestHandler());
            HandlerRegistry.Register(new Handlers.GetTrackingTokenEmailRequestHandler());
            HandlerRegistry.Register(new Handlers.CheckIncomingEmailRequestHandler());
            HandlerRegistry.Register(new Handlers.CheckPromoteEmailRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveDeploymentLicenseTypeRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveLicenseInfoRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveUserQueuesRequestHandler());
            HandlerRegistry.Register(new Handlers.ExportPdfDocumentRequestHandler());
            HandlerRegistry.Register(new Handlers.GenericCreateRequestHandler());

            // Metadata entity CRUD
            HandlerRegistry.Register(new Handlers.CreateEntityRequestHandler());
            HandlerRegistry.Register(new Handlers.UpdateEntityRequestHandler());
            HandlerRegistry.Register(new Handlers.DeleteEntityRequestHandler());

            // Metadata attribute CRUD
            HandlerRegistry.Register(new Handlers.CreateAttributeRequestHandler());
            HandlerRegistry.Register(new Handlers.UpdateAttributeRequestHandler());
            HandlerRegistry.Register(new Handlers.DeleteAttributeRequestHandler());

            // Relationship CRUD
            HandlerRegistry.Register(new Handlers.CreateOneToManyRequestHandler());
            HandlerRegistry.Register(new Handlers.CreateManyToManyRequestHandler());
            HandlerRegistry.Register(new Handlers.DeleteRelationshipRequestHandler());
            HandlerRegistry.Register(new Handlers.UpdateRelationshipRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveRelationshipRequestHandler());

            // Entity key CRUD
            HandlerRegistry.Register(new Handlers.CreateEntityKeyRequestHandler());
            HandlerRegistry.Register(new Handlers.DeleteEntityKeyRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveEntityKeyRequestHandler());
            HandlerRegistry.Register(new Handlers.ReactivateEntityKeyRequestHandler());

            // OptionSet CRUD
            HandlerRegistry.Register(new Handlers.CreateOptionSetRequestHandler());
            HandlerRegistry.Register(new Handlers.UpdateOptionSetRequestHandler());
            HandlerRegistry.Register(new Handlers.DeleteOptionSetRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveAllOptionSetsRequestHandler());

            // Option value manipulation
            HandlerRegistry.Register(new Handlers.InsertStatusValueRequestHandler());
            HandlerRegistry.Register(new Handlers.DeleteOptionValueRequestHandler());
            HandlerRegistry.Register(new Handlers.UpdateOptionValueRequestHandler());
            HandlerRegistry.Register(new Handlers.OrderOptionRequestHandler());
            HandlerRegistry.Register(new Handlers.UpdateStateValueRequestHandler());

            // Relationship validation
            HandlerRegistry.Register(new Handlers.CanBeReferencedRequestHandler());
            HandlerRegistry.Register(new Handlers.CanBeReferencingRequestHandler());
            HandlerRegistry.Register(new Handlers.CanManyToManyRequestHandler());
            HandlerRegistry.Register(new Handlers.GetValidManyToManyRequestHandler());
            HandlerRegistry.Register(new Handlers.GetValidReferencedEntitiesRequestHandler());
            HandlerRegistry.Register(new Handlers.GetValidReferencingEntitiesRequestHandler());
            HandlerRegistry.Register(new Handlers.CreateCustomerRelationshipsRequestHandler());

            // Metadata query / utility
            HandlerRegistry.Register(new Handlers.RetrieveMetadataChangesRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveTimestampRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveAllManagedPropertiesRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveManagedPropertyRequestHandler());

            // Data encryption
            HandlerRegistry.Register(new Handlers.IsDataEncryptionActiveRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveDataEncryptionKeyRequestHandler());
            HandlerRegistry.Register(new Handlers.SetDataEncryptionKeyRequestHandler());

            // Auto-number
            HandlerRegistry.Register(new Handlers.GetAutoNumberSeedRequestHandler());
            HandlerRegistry.Register(new Handlers.SetAutoNumberSeedRequestHandler());
            HandlerRegistry.Register(new Handlers.GetNextAutoNumberValueRequestHandler());

            // Misc
            HandlerRegistry.Register(new Handlers.ConvertDateAndTimeBehaviorRequestHandler());
            HandlerRegistry.Register(new Handlers.ExecuteAsyncRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveEntityChangesRequestHandler());
            HandlerRegistry.Register(new Handlers.CreateAsyncJobToRevokeInheritedAccessRequestHandler());

            // Queue operations
            HandlerRegistry.Register(new Handlers.PickFromQueueRequestHandler());
            HandlerRegistry.Register(new Handlers.ReleaseToQueueRequestHandler());
            HandlerRegistry.Register(new Handlers.AddPrincipalToQueueRequestHandler());

            // Time zone
            HandlerRegistry.Register(new Handlers.LocalTimeFromUtcTimeRequestHandler());
            HandlerRegistry.Register(new Handlers.UtcTimeFromLocalTimeRequestHandler());

            // File block download
            HandlerRegistry.Register(new Handlers.InitializeFileBlocksDownloadRequestHandler());

            // Annotation blocks
            HandlerRegistry.Register(new Handlers.InitializeAnnotationBlocksUploadRequestHandler());
            HandlerRegistry.Register(new Handlers.CommitAnnotationBlocksUploadRequestHandler());
            HandlerRegistry.Register(new Handlers.InitializeAnnotationBlocksDownloadRequestHandler());

            // Attachment blocks
            HandlerRegistry.Register(new Handlers.InitializeAttachmentBlocksUploadRequestHandler());
            HandlerRegistry.Register(new Handlers.CommitAttachmentBlocksUploadRequestHandler());
            HandlerRegistry.Register(new Handlers.InitializeAttachmentBlocksDownloadRequestHandler());

            // Sales pipeline
            HandlerRegistry.Register(new Handlers.FulfillSalesOrderRequestHandler());
            HandlerRegistry.Register(new Handlers.CancelSalesOrderRequestHandler());
            HandlerRegistry.Register(new Handlers.ConvertQuoteToSalesOrderRequestHandler());
            HandlerRegistry.Register(new Handlers.ConvertSalesOrderToInvoiceRequestHandler());
            HandlerRegistry.Register(new Handlers.GenerateQuoteFromOpportunityRequestHandler());
            HandlerRegistry.Register(new Handlers.GenerateSalesOrderFromOpportunityRequestHandler());
            HandlerRegistry.Register(new Handlers.GenerateInvoiceFromOpportunityRequestHandler());

            // Audit stubs
            HandlerRegistry.Register(new Handlers.DeleteAuditDataRequestHandler());
            HandlerRegistry.Register(new Handlers.DeleteRecordChangeHistoryRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveAuditDetailsRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveAuditPartitionListRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveAttributeChangeHistoryRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveRecordChangeHistoryRequestHandler());

            // Solution & process stubs
            HandlerRegistry.Register(new Handlers.AddSolutionComponentRequestHandler());
            HandlerRegistry.Register(new Handlers.RemoveSolutionComponentRequestHandler());
            HandlerRegistry.Register(new Handlers.ExecuteWorkflowRequestHandler());
            HandlerRegistry.Register(new Handlers.SetProcessRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveProcessInstancesRequestHandler());

            // Publishing stubs
            HandlerRegistry.Register(new Handlers.PublishAllXmlAsyncRequestHandler(_unpublishedRecordStore, _store));
            HandlerRegistry.Register(new Handlers.PublishDuplicateRuleRequestHandler());
            HandlerRegistry.Register(new Handlers.UnpublishDuplicateRuleRequestHandler());
            HandlerRegistry.Register(new Handlers.PublishProductHierarchyRequestHandler());
            HandlerRegistry.Register(new Handlers.PublishThemeRequestHandler());

            // Unpublished record retrieval
            HandlerRegistry.Register(new Handlers.RetrieveUnpublishedRequestHandler(_unpublishedRecordStore));
            HandlerRegistry.Register(new Handlers.RetrieveUnpublishedMultipleRequestHandler(_unpublishedRecordStore, QueryEvaluator, FetchXmlEvaluator));

            // Validation stubs
            HandlerRegistry.Register(new Handlers.ValidateSavedQueryRequestHandler());
            HandlerRegistry.Register(new Handlers.ValidateFetchXmlExpressionRequestHandler());

            // Language stubs
            HandlerRegistry.Register(new Handlers.RetrieveAvailableLanguagesRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveInstalledLanguagePacksRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveInstalledLanguagePackVersionRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveProvisionedLanguagesRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveProvisionedLanguagePackVersionRequestHandler());

            // Feature control stubs
            HandlerRegistry.Register(new Handlers.RetrieveFeatureControlSettingRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveFeatureControlSettingsRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveFeatureControlSettingsByNamespaceRequestHandler());
            HandlerRegistry.Register(new Handlers.SetFeatureStatusRequestHandler());
            HandlerRegistry.Register(new Handlers.UpdateFeatureConfigRequestHandler());

            // Duplicate detection stubs
            HandlerRegistry.Register(new Handlers.BulkDetectDuplicatesRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveDuplicatesRequestHandler());

            // Campaign & marketing stubs
            HandlerRegistry.Register(new Handlers.AddItemCampaignRequestHandler());
            HandlerRegistry.Register(new Handlers.AddItemCampaignActivityRequestHandler());
            HandlerRegistry.Register(new Handlers.RemoveItemCampaignRequestHandler());
            HandlerRegistry.Register(new Handlers.RemoveItemCampaignActivityRequestHandler());
            HandlerRegistry.Register(new Handlers.CopyCampaignRequestHandler());
            HandlerRegistry.Register(new Handlers.CopyCampaignResponseRequestHandler());

            // Marketing list stubs
            HandlerRegistry.Register(new Handlers.AddMemberListRequestHandler());
            HandlerRegistry.Register(new Handlers.CopyMembersListRequestHandler());
            HandlerRegistry.Register(new Handlers.CopyDynamicListToStaticRequestHandler());
            HandlerRegistry.Register(new Handlers.QualifyMemberListRequestHandler());

            // Product stubs
            HandlerRegistry.Register(new Handlers.CloneProductRequestHandler());
            HandlerRegistry.Register(new Handlers.RevertProductRequestHandler());
            HandlerRegistry.Register(new Handlers.AddSubstituteProductRequestHandler());
            HandlerRegistry.Register(new Handlers.RemoveSubstituteProductRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveProductPropertiesRequestHandler());
            HandlerRegistry.Register(new Handlers.UpdateProductPropertiesRequestHandler());

            // Knowledge article stubs
            HandlerRegistry.Register(new Handlers.IncrementKnowledgeArticleViewCountRequestHandler());
            HandlerRegistry.Register(new Handlers.CreateKnowledgeArticleTranslationRequestHandler());
            HandlerRegistry.Register(new Handlers.CreateKnowledgeArticleVersionRequestHandler());

            // Contract stubs
            HandlerRegistry.Register(new Handlers.CancelContractRequestHandler());
            HandlerRegistry.Register(new Handlers.CloneContractRequestHandler());
            HandlerRegistry.Register(new Handlers.RenewEntitlementRequestHandler());

            // Reassignment & security stubs
            HandlerRegistry.Register(new Handlers.ReassignObjectsOwnerRequestHandler());
            HandlerRegistry.Register(new Handlers.ReassignObjectsSystemUserRequestHandler());
            HandlerRegistry.Register(new Handlers.AddPrivilegesRoleRequestHandler());
            HandlerRegistry.Register(new Handlers.RemovePrivilegeRoleRequestHandler());
            HandlerRegistry.Register(new Handlers.ReplacePrivilegesRoleRequestHandler());
            HandlerRegistry.Register(new Handlers.ConvertOwnerTeamToAccessTeamRequestHandler());

            // User/BU hierarchy stubs
            HandlerRegistry.Register(new Handlers.SetBusinessSystemUserRequestHandler());
            HandlerRegistry.Register(new Handlers.SetParentBusinessUnitRequestHandler());
            HandlerRegistry.Register(new Handlers.SetParentSystemUserRequestHandler());
            HandlerRegistry.Register(new Handlers.SetParentTeamRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveAllChildUsersSystemUserRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveSubsidiaryTeamsBusinessUnitRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveSubsidiaryUsersBusinessUnitRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveBusinessHierarchyBusinessUnitRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveTeamsSystemUserRequestHandler());

            // User settings & privilege retrieval stubs
            HandlerRegistry.Register(new Handlers.RetrieveUserSettingsSystemUserRequestHandler());
            HandlerRegistry.Register(new Handlers.UpdateUserSettingsSystemUserRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveRolePrivilegesRoleRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveTeamPrivilegesRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveUserPrivilegeByPrivilegeIdRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveUserPrivilegeByPrivilegeNameRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveUsersPrivilegesThroughTeamsRequestHandler());

            // Opportunity product retrieval stubs
            HandlerRegistry.Register(new Handlers.GetInvoiceProductsFromOpportunityRequestHandler());
            HandlerRegistry.Register(new Handlers.GetQuoteProductsFromOpportunityRequestHandler());
            HandlerRegistry.Register(new Handlers.GetSalesOrderProductsFromOpportunityRequestHandler());

            // Miscellaneous stubs
            HandlerRegistry.Register(new Handlers.IsBackOfficeInstalledRequestHandler());
            HandlerRegistry.Register(new Handlers.IsComponentCustomizableRequestHandler());
            HandlerRegistry.Register(new Handlers.FormatAddressRequestHandler());
            HandlerRegistry.Register(new Handlers.GetDefaultPriceLevelRequestHandler());
            HandlerRegistry.Register(new Handlers.GetQuantityDecimalRequestHandler());
            HandlerRegistry.Register(new Handlers.GetReportHistoryLimitRequestHandler());
            HandlerRegistry.Register(new Handlers.GetTimeZoneCodeByLocalizedNameRequestHandler());
            HandlerRegistry.Register(new Handlers.GetAllTimeZonesWithDisplayNameRequestHandler());
            HandlerRegistry.Register(new Handlers.GetFileSasUrlRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveOrganizationInfoRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrievePrincipalAccessInfoRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveUserLicenseInfoRequestHandler());
            HandlerRegistry.Register(new Handlers.CalculateActualValueOpportunityRequestHandler());
            HandlerRegistry.Register(new Handlers.CalculateTotalTimeIncidentRequestHandler());
            HandlerRegistry.Register(new Handlers.RetrieveTotalRecordCountRequestHandler());
            HandlerRegistry.Register(new Handlers.MakeAvailableToOrganizationTemplateRequestHandler());
            HandlerRegistry.Register(new Handlers.MakeUnavailableToOrganizationTemplateRequestHandler());
        }

        // ── Private Helpers ──────────────────────────────────────────────────

        private static Entity ParseEntityFromJson(JsonElement element)
        {
            var logicalName = element.GetProperty("logicalName").GetString()
                ?? throw new ArgumentException("Entity must have a 'logicalName' property.");

            var entity = new Entity(logicalName);

            if (element.TryGetProperty("id", out var idElement))
            {
                var idString = idElement.GetString();
                if (idString != null && Guid.TryParse(idString, out var id))
                    entity.Id = id;
            }

            if (element.TryGetProperty("attributes", out var attrsElement))
            {
                foreach (var attr in attrsElement.EnumerateObject())
                {
                    entity[attr.Name] = ConvertJsonValue(attr.Value);
                }
            }

            return entity;
        }

        private static object? ConvertJsonValue(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.String:
                    return value.GetString();
                case JsonValueKind.Number:
                    if (value.TryGetInt32(out var intVal))
                        return intVal;
                    if (value.TryGetInt64(out var longVal))
                        return longVal;
                    return value.GetDecimal();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                default:
                    return value.ToString();
            }
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            int i = 0;
            while (i < line.Length)
            {
                if (line[i] == '"')
                {
                    i++; // skip opening quote
                    int start = i;
                    var sb = new System.Text.StringBuilder();
                    while (i < line.Length)
                    {
                        if (line[i] == '"')
                        {
                            if (i + 1 < line.Length && line[i + 1] == '"')
                            {
                                sb.Append(line, start, i - start);
                                sb.Append('"');
                                i += 2;
                                start = i;
                            }
                            else
                            {
                                sb.Append(line, start, i - start);
                                i++; // skip closing quote
                                break;
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }
                    result.Add(sb.ToString());
                    if (i < line.Length && line[i] == ',') i++; // skip comma
                }
                else
                {
                    int start = i;
                    while (i < line.Length && line[i] != ',') i++;
                    result.Add(line.Substring(start, i - start));
                    if (i < line.Length) i++; // skip comma
                }
            }
            return result.ToArray();
        }

        private Dictionary<(string EntityName, Guid EntityId, string AttributeName), byte[]> CloneBinaryStore()
        {
            var clone = new Dictionary<(string, Guid, string), byte[]>();
            foreach (var kvp in _binaryStore)
            {
                clone[kvp.Key] = (byte[])kvp.Value.Clone();
            }
            return clone;
        }

        private void RestoreBinaryStore(Dictionary<(string EntityName, Guid EntityId, string AttributeName), byte[]> data)
        {
            _binaryStore.Clear();
            foreach (var kvp in data)
            {
                _binaryStore[kvp.Key] = (byte[])kvp.Value.Clone();
            }
        }

        // ── Nested Private Types ─────────────────────────────────────────────

        private sealed class StatusTransition
        {
            public int FromStateCode { get; }
            public int FromStatusCode { get; }
            public int ToStateCode { get; }
            public int ToStatusCode { get; }

            public StatusTransition(int fromState, int fromStatus, int toState, int toStatus)
            {
                FromStateCode = fromState;
                FromStatusCode = fromStatus;
                ToStateCode = toState;
                ToStatusCode = toStatus;
            }
        }

        private sealed class FileUploadSession
        {
            internal string EntityName { get; }
            internal Guid EntityId { get; }
            internal string AttributeName { get; }
            internal List<byte[]> Blocks { get; } = new List<byte[]>();

            internal FileUploadSession(string entityName, Guid entityId, string attributeName)
            {
                EntityName = entityName;
                EntityId = entityId;
                AttributeName = attributeName;
            }
        }

        private sealed class Snapshot
        {
            internal Dictionary<string, Dictionary<Guid, Entity>> EntityData { get; }
            internal Dictionary<(string EntityName, Guid EntityId, string AttributeName), byte[]> BinaryData { get; }
            internal long VersionCounter { get; }
            internal Dictionary<string, Dictionary<Guid, Entity>> UnpublishedEntityData { get; }
            internal HashSet<string> UnpublishedEntities { get; }

            internal Snapshot(
                Dictionary<string, Dictionary<Guid, Entity>> entityData,
                Dictionary<(string, Guid, string), byte[]> binaryData,
                long versionCounter,
                Dictionary<string, Dictionary<Guid, Entity>> unpublishedEntityData,
                HashSet<string> unpublishedEntities)
            {
                EntityData = entityData;
                BinaryData = binaryData;
                VersionCounter = versionCounter;
                UnpublishedEntityData = unpublishedEntityData;
                UnpublishedEntities = unpublishedEntities;
            }
        }

        private sealed class EnvironmentScope : IDisposable
        {
            private readonly FakeDataverseEnvironment _environment;
            private readonly object _snapshot;
            private bool _disposed;

            internal EnvironmentScope(FakeDataverseEnvironment environment)
            {
                _environment = environment;
                _snapshot = environment.TakeSnapshot();
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _environment.RestoreSnapshot(_snapshot);
                    _disposed = true;
                }
            }
        }
    }
}
