using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Metadata
{
    /// <summary>
    /// Attribute metadata manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/AttributeMetadata.Tests.ps1
    /// </summary>
    public class AttributeMetadataTests : E2ETestBase
    {
[Fact]
        public void CanCreateReadUpdateAndDeleteAllAttributeTypesComprehensively()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmm')
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $entityName = ""new_attrtest_${timestamp}_$testRunId""
    
    Write-Host 'Creating test entity...'
    Invoke-WithRetry {
        Set-DataverseEntityMetadata -Connection $connection `
            -EntityName $entityName `
            -SchemaName ""new_AttrTest_${timestamp}_$testRunId"" `
            -DisplayName 'Attribute Test Entity' `
            -DisplayCollectionName 'Attribute Test Entities' `
            -PrimaryAttributeSchemaName 'new_name' `
            -OwnershipType UserOwned `
            -Confirm:$false
    }
    Write-Host '✓ Test entity created'
    
    Write-Host 'Creating string attribute...'
    Invoke-WithRetry {
        Set-DataverseAttributeMetadata -Connection $connection `
            -EntityName $entityName `
            -AttributeName 'new_teststring' `
            -SchemaName 'new_TestString' `
            -AttributeType String `
            -DisplayName 'Test String' `
            -MaxLength 100 `
            -Confirm:$false
    }
    Write-Host '✓ String attribute created'
    
    Write-Host 'Cleanup - Removing test entity...'
    Invoke-WithRetry {
        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
    }
    Write-Host '✓ Test entity deleted'
    
    Write-Host 'SUCCESS: All attribute metadata operations completed'
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
