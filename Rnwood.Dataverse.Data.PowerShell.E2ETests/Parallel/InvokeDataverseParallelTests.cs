using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Parallel
{
    /// <summary>
    /// Tests for Invoke-DataverseParallel functionality.
    /// </summary>
    public class InvokeDataverseParallelTests : E2ETestBase
    {
        [Fact]
        public void CanUseInvokeDataverseParallelWithWhoAmI()
        {


            var script = GetConnectionScript(@"
# Test parallel processing with WhoAmI call
$results = 1..5 | Invoke-DataverseParallel -Connection $connection -ChunkSize 2 -MaxDegreeOfParallelism 3 -ScriptBlock {
    $_ | %{
        $whoami = Get-DataverseWhoAmI -Connection $connection
        # Return a simple object with the item and user ID
        [PSCustomObject]@{
            Item = $_
            UserId = $whoami.UserId
        }
    }
}

# Verify we got 5 results
if ($results.Count -ne 5) {
    throw ""Expected 5 results, got $($results.Count)""
}

# Verify all have a UserId
foreach ($result in $results) {
    if (-not $result.UserId) {
        throw ""Result missing UserId: $($result | ConvertTo-Json)""
    }
}

Write-Host 'Success: Executed 5 parallel WhoAmI calls'
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact(Skip = "Known Issue: ServiceClient.Clone() fails with 'Fault While initializing client' when used with Azure AD client secret authentication in parallel scenarios. Requires investigation - see InvokeDataverseParallelCmdlet.cs")]
        public void InvokeDataverseParallelCanCreateUpdateAndVerifyManyAccounts()
        {


            var script = GetConnectionScript(@"
# Generate unique test prefix to avoid conflicts with parallel CI runs
$testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
$testPrefix = ""ParallelTest-$testRunId-""
$numberOfRecords = 1000
Write-Host ""Using test prefix: $testPrefix""

Write-Host ""Step 1: Creating $numberOfRecords test accounts in parallel...""
$startTime = Get-Date

# Generate accounts to create
$accountsToCreate = 1..$numberOfRecords | ForEach-Object {
    [PSCustomObject]@{
        name = ""$testPrefix$_""
        accountnumber = ""ACCT-$testRunId-$_""
        description = ""Test account created by parallel test - Run $testRunId - Index $_""
    }
}

# Create accounts in parallel batches
$accountsToCreate | Invoke-DataverseParallel -Connection $connection -ChunkSize 100 -MaxDegreeOfParallelism 8 -ScriptBlock {
    $_ | Set-DataverseRecord -TableName account -CreateOnly -BatchSize 100
}

$createDuration = (Get-Date) - $startTime
Write-Host ""Created $numberOfRecords accounts in $($createDuration.TotalSeconds) seconds""

Write-Host ""Step 2: Querying back all created accounts...""
$createdAccounts = Get-DataverseRecord -Connection $connection -TableName account -FilterValues @{ 'name:Like' = ""$testPrefix%"" } -Columns name, accountnumber, description

# Verify count
if ($createdAccounts.Count -ne $numberOfRecords) {
    throw ""Expected $numberOfRecords accounts, but found $($createdAccounts.Count)""
}
Write-Host ""Verified: Found all $numberOfRecords accounts""

# Cleanup
Write-Host ""Step 3: Cleanup - Deleting test accounts...""
$createdAccounts | Invoke-DataverseParallel -Connection $connection -ChunkSize 100 -MaxDegreeOfParallelism 8 -ScriptBlock {
    $_ | Remove-DataverseRecord -TableName account -BatchSize 100
}

Write-Host 'Success: All accounts created, verified, and cleaned up'
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }
    }
}
