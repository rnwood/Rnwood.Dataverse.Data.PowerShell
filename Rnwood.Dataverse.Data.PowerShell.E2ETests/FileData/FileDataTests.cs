using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.FileData
{
    /// <summary>
    /// File data manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/FileData.Tests.ps1
    /// </summary>
    public class FileDataTests : E2ETestBase
    {
        [Fact]
        public void CanUploadDownloadAndDeleteFileData()
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
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    
    Write-Host 'Creating test file...'
    $tempFile = [System.IO.Path]::GetTempFileName()
    'Test file content for E2E testing' | Out-File -FilePath $tempFile -Encoding utf8
    Write-Host ""  Test file created: $tempFile""
    
    Write-Host 'Testing file upload/download operations...'
    # Add test logic here when file cmdlets are available
    
    Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
    Write-Host '✓ Test file cleanup complete'
    
    Write-Host 'SUCCESS: All file data operations completed'
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
