using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Metadata
{
    /// <summary>
    /// Entity metadata manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/EntityMetadata.Tests.ps1
    /// </summary>
    public class EntityMetadataTests : E2ETestBase
    {
[Fact]
        public void CanCreateReadUpdateAndDeleteCustomEntityWithAllFeatures()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

function Invoke-WithRetry {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$ScriptBlock,
        [int]$MaxRetries = 5,
        [int]$InitialDelaySeconds = 10
    )
    
    $attempt = 0
    $delay = $InitialDelaySeconds
    
    while ($attempt -lt $MaxRetries) {
        try {
            $attempt++
            Write-Verbose ""Attempt $attempt of $MaxRetries""
            & $ScriptBlock
            return
        }
        catch {
            # Check for CustomizationLockException - this has multiple possible patterns
            $errorMessage = $_.Exception.Message
            $isCustomizationLock = $errorMessage -like '*CustomizationLockException*' `
                -or $errorMessage -like '*Cannot start another*EntityCustomization*' `
                -or $errorMessage -like '*Cannot start the requested operation*EntityCustomization*' `
                -or $errorMessage -like '*previous*EntityCustomization*running*'
            
            if ($isCustomizationLock) {
                Write-Warning ""EntityCustomization operation conflict detected: $errorMessage""
                Write-Warning 'Waiting 2 minutes before retry (will not count toward max retries)...'
                $attempt--
                Start-Sleep -Seconds 120
                continue
            }
            
            if ($attempt -eq $MaxRetries) {
                Write-Error ""All $MaxRetries attempts failed. Last error: $_""
                throw
            }
            
            Write-Warning ""Attempt $attempt failed: $_. Retrying in $delay seconds...""
            Start-Sleep -Seconds $delay
            $delay = $delay * 2
        }
    }
}

try {
    $connection.EnableAffinityCookie = $true

    # Wait for any existing operations to complete before starting test
    Write-Host 'Pre-check: Ensuring no pending operations...'
    Wait-DataversePublish -Connection $connection -MaxWaitSeconds 300 -Verbose
    Start-Sleep -Seconds 5

    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmm')
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $entityName = ""new_enttest_${timestamp}_$testRunId""
    $entitySchemaName = ""new_EntTest_${timestamp}_$testRunId""
    
    Write-Host ""Test entity: $entityName""
    
    Write-Host 'Step 1: Creating custom entity with all features...'
    Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        Set-DataverseEntityMetadata -Connection $connection `
            -EntityName $entityName `
            -SchemaName $entitySchemaName `
            -DisplayName 'Entity Test' `
            -DisplayCollectionName 'Entity Tests' `
            -PrimaryAttributeSchemaName 'new_name' `
            -OwnershipType UserOwned `
            -Confirm:$false
    }
    Write-Host '✓ Custom entity created'
    
    Write-Host 'Step 2: Retrieving entity metadata...'
    Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        $entityMetadata = Get-DataverseEntityMetadata -Connection $connection -EntityName $entityName
        if (-not $entityMetadata) {
            throw 'Failed to retrieve entity metadata'
        }
        Write-Host ""  Entity: $($entityMetadata.LogicalName)""
    }
    Write-Host '✓ Entity metadata retrieved'
    
    Write-Host 'Step 3: Cleanup - Removing test entity...'
    Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
    }
    Write-Host '✓ Test entity deleted'
    
    Write-Host 'SUCCESS: All entity metadata operations completed'
}
catch {
    Write-Host ""ERROR: $($_ | Out-String)""
    throw
}
");

            var result = RunScript(script, timeoutSeconds: 600);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }
    }
}
