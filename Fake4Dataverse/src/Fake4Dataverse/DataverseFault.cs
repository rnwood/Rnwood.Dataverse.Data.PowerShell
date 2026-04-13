using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse
{
    /// <summary>
    /// Provides Dataverse-compatible error codes and helpers to create
    /// <see cref="FaultException{OrganizationServiceFault}"/> instances.
    /// </summary>
    public static class DataverseFault
    {
        /// <summary>Entity with the specified ID does not exist (0x80040217).</summary>
        public const int ObjectDoesNotExist = unchecked((int)0x80040217);
        /// <summary>A record with matching key values already exists (0x80040237).</summary>
        public const int DuplicateRecord = unchecked((int)0x80040237);
        /// <summary>Invalid argument (0x80040203).</summary>
        public const int InvalidArgument = unchecked((int)0x80040203);
        /// <summary>Unspecified error (0x80040216).</summary>
        public const int Unspecified = unchecked((int)0x80040216);
        /// <summary>Concurrency version mismatch (0x80060882).</summary>
        public const int ConcurrencyVersionMismatch = unchecked((int)0x80060882);
        /// <summary>Concurrency version not provided (0x80060883).</summary>
        public const int ConcurrencyVersionNotProvided = unchecked((int)0x80060883);
        /// <summary>Optimistic concurrency not enabled (0x80060893).</summary>
        public const int OptimisticConcurrencyNotEnabled = unchecked((int)0x80060893);

        /// <summary>Creates a fault for an entity not found by ID.</summary>
        public static FaultException<OrganizationServiceFault> EntityNotFound(string entityName, Guid id)
        {
            return Create(ObjectDoesNotExist, $"{entityName} With Id = {id:D} Does Not Exist");
        }

        /// <summary>Creates a fault for a duplicate record ID.</summary>
        public static FaultException<OrganizationServiceFault> DuplicateId(string entityName, Guid id)
        {
            return Create(DuplicateRecord, $"A record with matching key values already exists. Entity: {entityName}, Id: {id:D}");
        }

        /// <summary>Creates a fault for an invalid argument.</summary>
        public static FaultException<OrganizationServiceFault> InvalidArgumentFault(string message)
        {
            return Create(InvalidArgument, message);
        }

        /// <summary>Creates a fault for a concurrency version mismatch.</summary>
        public static FaultException<OrganizationServiceFault> ConcurrencyVersionMismatchFault(string entityName, Guid id)
        {
            return Create(ConcurrencyVersionMismatch,
                $"The version of the existing record of entity '{entityName}' with id '{id:D}' does not match the RowVersion property provided.");
        }

        /// <summary>Creates a fault for a missing concurrency version.</summary>
        public static FaultException<OrganizationServiceFault> ConcurrencyVersionNotProvidedFault()
        {
            return Create(ConcurrencyVersionNotProvided,
                "The RowVersion property must be provided when the request property ConcurrencyBehavior is set to IfRowVersionMatches.");
        }

        /// <summary>Creates a <see cref="FaultException{OrganizationServiceFault}"/> with the specified error code and message.</summary>
        public static FaultException<OrganizationServiceFault> Create(int errorCode, string message)
        {
            var fault = new OrganizationServiceFault
            {
                ErrorCode = errorCode,
                Message = message
            };
            return new FaultException<OrganizationServiceFault>(fault, new FaultReason(message));
        }
    }
}
