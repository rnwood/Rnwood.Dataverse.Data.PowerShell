using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.Pipeline
{
    /// <summary>
    /// Holds optional v2–v7 <see cref="IPluginExecutionContext"/> properties for
    /// <see cref="FakePipelineContext"/> construction.
    /// </summary>
    internal struct FakePipelineContextSettings
    {
        /// <summary>Application ID of the initiating user; <see cref="Guid.Empty"/> if not an app user.</summary>
        public Guid InitiatingUserApplicationId { get; set; }

        /// <summary>Azure Active Directory object ID of the initiating user.</summary>
        public Guid InitiatingUserAzureActiveDirectoryObjectId { get; set; }

        /// <summary>Whether the call originated from Power Pages / portals.</summary>
        public bool IsPortalsClientCall { get; set; }

        /// <summary>Contact ID for portals calls; <see cref="Guid.Empty"/> otherwise.</summary>
        public Guid PortalsContactId { get; set; }

        /// <summary>Azure Active Directory object ID of the executing user.</summary>
        public Guid UserAzureActiveDirectoryObjectId { get; set; }

        /// <summary>ID of the authenticated user (distinct from the impersonated <c>UserId</c>).</summary>
        public Guid AuthenticatedUserId { get; set; }

        /// <summary>HTTP user-agent string of the caller.</summary>
        public string InitiatingUserAgent { get; set; }

        /// <summary>Power Platform environment ID string.</summary>
        public string EnvironmentId { get; set; }

        /// <summary>Azure AD tenant ID.</summary>
        public Guid TenantId { get; set; }

        /// <summary>Whether <c>UserId</c> refers to an Application User.</summary>
        public bool IsApplicationUser { get; set; }
    }

    /// <summary>
    /// In-memory implementation of <see cref="IPluginExecutionContext7"/> passed to pipeline step
    /// callbacks and <see cref="IPlugin"/> instances during test execution.
    /// </summary>
    public sealed class FakePipelineContext : IPluginExecutionContext7
    {
        // ── IExecutionContext ────────────────────────────────────────────────

        /// <inheritdoc />
        public string MessageName { get; }

        /// <inheritdoc />
        public string PrimaryEntityName { get; }

        /// <inheritdoc />
        public Guid PrimaryEntityId { get; set; }

        /// <inheritdoc />
        public ParameterCollection InputParameters { get; }

        /// <inheritdoc />
        public ParameterCollection OutputParameters { get; }

        /// <inheritdoc />
        public EntityImageCollection PreEntityImages { get; } = new EntityImageCollection();

        /// <inheritdoc />
        public EntityImageCollection PostEntityImages { get; } = new EntityImageCollection();

        /// <summary>
        /// Gets the current pipeline stage integer value as defined by <see cref="IExecutionContext"/>.
        /// The typed equivalent is available via <see cref="PipelineStage"/>.
        /// </summary>
        public int Stage { get; internal set; }

        /// <summary>
        /// Gets the current pipeline stage as a typed <see cref="Fake4Dataverse.Pipeline.PipelineStage"/> value.
        /// </summary>
        public PipelineStage PipelineStage => (PipelineStage)Stage;

        /// <inheritdoc />
        public int Depth { get; }

        /// <inheritdoc />
        public Guid UserId { get; }

        /// <inheritdoc />
        public Guid InitiatingUserId { get; }

        /// <inheritdoc />
        public Guid BusinessUnitId { get; }

        /// <inheritdoc />
        public Guid OrganizationId { get; }

        /// <inheritdoc />
        public string OrganizationName { get; }

        /// <inheritdoc />
        public ParameterCollection SharedVariables { get; } = new ParameterCollection();

        /// <inheritdoc />
        public Guid CorrelationId { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public Guid OperationId { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public DateTime OperationCreatedOn { get; }

        /// <inheritdoc />
        public Guid? RequestId { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public string SecondaryEntityName => string.Empty;

        /// <inheritdoc />
        public EntityReference OwningExtension => new EntityReference();

        /// <inheritdoc />
        public bool IsExecutingOffline => false;

        /// <inheritdoc />
        public bool IsOfflinePlayback => false;

        /// <inheritdoc />
        public bool IsInTransaction => true;

        /// <inheritdoc />
        public int IsolationMode => 1; // Sandbox

        /// <inheritdoc />
        public int Mode { get; internal set; }

        // ── IPluginExecutionContext ──────────────────────────────────────────

        /// <inheritdoc />
        public IPluginExecutionContext ParentContext => null!;

        // ── IPluginExecutionContext2 ─────────────────────────────────────────

        /// <inheritdoc />
        public Guid InitiatingUserApplicationId { get; }

        /// <inheritdoc />
        public Guid InitiatingUserAzureActiveDirectoryObjectId { get; }

        /// <inheritdoc />
        public bool IsPortalsClientCall { get; }

        /// <inheritdoc />
        public Guid PortalsContactId { get; }

        /// <inheritdoc />
        public Guid UserAzureActiveDirectoryObjectId { get; }

        // ── IPluginExecutionContext3 ─────────────────────────────────────────

        /// <inheritdoc />
        public Guid AuthenticatedUserId { get; }

        // ── IPluginExecutionContext4 ─────────────────────────────────────────

        /// <inheritdoc />
        public EntityImageCollection[] PreEntityImagesCollection => new[] { PreEntityImages };

        /// <inheritdoc />
        public EntityImageCollection[] PostEntityImagesCollection => new[] { PostEntityImages };

        // ── IPluginExecutionContext5 ─────────────────────────────────────────

        /// <inheritdoc />
        public string InitiatingUserAgent { get; }

        // ── IPluginExecutionContext6 ─────────────────────────────────────────

        /// <inheritdoc />
        public string EnvironmentId { get; }

        /// <inheritdoc />
        public Guid TenantId { get; }

        // ── IPluginExecutionContext7 ─────────────────────────────────────────

        /// <inheritdoc />
        public bool IsApplicationUser { get; }

        // ── Constructors ─────────────────────────────────────────────────────

        /// <summary>
        /// Initializes a new <see cref="FakePipelineContext"/> with default values for all
        /// v2–v7 properties.
        /// </summary>
        internal FakePipelineContext(
            string messageName,
            string primaryEntityName,
            ParameterCollection inputParameters,
            Guid userId,
            Guid initiatingUserId,
            Guid businessUnitId,
            Guid organizationId,
            string organizationName,
            DateTime operationCreatedOn,
            int depth = 1)
            : this(
                messageName,
                primaryEntityName,
                inputParameters,
                userId,
                initiatingUserId,
                businessUnitId,
                organizationId,
                organizationName,
                operationCreatedOn,
                depth,
                settings: default)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="FakePipelineContext"/> with explicit v2–v7 settings.
        /// </summary>
        internal FakePipelineContext(
            string messageName,
            string primaryEntityName,
            ParameterCollection inputParameters,
            Guid userId,
            Guid initiatingUserId,
            Guid businessUnitId,
            Guid organizationId,
            string organizationName,
            DateTime operationCreatedOn,
            int depth,
            FakePipelineContextSettings settings)
        {
            MessageName = messageName ?? throw new ArgumentNullException(nameof(messageName));
            PrimaryEntityName = primaryEntityName ?? string.Empty;
            InputParameters = inputParameters ?? throw new ArgumentNullException(nameof(inputParameters));
            OutputParameters = new ParameterCollection();
            UserId = userId;
            InitiatingUserId = initiatingUserId;
            BusinessUnitId = businessUnitId;
            OrganizationId = organizationId;
            OrganizationName = organizationName ?? string.Empty;
            OperationCreatedOn = operationCreatedOn;
            Depth = depth;
            InitiatingUserApplicationId = settings.InitiatingUserApplicationId;
            InitiatingUserAzureActiveDirectoryObjectId = settings.InitiatingUserAzureActiveDirectoryObjectId;
            IsPortalsClientCall = settings.IsPortalsClientCall;
            PortalsContactId = settings.PortalsContactId;
            UserAzureActiveDirectoryObjectId = settings.UserAzureActiveDirectoryObjectId;
            AuthenticatedUserId = settings.AuthenticatedUserId;
            InitiatingUserAgent = settings.InitiatingUserAgent ?? string.Empty;
            EnvironmentId = settings.EnvironmentId ?? string.Empty;
            TenantId = settings.TenantId;
            IsApplicationUser = settings.IsApplicationUser;
        }
    }
}
