using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Plugin
{
    /// <summary>
    /// Tests for plugin management cmdlets.
    /// Converted from e2e-tests/PluginManagement.Tests.ps1
    /// </summary>
    public class PluginManagementTests : E2ETestBase
    {
        [Fact]
        public void CanQueryPluginAssembliesFromRealEnvironment()
        {


            var script = GetConnectionScript(@"
$assemblies = Get-DataversePluginAssembly -Connection $connection

Write-Host ""Found $($assemblies.Count) plugin assemblies""

if ($null -eq $assemblies) {
    throw 'Expected to get plugin assemblies, but got null'
}

if ($assemblies.Count -gt 0) {
    $firstAssembly = $assemblies[0]
    if (-not $firstAssembly.name) {
        throw ""Assembly missing 'name' property""
    }
    if (-not $firstAssembly.Id) {
        throw ""Assembly missing 'Id' property""
    }
    Write-Host ""Successfully retrieved plugin assembly with name: $($firstAssembly.name)""
}

Write-Host 'Plugin assembly query test passed'
Write-Host 'Success'
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void CanQueryPluginTypesFromRealEnvironment()
        {


            var script = GetConnectionScript(@"
$types = Get-DataversePluginType -Connection $connection

Write-Host ""Found $($types.Count) plugin types""

if ($null -eq $types) {
    throw 'Expected to get plugin types, but got null'
}

if ($types.Count -gt 0) {
    $firstType = $types[0]
    if (-not $firstType.typename) {
        throw ""Plugin type missing 'typename' property""
    }
    if (-not $firstType.Id) {
        throw ""Plugin type missing 'Id' property""
    }
    Write-Host ""Successfully retrieved plugin type: $($firstType.typename)""
}

Write-Host 'Plugin type query test passed'
Write-Host 'Success'
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void CanQueryPluginStepsFromRealEnvironment()
        {


            var script = GetConnectionScript(@"
# First get a plugin type to filter by, to avoid retrieving all plugin steps
$pluginType = Get-DataversePluginType -Connection $connection | Select-Object -First 1

if ($null -eq $pluginType) {
    Write-Host 'No plugin types found in environment, skipping step query'
    Write-Host 'Success'
    exit 0
}

Write-Host ""Querying steps for plugin type: $($pluginType.plugintypeid)""

$steps = Get-DataversePluginStep -Connection $connection -PluginTypeId $pluginType.plugintypeid

Write-Host ""Found $($steps.Count) plugin steps for this type""

if ($null -eq $steps) {
    # It's OK if this specific plugin type has no steps
    Write-Host 'Plugin type has no steps, but query succeeded'
} elseif ($steps.Count -gt 0) {
    $firstStep = $steps[0]
    if (-not $firstStep.name) {
        throw ""Plugin step missing 'name' property""
    }
    if (-not $firstStep.Id) {
        throw ""Plugin step missing 'Id' property""
    }
    Write-Host ""Successfully retrieved plugin step: $($firstStep.name)""
}

Write-Host 'Plugin step query test passed'
Write-Host 'Success'
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void EnumParametersWorkCorrectlyWithTabCompletionSupport()
        {


            var script = GetConnectionScript(@"
# Verify that the enum types exist and can be used
$isolationModeType = [Rnwood.Dataverse.Data.PowerShell.Commands.PluginAssemblyIsolationMode]
$sourceTypeType = [Rnwood.Dataverse.Data.PowerShell.Commands.PluginAssemblySourceType]
$stageType = [Rnwood.Dataverse.Data.PowerShell.Commands.PluginStepStage]
$modeType = [Rnwood.Dataverse.Data.PowerShell.Commands.PluginStepMode]
$imageTypeType = [Rnwood.Dataverse.Data.PowerShell.Commands.PluginStepImageType]
$deploymentType = [Rnwood.Dataverse.Data.PowerShell.Commands.PluginStepDeployment]

# Verify enum values
if ([int]$isolationModeType::Sandbox -ne 2) {
    throw ""Expected Sandbox isolation mode to be 2, got $([int]$isolationModeType::Sandbox)""
}

if ([int]$stageType::PreOperation -ne 20) {
    throw ""Expected PreOperation stage to be 20, got $([int]$stageType::PreOperation)""
}

if ([int]$modeType::Synchronous -ne 0) {
    throw ""Expected Synchronous mode to be 0, got $([int]$modeType::Synchronous)""
}

if ([int]$imageTypeType::PreImage -ne 0) {
    throw ""Expected PreImage type to be 0, got $([int]$imageTypeType::PreImage)""
}

Write-Host 'All enum types and values are correct'
Write-Host 'Success'
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }
    }
}
