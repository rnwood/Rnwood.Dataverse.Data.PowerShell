using System;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk;

namespace Fake4Dataverse.AwesomeAssertions
{
    /// <summary>
    /// AwesomeAssertions extensions for <see cref="FakeOrganizationService"/>.
    /// </summary>
    public static class FakeOrganizationServiceAwesomeAssertionExtensions
    {
        /// <summary>
        /// Returns a <see cref="FakeOrganizationServiceAwesomeAssertions"/> object for the service.
        /// </summary>
        public static FakeOrganizationServiceAwesomeAssertions Should(this FakeOrganizationService service)
        {
            return new FakeOrganizationServiceAwesomeAssertions(service);
        }
    }

    /// <summary>
    /// AwesomeAssertions-based assertion class for <see cref="FakeOrganizationService"/>.
    /// </summary>
    public sealed class FakeOrganizationServiceAwesomeAssertions
    {
        private readonly FakeOrganizationService _subject;

        /// <summary>
        /// Initializes a new instance of <see cref="FakeOrganizationServiceAwesomeAssertions"/>.
        /// </summary>
        public FakeOrganizationServiceAwesomeAssertions(FakeOrganizationService subject)
        {
            _subject = subject ?? throw new ArgumentNullException(nameof(subject));
        }

        /// <summary>
        /// Asserts that a Create operation was recorded for the specified entity type and ID.
        /// </summary>
        public AndConstraint<FakeOrganizationServiceAwesomeAssertions> HaveCreated(
            string entityName, Guid id, string because = "", params object[] becauseArgs)
        {
            _subject.OperationLog.HasCreated(entityName, id)
                .Should().BeTrue(because, becauseArgs);

            return new AndConstraint<FakeOrganizationServiceAwesomeAssertions>(this);
        }

        /// <summary>
        /// Asserts that an Update operation was recorded for the specified entity type and ID.
        /// </summary>
        public AndConstraint<FakeOrganizationServiceAwesomeAssertions> HaveUpdated(
            string entityName, Guid id, string because = "", params object[] becauseArgs)
        {
            _subject.OperationLog.HasUpdated(entityName, id)
                .Should().BeTrue(because, becauseArgs);

            return new AndConstraint<FakeOrganizationServiceAwesomeAssertions>(this);
        }

        /// <summary>
        /// Asserts that a Delete operation was recorded for the specified entity type and ID.
        /// </summary>
        public AndConstraint<FakeOrganizationServiceAwesomeAssertions> HaveDeleted(
            string entityName, Guid id, string because = "", params object[] becauseArgs)
        {
            _subject.OperationLog.HasDeleted(entityName, id)
                .Should().BeTrue(because, becauseArgs);

            return new AndConstraint<FakeOrganizationServiceAwesomeAssertions>(this);
        }

        /// <summary>
        /// Asserts that an Execute operation was recorded for the specified request type.
        /// </summary>
        public AndConstraint<FakeOrganizationServiceAwesomeAssertions> HaveExecuted<TRequest>(
            string because = "", params object[] becauseArgs) where TRequest : OrganizationRequest
        {
            _subject.OperationLog.HasExecuted<TRequest>()
                .Should().BeTrue(because, becauseArgs);

            return new AndConstraint<FakeOrganizationServiceAwesomeAssertions>(this);
        }
    }
}
