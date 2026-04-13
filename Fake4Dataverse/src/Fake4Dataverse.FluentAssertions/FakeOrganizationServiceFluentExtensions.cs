using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.FluentAssertions
{
    /// <summary>
    /// FluentAssertions extensions for <see cref="FakeOrganizationService"/>.
    /// </summary>
    public static class FakeOrganizationServiceFluentExtensions
    {
        /// <summary>
        /// Returns a <see cref="FakeOrganizationServiceFluentAssertions"/> object for the service.
        /// </summary>
        public static FakeOrganizationServiceFluentAssertions Should(this FakeOrganizationService service)
        {
            return new FakeOrganizationServiceFluentAssertions(service);
        }
    }

    /// <summary>
    /// FluentAssertions-based assertion class for <see cref="FakeOrganizationService"/>.
    /// </summary>
    public sealed class FakeOrganizationServiceFluentAssertions
        : ReferenceTypeAssertions<FakeOrganizationService, FakeOrganizationServiceFluentAssertions>
    {
        /// <inheritdoc />
        protected override string Identifier => "FakeOrganizationService";

        /// <summary>
        /// Initializes a new instance of <see cref="FakeOrganizationServiceFluentAssertions"/>.
        /// </summary>
        public FakeOrganizationServiceFluentAssertions(FakeOrganizationService subject)
            : base(subject)
        {
        }

        /// <summary>
        /// Asserts that a Create operation was recorded for the specified entity type and ID.
        /// </summary>
        public AndConstraint<FakeOrganizationServiceFluentAssertions> HaveCreated(
            string entityName, Guid id, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(Subject.OperationLog.HasCreated(entityName, id))
                .FailWith("Expected a Create of {0} with ID {1}{reason}, but it was not recorded.", entityName, id);

            return new AndConstraint<FakeOrganizationServiceFluentAssertions>(this);
        }

        /// <summary>
        /// Asserts that an Update operation was recorded for the specified entity type and ID.
        /// </summary>
        public AndConstraint<FakeOrganizationServiceFluentAssertions> HaveUpdated(
            string entityName, Guid id, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(Subject.OperationLog.HasUpdated(entityName, id))
                .FailWith("Expected an Update of {0} with ID {1}{reason}, but it was not recorded.", entityName, id);

            return new AndConstraint<FakeOrganizationServiceFluentAssertions>(this);
        }

        /// <summary>
        /// Asserts that a Delete operation was recorded for the specified entity type and ID.
        /// </summary>
        public AndConstraint<FakeOrganizationServiceFluentAssertions> HaveDeleted(
            string entityName, Guid id, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(Subject.OperationLog.HasDeleted(entityName, id))
                .FailWith("Expected a Delete of {0} with ID {1}{reason}, but it was not recorded.", entityName, id);

            return new AndConstraint<FakeOrganizationServiceFluentAssertions>(this);
        }

        /// <summary>
        /// Asserts that an Execute operation was recorded for the specified request type.
        /// </summary>
        public AndConstraint<FakeOrganizationServiceFluentAssertions> HaveExecuted<TRequest>(
            string because = "", params object[] becauseArgs) where TRequest : OrganizationRequest
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(Subject.OperationLog.HasExecuted<TRequest>())
                .FailWith("Expected an Execute of {0}{reason}, but it was not recorded.", typeof(TRequest).Name);

            return new AndConstraint<FakeOrganizationServiceFluentAssertions>(this);
        }
    }
}
