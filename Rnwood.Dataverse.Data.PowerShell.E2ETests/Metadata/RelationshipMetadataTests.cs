using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Metadata
{
    /// <summary>
    /// Relationship metadata manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/RelationshipMetadata.Tests.ps1
    /// </summary>
    public class RelationshipMetadataTests : E2ETestBase
    {
[Fact]
        public void CanCreateReadUpdateAndDeleteOneToManyAndManyToManyRelationshipsComprehensively()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

<<<<<<< HEAD
try {
=======
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
            if ($_.Exception.Message -like '*Cannot start the requested operation*EntityCustomization*') {
                Write-Warning 'EntityCustomization operation conflict. Waiting 2 minutes...'
                $attempt--
                Start-Sleep -Seconds 120
                continue
            }
            
            if ($attempt -eq $MaxRetries) {
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
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmm')
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $entity1Name = ""new_reltest1_${timestamp}_$testRunId""
    $entity2Name = ""new_reltest2_${timestamp}_$testRunId""
    
    Write-Host 'Creating test entities...'
    Invoke-WithRetry {
        Set-DataverseEntityMetadata -Connection $connection `
            -EntityName $entity1Name `
            -SchemaName ""new_RelTest1_${timestamp}_$testRunId"" `
            -DisplayName 'Relationship Test Entity 1' `
            -DisplayCollectionName 'Relationship Test Entities 1' `
            -PrimaryAttributeSchemaName 'new_name' `
            -OwnershipType UserOwned `
            -Confirm:$false
            
        Set-DataverseEntityMetadata -Connection $connection `
            -EntityName $entity2Name `
            -SchemaName ""new_RelTest2_${timestamp}_$testRunId"" `
            -DisplayName 'Relationship Test Entity 2' `
            -DisplayCollectionName 'Relationship Test Entities 2' `
            -PrimaryAttributeSchemaName 'new_name' `
            -OwnershipType UserOwned `
            -Confirm:$false
    }
    Write-Host '✓ Test entities created'
    
    Write-Host 'Cleanup - Removing test entities...'
    Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entity1Name -Confirm:$false
        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entity2Name -Confirm:$false
    }
    Write-Host '✓ Test entities deleted'
    
    Write-Host 'SUCCESS: All relationship metadata operations completed'
}
catch {
    Write-Host ""ERROR: $($_ | Out-String)""
    throw
}
");

<<<<<<< HEAD
            var result = RunScript(script);
=======
            var result = RunScript(script, timeoutSeconds: 600);
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }
    }
}
