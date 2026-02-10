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
<<<<<<< HEAD
        Invoke-WithRetry {
            Get-DataverseRecord -Connection $Connection -TableName environmentvariablevalue -FilterValues @{ environmentvariabledefinitionid = $DefinitionId } -Columns environmentvariablevalueid -ErrorAction SilentlyContinue | ForEach-Object {
                Remove-DataverseRecord -Connection $Connection -TableName environmentvariablevalue -Id $_.Id -Confirm:$false -ErrorAction SilentlyContinue
            }
=======
        Get-DataverseRecord -Connection $Connection -TableName environmentvariablevalue -FilterValues @{ environmentvariabledefinitionid = $DefinitionId } -Columns environmentvariablevalueid -ErrorAction SilentlyContinue | ForEach-Object {
            Remove-DataverseRecord -Connection $Connection -TableName environmentvariablevalue -Id $_.Id -Confirm:$false -ErrorAction SilentlyContinue
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        }
    } catch { }
    
    try {
<<<<<<< HEAD
        Invoke-WithRetry {
            Remove-DataverseEnvironmentVariableDefinition -Connection $Connection -SchemaName $SchemaName -Confirm:$false -ErrorAction SilentlyContinue
        }
=======
        Remove-DataverseEnvironmentVariableDefinition -Connection $Connection -SchemaName $SchemaName -Confirm:$false -ErrorAction SilentlyContinue
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    } catch { }
}

try {
<<<<<<< HEAD
=======
    $connection.EnableAffinityCookie = $true
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $schemaName = ""new_e2eenvvar_$testRunId""
    
    Write-Host 'Creating environment variable definition...'
<<<<<<< HEAD
    $definition = Invoke-WithRetry {
        Set-DataverseEnvironmentVariableDefinition -Connection $connection `
            -SchemaName $schemaName `
            -DisplayName ""E2E Test EnvVar $testRunId"" `
            -Type String `
            -PassThru `
            -Confirm:$false
    }
=======
    $definition = Set-DataverseEnvironmentVariableDefinition -Connection $connection `
        -SchemaName $schemaName `
        -DisplayName ""E2E Test EnvVar $testRunId"" `
        -Type String `
        -PassThru `
        -Confirm:$false
    
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    if (-not $definition) {
        throw 'Failed to create environment variable definition'
    }
    $definitionId = $definition.environmentvariabledefinitionid
    Write-Host ""✓ Environment variable definition created (ID: $definitionId)""
    
    Write-Host 'Setting environment variable value...'
<<<<<<< HEAD
    Invoke-WithRetry {
        Set-DataverseEnvironmentVariableValue -Connection $connection `
            -DefinitionSchemaName $schemaName `
            -Value 'TestValue123' `
            -Confirm:$false
    }
=======
    Set-DataverseEnvironmentVariableValue -Connection $connection `
        -DefinitionSchemaName $schemaName `
        -Value 'TestValue123' `
        -Confirm:$false
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
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

<<<<<<< HEAD
            var result = RunScript(script);
=======
            var result = RunScript(script, timeoutSeconds: 300);
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }
    }
}
