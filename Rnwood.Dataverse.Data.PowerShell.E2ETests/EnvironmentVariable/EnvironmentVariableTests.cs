using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.EnvironmentVariable
{
    /// <summary>
    /// Environment variable manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/EnvironmentVariable.Tests.ps1
    /// </summary>
    public class EnvironmentVariableTests : E2ETestBase
    {
        [Fact]
        public void CanPerformAllEnvironmentVariableOperationsComprehensively()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

function Remove-TestEnvironmentVariable {
    param(
        [Parameter(Mandatory)]
        $Connection,
        [Parameter(Mandatory)]
        [string]$DefinitionId,
        [Parameter(Mandatory)]
        [string]$SchemaName
    )
    
    try {
        Get-DataverseRecord -Connection $Connection -TableName environmentvariablevalue -FilterValues @{ environmentvariabledefinitionid = $DefinitionId } -Columns environmentvariablevalueid -ErrorAction SilentlyContinue | ForEach-Object {
            Remove-DataverseRecord -Connection $Connection -TableName environmentvariablevalue -Id $_.Id -Confirm:$false -ErrorAction SilentlyContinue
        }
    } catch { }
    
    try {
        Remove-DataverseEnvironmentVariableDefinition -Connection $Connection -SchemaName $SchemaName -Confirm:$false -ErrorAction SilentlyContinue
    } catch { }
}

try {
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $schemaName = ""new_e2eenvvar_$testRunId""
    
    Write-Host 'Creating environment variable definition...'
    $definition = Set-DataverseEnvironmentVariableDefinition -Connection $connection `
        -SchemaName $schemaName `
        -DisplayName ""E2E Test EnvVar $testRunId"" `
        -Type String `
        -PassThru `
        -Confirm:$false
    
    if (-not $definition) {
        throw 'Failed to create environment variable definition'
    }
    $definitionId = $definition.environmentvariabledefinitionid
    Write-Host ""✓ Environment variable definition created (ID: $definitionId)""
    
    Write-Host 'Setting environment variable value...'
    Set-DataverseEnvironmentVariableValue -Connection $connection `
        -DefinitionSchemaName $schemaName `
        -Value 'TestValue123' `
        -Confirm:$false
    Write-Host '✓ Environment variable value set'
    
    Write-Host 'Cleanup - Removing environment variable...'
    Remove-TestEnvironmentVariable -Connection $connection -DefinitionId $definitionId -SchemaName $schemaName
    Write-Host '✓ Environment variable deleted'
    
    Write-Host 'SUCCESS: All environment variable operations completed'
}
catch {
    Write-Host ""ERROR: $($_ | Out-String)""
    throw
}
");

            var result = RunScript(script, timeoutSeconds: 300);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }
    }
}
