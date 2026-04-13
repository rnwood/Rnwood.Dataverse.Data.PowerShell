namespace Fake4Dataverse
{
    /// <summary>
    /// Extension methods for fluent assertions on <see cref="FakeOrganizationService"/>.
    /// </summary>
    public static class FakeOrganizationServiceAssertionExtensions
    {
        /// <summary>
        /// Returns a fluent assertion object for verifying operations performed against
        /// the <see cref="FakeOrganizationService"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// service.Should().HaveCreated("account", id);
        /// service.Should().HaveUpdated("account", id);
        /// service.Should().HaveDeleted("account", id);
        /// service.Should().HaveExecuted&lt;WhoAmIRequest&gt;();
        /// </code>
        /// </example>
        public static FakeOrganizationServiceAssertions Should(this FakeOrganizationService service)
        {
            return new FakeOrganizationServiceAssertions(service);
        }
    }
}
