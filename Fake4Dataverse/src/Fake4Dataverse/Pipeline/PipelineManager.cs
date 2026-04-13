using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Pipeline
{
    /// <summary>
    /// Manages a plugin-like execution pipeline with pre-validation, pre-operation,
    /// core operation, and post-operation stages.
    /// Supports both lambda callbacks (<see cref="Action{T}"/> of <see cref="IPluginExecutionContext"/>)
    /// and real <see cref="IPlugin"/> instances for end-to-end plugin testing.
    /// </summary>
    public sealed class PipelineManager
    {
        private readonly object _lock = new object();
        private readonly List<StepEntry> _steps = new List<StepEntry>();
        private readonly Func<Guid?, IOrganizationService>? _serviceFactory;
        private readonly Func<string, Guid, Entity?>? _entityRetriever;
        private readonly FakeTracingService _tracingService = new FakeTracingService();
        private static readonly AsyncLocal<int> _currentDepth = new AsyncLocal<int>();

        /// <summary>
        /// Maximum allowed plugin execution depth. Matches Dataverse server behavior.
        /// </summary>
        internal const int MaxDepth = 8;

        /// <summary>
        /// Gets all trace messages written by plugins since the last <see cref="ClearTraces"/> call.
        /// </summary>
        public IReadOnlyList<string> Traces => _tracingService.Traces;

        /// <summary>Removes all captured plugin trace messages.</summary>
        public void ClearTraces() => _tracingService.Clear();

        /// <summary>
        /// Initializes a <see cref="PipelineManager"/> with no service factory.
        /// Registering <see cref="IPlugin"/> steps requires using the factory-enabled overload
        /// (done automatically when the pipeline is created by <see cref="FakeOrganizationService"/>).
        /// </summary>
        public PipelineManager() : this(null, null) { }

        /// <summary>
        /// Initializes a <see cref="PipelineManager"/> with a service factory used to supply
        /// <see cref="IOrganizationServiceFactory"/> when executing <see cref="IPlugin"/> steps.
        /// </summary>
        /// <param name="serviceFactory">
        /// Factory delegate invoked with an optional user ID; return the <see cref="IOrganizationService"/>
        /// that plugin code should use.
        /// </param>
        /// <param name="entityRetriever">
        /// Optional delegate that retrieves an entity by logical name and ID for image capture.
        /// Returns <c>null</c> if the entity does not exist.
        /// </param>
        internal PipelineManager(Func<Guid?, IOrganizationService>? serviceFactory, Func<string, Guid, Entity?>? entityRetriever = null)
        {
            _serviceFactory = serviceFactory;
            _entityRetriever = entityRetriever;
        }

        // ── RegisterStep (callback) ──────────────────────────────────────────

        /// <summary>
        /// Registers a pipeline step callback for a specific message and stage (all entities).
        /// Returns a disposable handle to unregister the step.
        /// </summary>
        /// <param name="messageName">The message name, e.g. "Create", "Update", "Delete".</param>
        /// <param name="stage">The pipeline stage to fire at.</param>
        /// <param name="callback">The callback to invoke with the execution context.</param>
        public PipelineStepRegistration RegisterStep(
            string messageName,
            PipelineStage stage,
            Action<IPluginExecutionContext> callback)
        {
            return RegisterStep(messageName, stage, null, callback);
        }

        /// <summary>
        /// Registers a pipeline step callback scoped to a specific entity, message, and stage.
        /// Returns a disposable handle to unregister the step.
        /// </summary>
        /// <param name="messageName">The message name, e.g. "Create", "Update", "Delete".</param>
        /// <param name="stage">The pipeline stage to fire at.</param>
        /// <param name="entityName">Entity logical name to scope to, or <c>null</c> for all entities.</param>
        /// <param name="callback">The callback to invoke with the execution context.</param>
        public PipelineStepRegistration RegisterStep(
            string messageName,
            PipelineStage stage,
            string? entityName,
            Action<IPluginExecutionContext> callback)
        {
            if (messageName == null) throw new ArgumentNullException(nameof(messageName));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var entry = new StepEntry(messageName, stage, entityName, callback, null!);
            var registration = new PipelineStepRegistration(() => { lock (_lock) { _steps.Remove(entry); } });
            entry.Registration = registration;
            lock (_lock) { _steps.Add(entry); }
            return registration;
        }

        // ── RegisterStep (IPlugin) ───────────────────────────────────────────

        /// <summary>
        /// Registers a real <see cref="IPlugin"/> instance for a specific message and stage (all entities).
        /// The plugin's <see cref="IPlugin.Execute"/> method is called with a fully-populated
        /// <see cref="IServiceProvider"/> that resolves <see cref="IPluginExecutionContext"/>,
        /// <see cref="IOrganizationServiceFactory"/>, and <see cref="ITracingService"/>.
        /// Returns a disposable handle to unregister the step.
        /// </summary>
        /// <param name="messageName">The message name, e.g. "Create", "Update", "Delete".</param>
        /// <param name="stage">The pipeline stage to fire at.</param>
        /// <param name="plugin">The plugin instance to execute.</param>
        public PipelineStepRegistration RegisterStep(
            string messageName,
            PipelineStage stage,
            IPlugin plugin)
        {
            return RegisterStep(messageName, stage, null, plugin);
        }

        /// <summary>
        /// Registers a real <see cref="IPlugin"/> instance scoped to a specific entity, message, and stage.
        /// The plugin's <see cref="IPlugin.Execute"/> method is called with a fully-populated
        /// <see cref="IServiceProvider"/> that resolves <see cref="IPluginExecutionContext"/>,
        /// <see cref="IOrganizationServiceFactory"/>, and <see cref="ITracingService"/>.
        /// Returns a disposable handle to unregister the step.
        /// </summary>
        /// <param name="messageName">The message name, e.g. "Create", "Update", "Delete".</param>
        /// <param name="stage">The pipeline stage to fire at.</param>
        /// <param name="entityName">Entity logical name to scope to, or <c>null</c> for all entities.</param>
        /// <param name="plugin">The plugin instance to execute.</param>
        public PipelineStepRegistration RegisterStep(
            string messageName,
            PipelineStage stage,
            string? entityName,
            IPlugin plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));

            return RegisterStep(messageName, stage, entityName, ctx =>
            {
                if (_serviceFactory == null)
                    throw new InvalidOperationException(
                        "IPlugin steps require a service factory. Create PipelineManager through " +
                        "FakeOrganizationService.Pipeline rather than instantiating it directly.");

                var factory = new FakeOrganizationServiceFactory(_serviceFactory);
                var serviceProvider = new FakePluginServiceProvider(ctx, factory, _tracingService);
                plugin.Execute(serviceProvider);
            });
        }

        // ── Convenience helpers ──────────────────────────────────────────────

        /// <summary>Registers a pre-validation callback (stage 10) for a message across all entities.</summary>
        public PipelineStepRegistration RegisterPreValidation(string messageName, Action<IPluginExecutionContext> callback)
            => RegisterStep(messageName, PipelineStage.PreValidation, callback);

        /// <summary>Registers a pre-validation callback (stage 10) scoped to a specific entity.</summary>
        public PipelineStepRegistration RegisterPreValidation(string messageName, string? entityName, Action<IPluginExecutionContext> callback)
            => RegisterStep(messageName, PipelineStage.PreValidation, entityName, callback);

        /// <summary>Registers a pre-operation callback (stage 20) for a message across all entities.</summary>
        public PipelineStepRegistration RegisterPreOperation(string messageName, Action<IPluginExecutionContext> callback)
            => RegisterStep(messageName, PipelineStage.PreOperation, callback);

        /// <summary>Registers a pre-operation callback (stage 20) scoped to a specific entity.</summary>
        public PipelineStepRegistration RegisterPreOperation(string messageName, string? entityName, Action<IPluginExecutionContext> callback)
            => RegisterStep(messageName, PipelineStage.PreOperation, entityName, callback);

        /// <summary>Registers a post-operation callback (stage 40) for a message across all entities.</summary>
        public PipelineStepRegistration RegisterPostOperation(string messageName, Action<IPluginExecutionContext> callback)
            => RegisterStep(messageName, PipelineStage.PostOperation, callback);

        /// <summary>Registers a post-operation callback (stage 40) scoped to a specific entity.</summary>
        public PipelineStepRegistration RegisterPostOperation(string messageName, string? entityName, Action<IPluginExecutionContext> callback)
            => RegisterStep(messageName, PipelineStage.PostOperation, entityName, callback);

        // ── Internal execution ───────────────────────────────────────────────

        /// <summary>
        /// Executes the pipeline: fires pre-stages, runs the core operation, then fires post-stage.
        /// </summary>
        internal FakePipelineContext Execute(
            string messageName,
            string entityName,
            ParameterCollection inputParams,
            Func<FakePipelineContext, ParameterCollection> coreOperation,
            Guid userId,
            Guid initiatingUserId,
            Guid businessUnitId,
            Guid organizationId,
            string organizationName,
            DateTime operationCreatedOn,
            FakePipelineContextSettings settings = default)
        {
            var depth = _currentDepth.Value + 1;
            if (depth > MaxDepth)
                throw new InvalidPluginExecutionException($"This workflow job was canceled because the workflow that started it included an infinite loop. Correct the workflow logic and try again. (Depth {depth} exceeds maximum {MaxDepth}).");

            var previousDepth = _currentDepth.Value;
            _currentDepth.Value = depth;
            try
            {
            var context = new FakePipelineContext(
                messageName, entityName, inputParams,
                userId, initiatingUserId, businessUnitId,
                organizationId, organizationName, operationCreatedOn,
                depth: depth, settings: settings);

            // Capture pre-image snapshot (entity state before core operation)
            Entity? preImageSnapshot = null;
            var targetId = GetTargetEntityId(inputParams);
            if (targetId != Guid.Empty && _entityRetriever != null)
                preImageSnapshot = _entityRetriever(entityName, targetId);

            // Pre-validation
            FireStage(context, PipelineStage.PreValidation, preImageSnapshot, null);

            // Pre-operation
            FireStage(context, PipelineStage.PreOperation, preImageSnapshot, null);

            // Core operation
            var outputParams = coreOperation(context);
            foreach (var kvp in outputParams)
                context.OutputParameters[kvp.Key] = kvp.Value;

            // Capture post-image snapshot (entity state after core operation)
            Entity? postImageSnapshot = null;
            var postTargetId = targetId;
            if (postTargetId == Guid.Empty && context.OutputParameters.ContainsKey("id"))
                postTargetId = (Guid)context.OutputParameters["id"];
            if (postTargetId != Guid.Empty && _entityRetriever != null
                && !string.Equals(messageName, "Delete", StringComparison.OrdinalIgnoreCase))
                postImageSnapshot = _entityRetriever(entityName, postTargetId);

            // Post-operation
            FireStage(context, PipelineStage.PostOperation, preImageSnapshot, postImageSnapshot);

            return context;
            }
            finally
            {
                _currentDepth.Value = previousDepth;
            }
        }

        internal bool HasSteps
        {
            get { lock (_lock) { return _steps.Count > 0; } }
        }

        private void FireStage(FakePipelineContext context, PipelineStage stage, Entity? preImageSnapshot, Entity? postImageSnapshot)
        {
            context.Stage = (int)stage;
            List<StepEntry> matching;
            lock (_lock)
            {
                matching = _steps
                    .Where(s =>
                        string.Equals(s.MessageName, context.MessageName, StringComparison.OrdinalIgnoreCase)
                        && s.Stage == stage
                        && (s.EntityName == null || string.Equals(s.EntityName, context.PrimaryEntityName, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
            foreach (var step in matching)
            {
                // Populate images and mode for this step
                context.PreEntityImages.Clear();
                context.PostEntityImages.Clear();
                context.Mode = step.Registration.Mode;

                if (preImageSnapshot != null)
                {
                    foreach (var def in step.Registration.PreImageDefinitions)
                        context.PreEntityImages[def.Name] = ProjectImage(preImageSnapshot, def.Attributes);
                }

                if (postImageSnapshot != null)
                {
                    foreach (var def in step.Registration.PostImageDefinitions)
                        context.PostEntityImages[def.Name] = ProjectImage(postImageSnapshot, def.Attributes);
                }

                step.Callback(context);
            }
        }

        private static Guid GetTargetEntityId(ParameterCollection inputParams)
        {
            if (!inputParams.ContainsKey("Target")) return Guid.Empty;
            var target = inputParams["Target"];
            if (target is Entity e) return e.Id;
            if (target is EntityReference er) return er.Id;
            return Guid.Empty;
        }

        private static Entity ProjectImage(Entity snapshot, string[] attributes)
        {
            var image = new Entity(snapshot.LogicalName, snapshot.Id);
            if (attributes.Length == 0)
            {
                foreach (var attr in snapshot.Attributes)
                    image[attr.Key] = InMemoryEntityStore.CloneAttributeValue(attr.Value);
            }
            else
            {
                foreach (var attr in attributes)
                {
                    if (snapshot.Contains(attr))
                        image[attr] = InMemoryEntityStore.CloneAttributeValue(snapshot[attr]);
                }
            }
            return image;
        }

        private sealed class StepEntry
        {
            public string MessageName { get; }
            public PipelineStage Stage { get; }
            public string? EntityName { get; }
            public Action<IPluginExecutionContext> Callback { get; }
            public PipelineStepRegistration Registration { get; set; }

            public StepEntry(string messageName, PipelineStage stage, string? entityName, Action<IPluginExecutionContext> callback, PipelineStepRegistration registration)
            {
                MessageName = messageName;
                Stage = stage;
                EntityName = entityName;
                Callback = callback;
                Registration = registration;
            }
        }
    }
}
