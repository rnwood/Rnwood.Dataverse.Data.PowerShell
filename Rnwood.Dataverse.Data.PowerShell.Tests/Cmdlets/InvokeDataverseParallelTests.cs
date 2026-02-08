using FluentAssertions;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System.Linq;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Invoke-DataverseParallel cmdlet.
    /// Note: Invoke-DataverseParallel creates a RunspacePool that imports modules from the calling session.
    /// In xUnit tests, cmdlets are registered directly (not as modules), so the worker runspaces
    /// don't have access to Set-DataverseConnectionAsDefault which is called in the worker script.
    /// These tests require E2E testing with the full module loaded.
    /// </summary>
    public class InvokeDataverseParallelTests : TestBase
    {
        [Fact(Skip = "Requires E2E - Worker runspaces need Set-DataverseConnectionAsDefault from module, not available in xUnit")]
        public void InvokeDataverseParallel_ProcessesInputObjectsInParallelChunks_ReturnsAllResults()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseParallel", typeof(InvokeDataverseParallelCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseConnectionAsDefault", typeof(SetDataverseConnectionAsDefaultCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Create test input - array of numbers 1..10
            var input = Enumerable.Range(1, 10).Cast<object>().ToArray();

            // Act - Process in parallel with chunk size 3
            // ScriptBlock doubles each input number
            ps.AddScript(@"
                param($connection, $input)
                $input | Invoke-DataverseParallel -Connection $connection -ChunkSize 3 -ScriptBlock {
                    $_ | ForEach-Object { $_ * 2 }
                }
            ")
            .AddParameter("connection", mockConnection)
            .AddParameter("input", input);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(10);

            // Order may vary due to parallelism, so sort before comparing
            var sortedResults = results.Select(r => (int)r.BaseObject).OrderBy(x => x).ToArray();
            sortedResults.Should().Equal(2, 4, 6, 8, 10, 12, 14, 16, 18, 20);
        }

        [Fact(Skip = "Requires E2E - Worker runspaces need Set-DataverseConnectionAsDefault from module, not available in xUnit")]
        public void InvokeDataverseParallel_ClonedConnectionAvailableInScriptBlock_ProcessesSuccessfully()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseParallel", typeof(InvokeDataverseParallelCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseConnectionAsDefault", typeof(SetDataverseConnectionAsDefaultCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            var input = Enumerable.Range(1, 5).Cast<object>().ToArray();

            // Act - Connection is available via PSDefaultParameterValues in script block
            ps.AddScript(@"
                param($connection, $input)
                $input | Invoke-DataverseParallel -Connection $connection -ChunkSize 2 -ScriptBlock {
                    $_ | ForEach-Object { ""success-$_"" }
                }
            ")
            .AddParameter("connection", mockConnection)
            .AddParameter("input", input);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(5);
            results.Should().AllSatisfy(r => r.BaseObject.ToString().Should().StartWith("success-"));
        }

        [Fact(Skip = "Requires E2E - Worker runspaces need Set-DataverseConnectionAsDefault from module, not available in xUnit")]
        public void InvokeDataverseParallel_RespectsChunkSizeParameter_ProcessesInCorrectBatches()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseParallel", typeof(InvokeDataverseParallelCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseConnectionAsDefault", typeof(SetDataverseConnectionAsDefaultCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Create 25 items with chunk size 10 = 3 chunks (10, 10, 5)
            var input = Enumerable.Range(1, 25).Cast<object>().ToArray();

            // Act - Script block receives individual items, not chunks
            ps.AddScript(@"
                param($connection, $input)
                $input | Invoke-DataverseParallel -Connection $connection -ChunkSize 10 -ScriptBlock {
                    $_
                }
            ")
            .AddParameter("connection", mockConnection)
            .AddParameter("input", input);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(25);
        }

        [Fact]
        public void InvokeDataverseParallel_EmptyInput_HandlesGracefully()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseParallel", typeof(InvokeDataverseParallelCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - Empty array
            ps.AddScript(@"
                param($connection)
                @() | Invoke-DataverseParallel -Connection $connection -ChunkSize 5 -ScriptBlock { $_ }
            ")
            .AddParameter("connection", mockConnection);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().BeNullOrEmpty();
        }

        [Fact(Skip = "Requires E2E - Worker runspaces need Set-DataverseConnectionAsDefault from module, not available in xUnit")]
        public void InvokeDataverseParallel_SingleItemInput_ProcessesSuccessfully()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseParallel", typeof(InvokeDataverseParallelCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseConnectionAsDefault", typeof(SetDataverseConnectionAsDefaultCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - Single item
            ps.AddScript(@"
                param($connection)
                @(42) | Invoke-DataverseParallel -Connection $connection -ChunkSize 5 -ScriptBlock {
                    $_ | ForEach-Object { $_ * 2 }
                }
            ")
            .AddParameter("connection", mockConnection);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].BaseObject.Should().Be(84);
        }
    }
}
