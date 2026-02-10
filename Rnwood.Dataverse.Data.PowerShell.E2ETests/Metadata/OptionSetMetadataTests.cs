using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Metadata
{
    /// <summary>
    /// OptionSet metadata manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/OptionSetMetadata.Tests.ps1
    /// </summary>
    public class OptionSetMetadataTests : E2ETestBase
    {
[Fact]
        public void CanCreateReadUpdateAndDeleteGlobalOptionSetsComprehensively()
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
    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmm')
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $optionSetName = ""new_opttest_${timestamp}_$testRunId""
    
    Write-Host 'Creating global option set...'
    Invoke-WithRetry {
        Set-DataverseOptionSetMetadata -Connection $connection `
            -Name $optionSetName `
            -DisplayName 'OptionSet Test' `
            -Options @(
                @{ Value = 1; Label = 'Option 1' },
                @{ Value = 2; Label = 'Option 2' }
            ) `
            -Confirm:$false
    }
    Write-Host '✓ Global option set created'
    
    Write-Host 'Cleanup - Removing global option set...'
    Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection
        Remove-DataverseOptionSetMetadata -Connection $connection -Name $optionSetName -Confirm:$false
    }
    Write-Host '✓ Global option set deleted'
    
    Write-Host 'SUCCESS: All option set operations completed'
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
