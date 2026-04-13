using System;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse
{
    /// <summary>
    /// Represents a recorded operation performed against the fake organization service.
    /// </summary>
    public sealed class OperationRecord
    {
        /// <summary>
        /// Gets the type of operation (e.g. "Create", "Update", "Delete", "Retrieve", "RetrieveMultiple", "Execute", "Associate", "Disassociate").
        /// </summary>
        public string OperationType { get; }

        /// <summary>
        /// Gets the logical name of the entity involved, or <c>null</c> for operations without a specific entity.
        /// </summary>
        public string? EntityName { get; }

        /// <summary>
        /// Gets the unique identifier of the entity involved, or <c>null</c> for operations without a specific record.
        /// </summary>
        public Guid? EntityId { get; }

        /// <summary>
        /// Gets the UTC timestamp when the operation was recorded.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets a snapshot of the entity at operation time, or <c>null</c> if not applicable.
        /// </summary>
        public Entity? Entity { get; }

        /// <summary>
        /// Gets the organization request for Execute operations, or <c>null</c> if not applicable.
        /// </summary>
        public OrganizationRequest? Request { get; }

        internal OperationRecord(string operationType, string? entityName, Guid? entityId, DateTime timestamp, Entity? entity, OrganizationRequest? request)
        {
            OperationType = operationType;
            EntityName = entityName;
            EntityId = entityId;
            Timestamp = timestamp;
            Entity = entity;
            Request = request;
        }
    }
}
