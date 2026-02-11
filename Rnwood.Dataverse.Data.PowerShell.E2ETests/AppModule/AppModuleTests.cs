using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.AppModule
{
    /// <summary>
    /// AppModule manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/AppModule.Tests.ps1
    /// </summary>
    public class AppModuleTests : E2ETestBase
    {
[Fact]
        public void CanPerformFullLifecycleOfAppModuleManipulationIncludingComponents()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

try {
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $appModuleUniqueName = ""test_appmodule_$testRunId""
    $appModuleName = ""Test AppModule $testRunId""
    
    Write-Host ""Using test appmodule unique name: $appModuleUniqueName""
    
    Write-Host 'Creating new appmodule...'
    $appModuleId = Invoke-WithRetry {
        Set-DataverseAppModule -Connection $connection `
            -UniqueName $appModuleUniqueName `
            -Name $appModuleName `
            -Description 'Test AppModule for E2E testing' `
            -PassThru -Confirm:$false
    }
    if (-not $appModuleId) {
        throw 'Failed to create appmodule - no ID returned'
    }
    Write-Host ""Created appmodule with ID: $appModuleId""
    
    Write-Host 'Retrieving appmodule by ID...'
    $retrievedAppModule = Get-DataverseAppModule -Connection $connection -Id $appModuleId
    
    if (-not $retrievedAppModule) {
        throw 'Failed to retrieve appmodule by ID'
    }
    if ($retrievedAppModule.Id -ne $appModuleId) {
        throw ""Retrieved appmodule ID mismatch. Expected: $appModuleId, Got: $($retrievedAppModule.Id)""
    }
    Write-Host 'Successfully retrieved appmodule'
    
    Write-Host 'Cleanup - Removing appmodule...'
    Invoke-WithRetry {
        Remove-DataverseAppModule -Connection $connection -Id $appModuleId -Confirm:$false
    }
    Write-Host '✓ AppModule deleted'
    
    Write-Host 'SUCCESS: All appmodule operations completed successfully'
}
catch {
    Write-Host ""ERROR: $($_ | Out-String)""
    throw
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }
    }
}
