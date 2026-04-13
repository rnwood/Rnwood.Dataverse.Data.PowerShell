using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse
{
    /// <summary>
    /// Records all operations performed against the fake organization service for post-hoc assertions.
    /// </summary>
    public sealed class OperationLog
    {
        private readonly List<OperationRecord> _records = new List<OperationRecord>();

        /// <summary>
        /// Gets the list of all recorded operations.
        /// </summary>
        public IReadOnlyList<OperationRecord> Records => _records;

        /// <summary>
        /// Clears all recorded operations.
        /// </summary>
        public void Clear() => _records.Clear();

        internal void Add(OperationRecord record) => _records.Add(record);

        /// <summary>
        /// Returns <c>true</c> if a Create operation was recorded for the specified entity type.
        /// </summary>
        public bool HasCreated(string entityName) =>
            _records.Any(r => r.OperationType == "Create" && string.Equals(r.EntityName, entityName, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Returns <c>true</c> if a Create operation was recorded for the specified entity type and ID.
        /// </summary>
        public bool HasCreated(string entityName, Guid id) =>
            _records.Any(r => r.OperationType == "Create" && string.Equals(r.EntityName, entityName, StringComparison.OrdinalIgnoreCase) && r.EntityId == id);

        /// <summary>
        /// Returns <c>true</c> if an Update operation was recorded for the specified entity type and ID.
        /// </summary>
        public bool HasUpdated(string entityName, Guid id) =>
            _records.Any(r => r.OperationType == "Update" && string.Equals(r.EntityName, entityName, StringComparison.OrdinalIgnoreCase) && r.EntityId == id);

        /// <summary>
        /// Returns <c>true</c> if a Delete operation was recorded for the specified entity type and ID.
        /// </summary>
        public bool HasDeleted(string entityName, Guid id) =>
            _records.Any(r => r.OperationType == "Delete" && string.Equals(r.EntityName, entityName, StringComparison.OrdinalIgnoreCase) && r.EntityId == id);

        /// <summary>
        /// Returns <c>true</c> if an Execute operation was recorded for the specified request type.
        /// </summary>
        public bool HasExecuted<TRequest>() where TRequest : OrganizationRequest =>
            _records.Any(r => r.OperationType == "Execute" && r.Request is TRequest);

        /// <summary>
        /// Returns <c>true</c> if an Execute operation was recorded for the specified request name.
        /// </summary>
        public bool HasExecuted(string requestName) =>
            _records.Any(r => r.OperationType == "Execute" && string.Equals(r.Request?.RequestName, requestName, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Returns all recorded operations matching the specified operation type.
        /// </summary>
        public IReadOnlyList<OperationRecord> GetOperations(string operationType) =>
            _records.Where(r => string.Equals(r.OperationType, operationType, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary>
        /// Returns all recorded operations matching the specified operation type and entity name.
        /// </summary>
        public IReadOnlyList<OperationRecord> GetOperations(string operationType, string entityName) =>
            _records.Where(r => string.Equals(r.OperationType, operationType, StringComparison.OrdinalIgnoreCase)
                              && string.Equals(r.EntityName, entityName, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
