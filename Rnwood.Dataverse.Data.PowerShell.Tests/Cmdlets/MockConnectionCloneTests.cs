using System;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests to verify that mock connections support cloning through the IDataverseConnection interface.
    /// </summary>
    public class MockConnectionCloneTests : TestBase
    {
        [Fact]
        public void MockConnection_Clone_ShouldNotThrowException()
        {
            // Arrange
            var connection = CreateMockConnection("contact");

            // Act & Assert - Should not throw NotImplementedException
            var cloned = DataverseConnectionExtensions.CloneConnection(connection);
            Assert.NotNull(cloned);
            Assert.NotSame(connection, cloned);
        }

        [Fact]
        public void MockConnection_Clone_ShouldReturnFunctionalConnection()
        {
            // Arrange
            var connection = CreateMockConnection("contact");
            
            // Act
            var cloned = DataverseConnectionExtensions.CloneConnection(connection);

            // Assert - Cloned connection should be functional
            Assert.True(cloned.IsReady);
        }

        [Fact]
        public void MockConnection_CloneMultipleTimes_ShouldSucceed()
        {
            // Arrange
            var connection = CreateMockConnection("contact");

            // Act - Clone multiple times
            var clone1 = DataverseConnectionExtensions.CloneConnection(connection);
            var clone2 = DataverseConnectionExtensions.CloneConnection(connection);
            var clone3 = DataverseConnectionExtensions.CloneConnection(connection);

            // Assert
            Assert.NotNull(clone1);
            Assert.NotNull(clone2);
            Assert.NotNull(clone3);
            Assert.NotSame(clone1, clone2);
            Assert.NotSame(clone2, clone3);
            Assert.NotSame(clone1, clone3);
        }
    }
}
