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

            var result = RunScript(script, timeoutSeconds: 60);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }
    }
}
