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

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void SetDataverseView_WithNameButNoId_ShouldUpdateExistingViewNotCreateDuplicate()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

try {
    # Generate unique test identifiers
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $viewName = ""E2E No Duplicate Test $testRunId""
    
    Write-Host ""Using test view name: $viewName""
    
    # Cleanup existing views
    Write-Host 'Cleaning up any existing test views...'
    $existingViews = Invoke-WithRetry {
        Get-DataverseRecord -Connection $connection -TableName savedquery -FilterValues @{
            'name:Like' = 'E2E No Duplicate Test %'
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
    
    # TEST 1: Create a view using Name and TableName (no ID)
    Write-Host ""`nTest 1: Creating view with Name and TableName...""
    $viewId1 = Invoke-WithRetry {
        Set-DataverseView -Connection $connection `
            -Name $viewName `
            -TableName 'contact' `
            -ViewType 'System' `
            -Columns @('fullname', 'emailaddress1') `
            -QueryType 'MainApplicationView' `
            -Description 'First creation' `
            -PassThru `
            -Confirm:$false
    }
    
    if (-not $viewId1) {
        throw 'Failed to create view - no ID returned'
    }
    Write-Host ""Created view with ID: $viewId1""
    
    # TEST 2: Call Set-DataverseView again with same Name and TableName (no ID)
    # This should UPDATE the existing view, not create a duplicate
    Write-Host ""`nTest 2: Updating view using Name and TableName (no ID)...""
    $viewId2 = Invoke-WithRetry {
        Set-DataverseView -Connection $connection `
            -Name $viewName `
            -TableName 'contact' `
            -ViewType 'System' `
            -Columns @('fullname', 'emailaddress1', 'telephone1') `
            -QueryType 'MainApplicationView' `
            -Description 'Second update - should not create duplicate' `
            -PassThru `
            -Confirm:$false
    }
    
    if (-not $viewId2) {
        throw 'Failed to update view - no ID returned'
    }
    Write-Host ""Updated view with ID: $viewId2""
    
    # TEST 3: Verify IDs are the same (no duplicate created)
    Write-Host ""`nTest 3: Verifying no duplicate was created...""
    if ($viewId1 -ne $viewId2) {
        throw ""FAILED: Different IDs returned. First ID: $viewId1, Second ID: $viewId2. A duplicate view was created!""
    }
    Write-Host ""SUCCESS: Same ID returned ($viewId1 = $viewId2), no duplicate created""
    
    # TEST 4: Verify only one view with this name exists
    Write-Host ""`nTest 4: Verifying only one view exists...""
    $viewsWithName = Invoke-WithRetry {
        Get-DataverseView -Connection $connection `
            -Name $viewName `
            -TableName 'contact' `
            -ViewType 'System'
    }
    
    $viewCount = ($viewsWithName | Measure-Object).Count
    if ($viewCount -ne 1) {
        throw ""FAILED: Expected 1 view, found $viewCount views with name '$viewName'""
    }
    Write-Host ""SUCCESS: Confirmed only 1 view exists with name '$viewName'""
    
    # Cleanup
    Write-Host ""`nCleaning up test view...""
    Invoke-WithRetry {
        Remove-DataverseView -Connection $connection -Id $viewId1 -ViewType 'System' -Confirm:$false
    }
    
    Write-Host 'Successfully removed test view'
    Write-Host ""`nAll tests completed successfully!""
    Write-Host 'Success'
} catch {
    Write-Host ""ERROR: $($_ | Out-String)""
    throw
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
            result.StandardOutput.Should().Contain("no duplicate created");
        }
    }
}
