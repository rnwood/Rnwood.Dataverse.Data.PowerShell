using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse
{
    /// <summary>
    /// Provides fluent assertion methods over a <see cref="FakeOrganizationService"/> operation log.
    /// Obtain via <see cref="FakeOrganizationServiceAssertionExtensions.Should"/>.
    /// </summary>
    public sealed class FakeOrganizationServiceAssertions
    {
        private readonly FakeOrganizationService _service;

        internal FakeOrganizationServiceAssertions(FakeOrganizationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Asserts that a Create operation was recorded for the specified entity type.
        /// </summary>
        public FakeOrganizationServiceAssertions HaveCreated(string entityName)
        {
            if (!_service.OperationLog.HasCreated(entityName))
            {
                throw new FakeServiceAssertionException(
                    $"Expected a Create operation for entity '{entityName}', but none was recorded.");
            }

            return this;
        }

        /// <summary>
        /// Asserts that a Create operation was recorded for the specified entity type and ID.
        /// </summary>
        public FakeOrganizationServiceAssertions HaveCreated(string entityName, Guid id)
        {
            if (!_service.OperationLog.HasCreated(entityName, id))
            {
                throw new FakeServiceAssertionException(
                    $"Expected a Create operation for entity '{entityName}' with ID '{id}', but none was recorded.");
            }

            return this;
        }

        /// <summary>
        /// Asserts that an Update operation was recorded for the specified entity type and ID.
        /// </summary>
        public FakeOrganizationServiceAssertions HaveUpdated(string entityName, Guid id)
        {
            if (!_service.OperationLog.HasUpdated(entityName, id))
            {
                throw new FakeServiceAssertionException(
                    $"Expected an Update operation for entity '{entityName}' with ID '{id}', but none was recorded.");
            }

            return this;
        }

        /// <summary>
        /// Asserts that an Update operation was recorded for the specified entity type, ID,
        /// and that the update included the specified attribute values.
        /// </summary>
        public FakeOrganizationServiceAssertions HaveUpdated(string entityName, Guid id, params (string attributeName, object? expectedValue)[] attrs)
        {
            var updates = _service.OperationLog.GetOperations("Update", entityName);
            var match = updates.FirstOrDefault(r =>
                r.EntityId == id && r.Entity != null &&
                attrs.All(a => r.Entity.Contains(a.attributeName) && Equals(r.Entity[a.attributeName], a.expectedValue)));

            if (match == null)
            {
                throw new FakeServiceAssertionException(
                    $"Expected an Update operation for entity '{entityName}' with ID '{id}' containing the specified attributes, but none matched.");
            }

            return this;
        }

        /// <summary>
        /// Asserts that a Delete operation was recorded for the specified entity type and ID.
        /// </summary>
        public FakeOrganizationServiceAssertions HaveDeleted(string entityName, Guid id)
        {
            if (!_service.OperationLog.HasDeleted(entityName, id))
            {
                throw new FakeServiceAssertionException(
                    $"Expected a Delete operation for entity '{entityName}' with ID '{id}', but none was recorded.");
            }

            return this;
        }

        /// <summary>
        /// Asserts that an Execute operation was recorded for the specified request type.
        /// </summary>
        public FakeOrganizationServiceAssertions HaveExecuted<TRequest>() where TRequest : OrganizationRequest
        {
            if (!_service.OperationLog.HasExecuted<TRequest>())
            {
                throw new FakeServiceAssertionException(
                    $"Expected an Execute operation for request type '{typeof(TRequest).Name}', but none was recorded.");
            }

            return this;
        }

        /// <summary>
        /// Asserts that an Execute operation was recorded for the specified request name.
        /// </summary>
        public FakeOrganizationServiceAssertions HaveExecuted(string requestName)
        {
            if (!_service.OperationLog.HasExecuted(requestName))
            {
                throw new FakeServiceAssertionException(
                    $"Expected an Execute operation for request '{requestName}', but none was recorded.");
            }

            return this;
        }

        /// <summary>
        /// Asserts that no Create operation was recorded for the specified entity type.
        /// </summary>
        public FakeOrganizationServiceAssertions NotHaveCreated(string entityName)
        {
            if (_service.OperationLog.HasCreated(entityName))
            {
                throw new FakeServiceAssertionException(
                    $"Expected no Create operation for entity '{entityName}', but one was recorded.");
            }

            return this;
        }

        /// <summary>
        /// Asserts that no Delete operation was recorded for the specified entity type and ID.
        /// </summary>
        public FakeOrganizationServiceAssertions NotHaveDeleted(string entityName, Guid id)
        {
            if (_service.OperationLog.HasDeleted(entityName, id))
            {
                throw new FakeServiceAssertionException(
                    $"Expected no Delete operation for entity '{entityName}' with ID '{id}', but one was recorded.");
            }

            return this;
        }

        /// <summary>
        /// Asserts that no Execute operation was recorded for the specified request type.
        /// </summary>
        public FakeOrganizationServiceAssertions NotHaveExecuted<TRequest>() where TRequest : OrganizationRequest
        {
            if (_service.OperationLog.HasExecuted<TRequest>())
            {
                throw new FakeServiceAssertionException(
                    $"Expected no Execute operation for request type '{typeof(TRequest).Name}', but one was recorded.");
            }

            return this;
        }
    }
}
