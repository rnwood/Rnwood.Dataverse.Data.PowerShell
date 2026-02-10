using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Views
{
    /// <summary>
    /// Tests for view manipulation operations.
    /// Converted from e2e-tests/View.Tests.ps1
    /// </summary>
    public class ViewManipulationTests : E2ETestBase
    {
        [Fact]
        public void CanPerformFullLifecycleOfViewManipulationIncludingEmptyLayoutXmlUpdate()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

# Retry helper function
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
    # Generate unique test identifiers
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $viewName = ""E2E Test View $testRunId""
    
    Write-Host ""Using test view name: $viewName""
    
    # Cleanup existing views
    Write-Host 'Cleaning up any existing test views...'
    $existingViews = Invoke-WithRetry {
        Get-DataverseRecord -Connection $connection -TableName savedquery -FilterValues @{
            'name:Like' = 'E2E Test View %'
            'returnedtypecode' = 'contact'
        } -Columns savedqueryid, name -ErrorAction SilentlyContinue
    }
    
    if ($existingViews) {
        foreach ($view in $existingViews) {
            Write-Host ""  Removing old view: $($view.name)""
            try {
                Invoke-WithRetry {
                    Remove-DataverseView -Connection $connection -Id $view.savedqueryid -ViewType 'System' -Confirm:$false -IfExists
                }
            } catch {
                Write-Warning ""Failed to remove old view $($view.name): $_""
            }
        }
    }
    Write-Host 'Cleanup complete.'
    
    # TEST 1: Create a new view with FetchXML
    Write-Host ""`nTest 1: Creating new view with FetchXML...""
    $fetchXml = @""
<fetch>
  <entity name=""contact"">
    <attribute name=""fullname"" />
    <attribute name=""emailaddress1"" />
    <attribute name=""telephone1"" />
    <order attribute=""fullname"" descending=""false"" />
  </entity>
</fetch>
""@
    
    $viewId = Invoke-WithRetry {
        Set-DataverseView -Connection $connection `
            -Name $viewName `
            -TableName contact `
            -ViewType 'System' `
            -FetchXml $fetchXml `
            -Description 'E2E test view created by automated tests' `
            -PassThru `
            -Confirm:$false
    }
    
    if (-not $viewId) {
        throw 'Failed to create view - no ID returned'
    }
    Write-Host ""Created view with ID: $viewId""
    
    # TEST 2: Retrieve the created view
    Write-Host ""`nTest 2: Retrieving view by ID...""
    $retrievedView = Invoke-WithRetry {
        Get-DataverseView -Connection $connection -Id $viewId
    }
    
    if (-not $retrievedView) {
        throw 'Failed to retrieve view by ID'
    }
    if ($retrievedView.Name -ne $viewName) {
        throw ""Retrieved view Name mismatch. Expected: $viewName, Got: $($retrievedView.Name)""
    }
    Write-Host ""Successfully retrieved view: $($retrievedView.Name)""
    
    # TEST 3: Update view columns (bug fix scenario - empty layoutxml)
    Write-Host ""`nTest 3: Updating view columns (testing empty layoutxml bug fix)...""
    
    Invoke-WithRetry {
        Set-DataverseView -Connection $connection `
            -Id $viewId `
            -ViewType 'System' `
            -Columns @(
                @{ name = 'fullname'; width = 200 },
                @{ name = 'emailaddress1'; width = 150 },
                @{ name = 'telephone1'; width = 100 }
            ) `
            -Confirm:$false
    }
    
    Write-Host 'Successfully updated view columns (bug fix validated)'
    
    # TEST 5: Update view name and description
    Write-Host ""`nTest 5: Updating view name and description...""
    $updatedViewName = ""$viewName - Updated""
    Invoke-WithRetry {
        Set-DataverseView -Connection $connection `
            -Id $viewId `
            -ViewType 'System' `
            -Name $updatedViewName `
            -Description 'Updated description' `
            -Confirm:$false
    }
    
    Write-Host 'Successfully updated view metadata'
    
    # Cleanup
    Write-Host ""`nCleaning up: Removing test view...""
    Invoke-WithRetry {
        Remove-DataverseView -Connection $connection -Id $viewId -ViewType 'System' -Confirm:$false
    }
    
    Write-Host 'Successfully removed test view'
    Write-Host ""`nAll tests completed successfully!""
    Write-Host 'Success'
} catch {
    Write-Host ""ERROR: $($_ | Out-String)""
    throw
}
");

            var result = RunScript(script, timeoutSeconds: 120);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }
    }
}
