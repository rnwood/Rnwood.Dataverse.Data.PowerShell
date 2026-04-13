using System;
using Microsoft.Xrm.Sdk;
using Shouldly;

namespace Fake4Dataverse.Shouldly
{
    /// <summary>
    /// Shouldly-style assertion helpers for <see cref="FakeOrganizationService"/> operation log verification.
    /// </summary>
    public static class FakeOrganizationServiceShouldlyExtensions
    {
        /// <summary>
        /// Asserts that a Create operation was recorded for the specified entity type.
        /// </summary>
        public static FakeOrganizationService ShouldHaveCreated(this FakeOrganizationService service, string entityName)
        {
            EnsureService(service);
            service.OperationLog.HasCreated(entityName)
                .ShouldBeTrue($"Expected a Create of '{entityName}' to be recorded.");
            return service;
        }

        /// <summary>
        /// Asserts that a Create operation was recorded for the specified entity type and ID.
        /// </summary>
        public static FakeOrganizationService ShouldHaveCreated(this FakeOrganizationService service, string entityName, Guid id)
        {
            EnsureService(service);
            service.OperationLog.HasCreated(entityName, id)
                .ShouldBeTrue($"Expected a Create of '{entityName}' with ID '{id}' to be recorded.");
            return service;
        }

        /// <summary>
        /// Asserts that an Update operation was recorded for the specified entity type and ID.
        /// </summary>
        public static FakeOrganizationService ShouldHaveUpdated(this FakeOrganizationService service, string entityName, Guid id)
        {
            EnsureService(service);
            service.OperationLog.HasUpdated(entityName, id)
                .ShouldBeTrue($"Expected an Update of '{entityName}' with ID '{id}' to be recorded.");
            return service;
        }

        /// <summary>
        /// Asserts that a Delete operation was recorded for the specified entity type and ID.
        /// </summary>
        public static FakeOrganizationService ShouldHaveDeleted(this FakeOrganizationService service, string entityName, Guid id)
        {
            EnsureService(service);
            service.OperationLog.HasDeleted(entityName, id)
                .ShouldBeTrue($"Expected a Delete of '{entityName}' with ID '{id}' to be recorded.");
            return service;
        }

        /// <summary>
        /// Asserts that an Execute operation was recorded for the specified request type.
        /// </summary>
        public static FakeOrganizationService ShouldHaveExecuted<TRequest>(this FakeOrganizationService service)
            where TRequest : OrganizationRequest
        {
            EnsureService(service);
            service.OperationLog.HasExecuted<TRequest>()
                .ShouldBeTrue($"Expected an Execute of '{typeof(TRequest).Name}' to be recorded.");
            return service;
        }

        /// <summary>
        /// Asserts that an Execute operation was recorded for the specified request name.
        /// </summary>
        public static FakeOrganizationService ShouldHaveExecuted(this FakeOrganizationService service, string requestName)
        {
            EnsureService(service);
            service.OperationLog.HasExecuted(requestName)
                .ShouldBeTrue($"Expected an Execute of '{requestName}' to be recorded.");
            return service;
        }

        /// <summary>
        /// Asserts that no Create operation was recorded for the specified entity type.
        /// </summary>
        public static FakeOrganizationService ShouldNotHaveCreated(this FakeOrganizationService service, string entityName)
        {
            EnsureService(service);
            service.OperationLog.HasCreated(entityName)
                .ShouldBeFalse($"Expected no Create of '{entityName}' to be recorded.");
            return service;
        }

        /// <summary>
        /// Asserts that no Delete operation was recorded for the specified entity type and ID.
        /// </summary>
        public static FakeOrganizationService ShouldNotHaveDeleted(this FakeOrganizationService service, string entityName, Guid id)
        {
            EnsureService(service);
            service.OperationLog.HasDeleted(entityName, id)
                .ShouldBeFalse($"Expected no Delete of '{entityName}' with ID '{id}' to be recorded.");
            return service;
        }

        /// <summary>
        /// Asserts that no Execute operation was recorded for the specified request type.
        /// </summary>
        public static FakeOrganizationService ShouldNotHaveExecuted<TRequest>(this FakeOrganizationService service)
            where TRequest : OrganizationRequest
        {
            EnsureService(service);
            service.OperationLog.HasExecuted<TRequest>()
                .ShouldBeFalse($"Expected no Execute of '{typeof(TRequest).Name}' to be recorded.");
            return service;
        }

        private static void EnsureService(FakeOrganizationService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }
        }
    }
}
