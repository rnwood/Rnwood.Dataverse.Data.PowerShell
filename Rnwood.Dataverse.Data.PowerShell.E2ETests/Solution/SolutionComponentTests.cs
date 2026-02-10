using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Solution
{
    /// <summary>
    /// Solution component manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/SolutionComponent.Tests.ps1
    /// </summary>
    public class SolutionComponentTests : E2ETestBase
    {
        [Fact]
        public void CanAddReadUpdateAndManageSolutionComponents()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

# Retry helper function with exponential backoff
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
    $solutionName = ""e2esolcomp_${timestamp}_$testRunId""
    $solutionDisplayName = ""E2E Solution Component Test $testRunId""
    $publisherPrefix = 'e2e'
    $entityName = ""new_e2esolent_${timestamp}_$testRunId""
    $entitySchemaName = ""new_E2ESolEnt_${timestamp}_$testRunId""
    
    Write-Host ""Test solution: $solutionName""
    Write-Host ""Test entity: $entityName""
    
    Write-Host 'Step 1: Creating test solution...'
    Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        
        $existingSolution = Get-DataverseSolution -Connection $connection -UniqueName $solutionName -ErrorAction SilentlyContinue
        if ($existingSolution) {
            Write-Host '  Removing existing test solution...'
            Remove-DataverseSolution -Connection $connection -UniqueName $solutionName -Confirm:$false
        }
        
        $publisher = Get-DataverseRecord -Connection $connection -TableName publisher -FilterValues @{ 'customizationprefix' = $publisherPrefix } | Select-Object -First 1
        if (-not $publisher) {
            Write-Host '  Creating test publisher...'
            $publisher = @{
                'uniquename' = ""e2etestpublisher_$testRunId""
                'friendlyname' = 'E2E Test Publisher'
                'customizationprefix' = $publisherPrefix
            } | Set-DataverseRecord -Connection $connection -TableName publisher -PassThru
        }
        
        Set-DataverseSolution -Connection $connection `
            -UniqueName $solutionName `
            -Name $solutionDisplayName `
            -Version '1.0.0.0' `
            -PublisherUniqueName $publisher.uniquename `
            -Confirm:$false
        
        $solution = Get-DataverseSolution -Connection $connection -UniqueName $solutionName
        $script:solutionId = $solution.Id
    }
    
    Write-Host ""✓ Test solution created (ID: $($script:solutionId))""
    
    Write-Host 'Step 2: Creating test entity...'
    Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        
        Set-DataverseEntityMetadata -Connection $connection `
            -EntityName $entityName `
            -SchemaName $entitySchemaName `
            -DisplayName 'E2E Solution Test Entity' `
            -DisplayCollectionName 'E2E Solution Test Entities' `
            -PrimaryAttributeSchemaName 'new_name' `
            -OwnershipType UserOwned `
            -Confirm:$false
        
        # Entity creation can trigger long-running customization operations
        # Wait longer to ensure the operation completes
        Write-Verbose 'Entity created. Waiting 30 seconds for customization operations to complete...'
        Start-Sleep -Seconds 30
    }
    
    Write-Host '✓ Test entity created'
    
    Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        $entityMetadata = Get-DataverseEntityMetadata -Connection $connection -EntityName $entityName
        $script:entityObjectId = $entityMetadata.MetadataId
    }
    Write-Host ""  Entity ObjectId: $($script:entityObjectId)""
    
    Write-Host 'Step 3: Adding entity to solution with Behavior 0 (Include Subcomponents)...'
    Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        
        $result = Set-DataverseSolutionComponent -Connection $connection `
            -SolutionName $solutionName `
            -ComponentId $script:entityObjectId `
            -ComponentType 1 `
            -Behavior 0 `
            -PassThru `
            -Confirm:$false
        
        if ($result.WasUpdated -eq $true) {
            throw 'Component should not be marked as updated when adding new component'
        }
        if ($result.BehaviorValue -ne 0) {
            throw ""Expected behavior value 0, got $($result.BehaviorValue)""
        }
    }
    Write-Host '✓ Entity added to solution with Behavior 0'
    
    Write-Host 'Step 4: Verifying component exists in solution...'
    Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        $components = Get-DataverseSolutionComponent -Connection $connection -SolutionName $solutionName
        
        $entityComponent = $components | Where-Object { $_.ObjectId -eq $entityName }
        if (-not $entityComponent) {
            throw 'Entity component not found in solution'
        }
        if ($entityComponent.ComponentType -ne 1) {
            throw ""Expected component type 1 (Entity), got $($entityComponent.ComponentType)""
        }
        Write-Host ""  Found component: Type=$($entityComponent.ComponentType), Behavior=$($entityComponent.Behavior)""
        
        if ($entityComponent.Behavior -ne 'IncludeSubcomponents') {
            throw ""Expected behavior 'IncludeSubcomponents' for Behavior 0, got '$($entityComponent.Behavior)'""
        }
    }
    Write-Host '✓ Component verified in solution with Behavior 0 (IncludeSubcomponents)'
    
    Write-Host 'Step 5: Cleanup - Removing test solution...'
    Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        Remove-DataverseSolution -Connection $connection -UniqueName $solutionName -Confirm:$false
    }
    Write-Host '✓ Test solution deleted'
    
    Write-Host 'Step 6: Cleanup - Removing test entity...'
    Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
    }
    Write-Host '✓ Test entity deleted'
    
    Write-Host 'SUCCESS: All solution component operations completed successfully'
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
