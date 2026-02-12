using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Module
{
    /// <summary>
    /// Tests for basic Dataverse connection functionality.
    /// </summary>
    [CrossPlatformTest]
    public class ConnectionTests : E2ETestBase
    {
        [Fact]
        public void CanConnectToRealEnvAndQueryData()
        {


            var script = GetConnectionScript(@"
Get-DataverseRecord -Connection $connection -TableName systemuser | Out-Null
Write-Host 'Success'
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void DisableAffinityCookie_Should_SetPropertyToFalse()
        {
            var script = GetConnectionScript(@"
# Connection should have EnableAffinityCookie = false when -DisableAffinityCookie is specified
Write-Host ""EnableAffinityCookie: $($connection.EnableAffinityCookie)""
", useDisableAffinityCookie: true);

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("EnableAffinityCookie: False");
        }

        [Fact]
        public void EnableAffinityCookie_Should_BeTrue_ByDefault()
        {
            var script = GetConnectionScript(@"
# Connection should have EnableAffinityCookie = true by default
Write-Host ""EnableAffinityCookie: $($connection.EnableAffinityCookie)""
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("EnableAffinityCookie: True");
        }

        [Fact]
        public void ParallelizationWithAffinityCookie_Should_EmitWarning()
        {
            var script = GetConnectionScript(@"
# Using parallelization without DisableAffinityCookie should emit a warning
Get-DataverseRecord -Connection $connection -TableName systemuser -Top 10 | 
    Set-DataverseRecord -Connection $connection -TableName systemuser -MaxDegreeOfParallelism 2 -WhatIf
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardError.Should().Contain("Using parallelization with affinity cookie enabled may reduce performance");
        }

        [Fact]
        public void ParallelizationWithDisableAffinityCookie_Should_NotEmitWarning()
        {
            var script = GetConnectionScript(@"
# Using parallelization with DisableAffinityCookie should NOT emit a warning
Get-DataverseRecord -Connection $connection -TableName systemuser -Top 10 | 
    Set-DataverseRecord -Connection $connection -TableName systemuser -MaxDegreeOfParallelism 2 -WhatIf
", useDisableAffinityCookie: true);

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardError.Should().NotContain("Using parallelization with affinity cookie enabled may reduce performance");
        }
    }
}
