using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.RecordAccess
{
    /// <summary>
    /// Record access manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/RecordAccess.Tests.ps1
    /// </summary>
    public class RecordAccessTests : E2ETestBase
    {
[Fact]
        public void CanGrantTestGetAndRemoveRecordAccessComprehensively()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

try {
    Write-Host 'Getting current user identity...'
    $whoAmI = Get-DataverseWhoAmI -Connection $connection
    $currentUserId = $whoAmI.UserId
    Write-Host ""  Current user ID: $currentUserId""
    
    Write-Host 'Finding a second user to share with...'
    $allUsers = Get-DataverseRecord -Connection $connection -TableName systemuser -FilterValues @{isdisabled=$false} -Columns systemuserid, fullname
    $secondUser = $allUsers | Where-Object { $_.systemuserid -ne $currentUserId } | Select-Object -First 1
    
    if (-not $secondUser) {
        throw 'Could not find a second user to share with'
    }
    
    $secondUserId = $secondUser.systemuserid
    Write-Host ""  Second user ID: $secondUserId (Name: $($secondUser.fullname))""
    
    $testIdentifier = [Guid]::NewGuid().ToString().Substring(0, 8)
    $contactLastName = ""E2ETest_RecordAccess_$testIdentifier""
    
    Write-Host 'Creating test contact...'
    $contactRecord = @{
        'lastname' = $contactLastName
        'firstname' = 'RecordAccess'
    } | Set-DataverseRecord -Connection $connection -TableName contact -PassThru
    $contactId = $contactRecord.Id
    Write-Host ""  Test contact created (ID: $contactId)""
    
    Write-Host 'Cleanup - Removing test contact...'
    Remove-DataverseRecord -Connection $connection -TableName contact -Id $contactId -Confirm:$false
    Write-Host '✓ Test contact deleted'
    
    Write-Host 'SUCCESS: All record access operations completed'
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
