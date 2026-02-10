using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.ErrorHandling
{
    /// <summary>
    /// Tests for error message handling and validation.
    /// </summary>
    public class ErrorMessageTests : E2ETestBase
    {
        [Fact]
        public void ProvidesErrorMessageForNonReadableColumns()
        {


            var script = GetConnectionScript(@"
Write-Host 'Testing non-readable column error handling with organization entity...'

# The organization entity has 'telemetryinstrumentationkey' column that is updatable but not readable
$org = Get-DataverseRecord -Connection $connection -TableName organization -Columns organizationid -Top 1

if (-not $org) {
    throw 'Failed to retrieve organization record'
}

$orgId = $org.organizationid
Write-Host ""Using organization record: $orgId""

# Try to update with a non-readable column (should fail with clear error message)
$updateData = @{
    telemetryinstrumentationkey = 'test-instrumentation-key-value'
}

$errorReceived = $false
$errorMessage = ''

try {
    Set-DataverseRecord -Connection $connection -TableName organization -Id $orgId -InputObject $updateData -ErrorAction Stop
    throw 'Update with non-readable column should have failed'
} catch {
    $errorReceived = $true
    $errorMessage = $_.Exception.Message
    Write-Host ""Received expected error: $errorMessage""
}

if (-not $errorReceived) {
    throw 'Did not receive expected error for non-readable column'
}

# Verify error message contains helpful information
$missingElements = @()
if ($errorMessage -notmatch 'not valid for read') {
    $missingElements += 'missing not valid for read explanation'
}
if ($errorMessage -notmatch 'telemetryinstrumentationkey') {
    $missingElements += 'missing telemetryinstrumentationkey column name'
}
if ($errorMessage -notmatch 'organization') {
    $missingElements += 'missing organization entity name'
}
if ($errorMessage -notmatch '-UpdateAllColumns') {
    $missingElements += 'missing -UpdateAllColumns alternative'
}

if ($missingElements.Count -gt 0) {
    throw ""Error message missing helpful information: $($missingElements -join ', ')""
}

Write-Host 'Success: Error message verification passed'
");

            var result = RunScript(script, timeoutSeconds: 60);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }
    }
}
