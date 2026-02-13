using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using FluentAssertions;
using Xunit;
using System;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.RecordOperations
{
    /// <summary>
    /// E2E tests for basic Set-DataverseRecord functionality.
    /// </summary>
    public class SetDataverseRecordBasicTests : E2ETestBase
    {
        [Fact]
        public void SetDataverseRecord_ValuesAliasWithHashtable_Should_CreateRecord()
        {
            var testScript = GetConnectionScript($@"
                # Test using -Values alias with hashtable
                $testName = 'E2ETest_{Guid.NewGuid().ToString("N").Substring(0, 8)}'
                
                # Create a record using -Values alias
                $result = Set-DataverseRecord -Connection $connection `
                    -TableName account `
                    -Values @{{ 
                        name = $testName
                        description = 'Created via -Values alias'
                    }} `
                    -CreateOnly `
                    -PassThru
                
                # Verify the record was created
                if (-not $result.Id) {{
                    throw 'Record was not created - no Id returned'
                }}
                
                # Retrieve the record to confirm it exists
                $retrieved = Get-DataverseRecord -Connection $connection `
                    -TableName account `
                    -Id $result.Id `
                    -Columns @('accountid', 'name', 'description')
                
                if ($retrieved.name -ne $testName) {{
                    throw ""Name mismatch. Expected: $testName, Got: $($retrieved.name)""
                }}
                
                if ($retrieved.description -ne 'Created via -Values alias') {{
                    throw ""Description mismatch. Expected: 'Created via -Values alias', Got: $($retrieved.description)""
                }}
                
                # Clean up
                Remove-DataverseRecord -Connection $connection -TableName account -Id $result.Id
                
                Write-Host 'SUCCESS: Record created and verified using -Values alias' -ForegroundColor Green
            ");

            var result = RunScript(testScript);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }

        [Fact]
        public void SetDataverseRecord_ValuesAliasInLoop_Should_CreateMultipleRecords()
        {
            var testScript = GetConnectionScript($@"
                # Test the original issue scenario - using -Values in a loop
                $testPrefix = 'E2ETest_{Guid.NewGuid().ToString("N").Substring(0, 8)}'
                $testNames = @('Alpha', 'Beta', 'Gamma', 'Delta')
                $createdIds = @()
                
                foreach ($name in $testNames) {{
                    $result = Set-DataverseRecord -Connection $connection `
                        -TableName account `
                        -Values @{{ name = ""$testPrefix-$name"" }} `
                        -CreateOnly `
                        -PassThru | Out-Null
                    
                    if (-not $result) {{
                        throw ""Failed to create record for $name""
                    }}
                    
                    $createdIds += $result.Id
                }}
                
                Write-Host ""Created $($createdIds.Count) records"" -ForegroundColor Green
                
                # Verify all records were created
                if ($createdIds.Count -ne $testNames.Count) {{
                    throw ""Expected $($testNames.Count) records, got $($createdIds.Count)""
                }}
                
                # Clean up
                foreach ($id in $createdIds) {{
                    Remove-DataverseRecord -Connection $connection -TableName account -Id $id
                }}
                
                Write-Host 'SUCCESS: Multiple records created using -Values alias in loop' -ForegroundColor Green
            ");

            var result = RunScript(testScript);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("SUCCESS");
            result.StandardOutput.Should().Contain("Created 4 records");
        }
    }
}
