using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Sitemap
{
    /// <summary>
    /// Sitemap manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/Sitemap.Tests.ps1
    /// </summary>
    public class SitemapTests : E2ETestBase
    {
        [Fact(Skip = "Sitemap cmdlets have known issues - sitemap XML not saving correctly. See original Pester test which is also skipped.")]
        public void CanPerformFullLifecycleOfSitemapManipulationIncludingEntries()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

<<<<<<< HEAD
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

>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
try {
    # Generate unique test identifiers to avoid conflicts with parallel CI runs
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $sitemapUniqueName = ""test_sitemap_$testRunId""
    $sitemapName = ""Test Sitemap $testRunId""
    
    Write-Host ""Using test sitemap unique name: $sitemapUniqueName""
    
    # --- CLEANUP: Remove any previously failed test sitemaps ---
    Write-Host 'Cleaning up any existing test sitemaps from previous failed runs...'
    $existingSitemaps = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemap -Connection $connection | Where-Object {
            $_.UniqueName -like 'test_sitemap_*'
        }
    }
    
    foreach ($existingSitemap in $existingSitemaps) {
        Write-Host ""  Removing old sitemap: $($existingSitemap.UniqueName) (ID: $($existingSitemap.Id))""
        try {
            Invoke-WithRetry {
<<<<<<< HEAD
=======
                Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
                Remove-DataverseSitemap -Connection $connection -Id $existingSitemap.Id -IfExists -Confirm:$false
            }
        } catch {
            Write-Warning ""Failed to remove old sitemap $($existingSitemap.UniqueName): $_""
        }
    }
    Write-Host 'Cleanup complete.'
    
    # --- TEST 1: Create a new sitemap ---
    Write-Host ""`nTest 1: Creating new sitemap...""
    $sitemapXml = @""
<SiteMap IntroducedVersion=""7.0.0.0"">
  <Area Id=""area_test_$testRunId"" ResourceId=""Area.Test"" ShowGroups=""true"" IntroducedVersion=""7.0.0.0"">
    <Titles>
      <Title LCID=""1033"" Title=""Test Area $testRunId"" />
    </Titles>
    <Group Id=""group_test_$testRunId"" ResourceId=""Group.Test"" IntroducedVersion=""7.0.0.0"" IsProfile=""false"">
      <Titles>
        <Title LCID=""1033"" Title=""Test Group $testRunId"" />
      </Titles>
      <SubArea Id=""subarea_test_$testRunId"" Icon=""/_imgs/imagestrips/transparent_spacer.gif"" Entity=""contact"" Client=""All,Outlook,OutlookLaptopClient,OutlookWorkstationClient,Web"" AvailableOffline=""true"" PassParams=""false"" Sku=""All,OnPremise,Live,SPLA"">
        <Titles>
          <Title LCID=""1033"" Title=""Test SubArea $testRunId"" />
        </Titles>
      </SubArea>
    </Group>
  </Area>
</SiteMap>
""@
    
    $sitemapId = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Set-DataverseSitemap -Connection $connection -Name $sitemapName -UniqueName $sitemapUniqueName -SitemapXml $sitemapXml -PassThru -Confirm:$false
    }
    
    if (-not $sitemapId) {
        throw 'Failed to create sitemap - no ID returned'
    }
    Write-Host ""Created sitemap with ID: $sitemapId""
    
    # --- TEST 2: Retrieve the created sitemap ---
    Write-Host ""`nTest 2: Retrieving sitemap by UniqueName...""
    $retrievedSitemap = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemap -Connection $connection -UniqueName $sitemapUniqueName
    }
    
    if (-not $retrievedSitemap) {
        throw 'Failed to retrieve sitemap by UniqueName'
    }
    if ($retrievedSitemap.Id -ne $sitemapId) {
        throw ""Retrieved sitemap ID mismatch. Expected: $sitemapId, Got: $($retrievedSitemap.Id)""
    }
    if ($retrievedSitemap.Name -ne $sitemapName) {
        throw ""Retrieved sitemap Name mismatch. Expected: $sitemapName, Got: $($retrievedSitemap.Name)""
    }
    Write-Host ""Successfully retrieved sitemap: $($retrievedSitemap.Name)""
    
    # --- TEST 3: Retrieve sitemap by ID ---
    Write-Host ""`nTest 3: Retrieving sitemap by ID...""
    $retrievedById = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemap -Connection $connection -Id $sitemapId
    }
    
    if (-not $retrievedById) {
        throw 'Failed to retrieve sitemap by ID'
    }
    if ($retrievedById.UniqueName -ne $sitemapUniqueName) {
        throw 'Retrieved sitemap UniqueName mismatch'
    }
    Write-Host 'Successfully retrieved sitemap by ID'
    
    # --- TEST 4: Update the sitemap ---
    Write-Host ""`nTest 4: Updating sitemap...""
    $updatedName = ""Updated $sitemapName""
    
    # Get current sitemap to retrieve XML
    $currentSitemap = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemap -Connection $connection -Id $sitemapId
    }
    
    # Update name along with XML to ensure proper Dataverse handling
    Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Set-DataverseSitemap -Connection $connection -Id $sitemapId -Name $updatedName -SitemapXml $currentSitemap.SitemapXml -Confirm:$false
    }
    
    $updatedSitemap = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemap -Connection $connection -Id $sitemapId
    }
    if ($updatedSitemap.Name -ne $updatedName) {
        throw ""Sitemap name update failed. Expected: $updatedName, Got: $($updatedSitemap.Name)""
    }
    Write-Host ""Successfully updated sitemap name to: $updatedName""
    
    # --- TEST 5: Retrieve sitemap entries ---
    Write-Host ""`nTest 5: Retrieving sitemap entries...""
    
    # Get all entries
    $allEntries = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId
    }
    if ($allEntries.Count -eq 0) {
        throw 'No sitemap entries found'
    }
    Write-Host ""Found $($allEntries.Count) sitemap entries""
    
    # Get Area entries
    $areaEntries = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Area
    }
    if ($areaEntries.Count -eq 0) {
        throw 'No Area entries found'
    }
    Write-Host ""Found $($areaEntries.Count) Area entries""
    
    # Get Group entries
    $groupEntries = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Group
    }
    if ($groupEntries.Count -eq 0) {
        throw 'No Group entries found'
    }
    Write-Host ""Found $($groupEntries.Count) Group entries""
    
    # Get SubArea entries
    $subAreaEntries = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea
    }
    if ($subAreaEntries.Count -eq 0) {
        throw 'No SubArea entries found'
    }
    Write-Host ""Found $($subAreaEntries.Count) SubArea entries""
    
    # --- TEST 6: Add a new Area entry ---
    Write-Host ""`nTest 6: Adding new Area entry...""
    $newAreaId = ""area_new_$testRunId""
    $newAreaEntry = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Set-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Area ```
        -EntryId $newAreaId ```
        -Titles @{ 1033 = ""New Test Area $testRunId"" } ```
        -Icon ""/_imgs/area_icon.png"" ```
        -PassThru -Confirm:$false
    }
    
    if (-not $newAreaEntry) {
        throw 'Failed to create new Area entry'
    }
    Write-Host ""Created new Area entry: $newAreaId""
    
    # Verify new area exists
    $verifyArea = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Area -EntryId $newAreaId
    }
    if (-not $verifyArea) {
        throw 'Failed to retrieve newly created Area entry'
    }
    Write-Host 'Verified new Area entry exists'
    
    # --- TEST 7: Add a Group to the new Area ---
    Write-Host ""`nTest 7: Adding Group to new Area...""
    $newGroupId = ""group_new_$testRunId""
    $newGroupEntry = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Set-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Group ```
        -EntryId $newGroupId ```
        -ParentAreaId $newAreaId ```
        -Titles @{ 1033 = ""New Test Group $testRunId"" } ```
        -PassThru -Confirm:$false
    }
    
    if (-not $newGroupEntry) {
        throw 'Failed to create new Group entry'
    }
    Write-Host ""Created new Group entry: $newGroupId""
    
    # --- TEST 8: Add a SubArea to the new Group ---
    Write-Host ""`nTest 8: Adding SubArea to new Group...""
    $newSubAreaId = ""subarea_new_$testRunId""
    $newSubAreaEntry = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Set-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea ```
        -EntryId $newSubAreaId ```
        -ParentAreaId $newAreaId ```
        -ParentGroupId $newGroupId ```
        -Entity ""account"" ```
        -Titles @{ 1033 = ""New Test SubArea $testRunId"" } ```
        -Icon ""/_imgs/subarea_icon.png"" ```
        -PassThru -Confirm:$false
    }
    
    if (-not $newSubAreaEntry) {
        throw 'Failed to create new SubArea entry'
    }
    Write-Host ""Created new SubArea entry: $newSubAreaId""
    
    # --- TEST 9: Update an existing entry ---
    Write-Host ""`nTest 9: Updating SubArea entry...""
    $updatedSubAreaEntry = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Set-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea ```
        -EntryId $newSubAreaId ```
        -Titles @{ 1033 = ""Updated SubArea $testRunId"" } ```
        -PassThru -Confirm:$false
    }
    
    if (-not $updatedSubAreaEntry) {
        throw 'Failed to update SubArea entry'
    }
    
    # Verify update
    $verifyUpdatedSubArea = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea -EntryId $newSubAreaId
    }
    $updatedTitle = $verifyUpdatedSubArea.Titles[1033]
    if ($updatedTitle -ne ""Updated SubArea $testRunId"") {
        throw ""SubArea title update failed. Expected: 'Updated SubArea $testRunId', Got: '$updatedTitle'""
    }
    Write-Host 'Successfully updated SubArea entry'
    
    # --- TEST 10: Publish sitemap ---
    Write-Host ""`nTest 10: Publishing sitemap...""
    Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Set-DataverseSitemap -Connection $connection -Id $sitemapId -Name $updatedName -Publish -Confirm:$false
    }
    Write-Host 'Successfully published sitemap'
    
    # Verify published sitemap is retrievable
    $publishedSitemap = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemap -Connection $connection -Id $sitemapId -Published
    }
    if (-not $publishedSitemap) {
        throw 'Failed to retrieve published sitemap'
    }
    Write-Host 'Verified published sitemap is retrievable'
    
    # --- TEST 10a: Verify original issue - Publish by UniqueName without other parameters ---
    Write-Host ""`nTest 10a: Testing original issue - Publish sitemap by UniqueName only (no Name, no SitemapXml)...""
    
    # Get the current sitemap XML before publish-only operation
    $sitemapBeforePublish = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemap -Connection $connection -UniqueName $sitemapUniqueName
    }
    
    if ([string]::IsNullOrWhiteSpace($sitemapBeforePublish.SitemapXml)) {
        throw 'Sitemap XML is already empty before test'
    }
    
    $originalSitemapXml = $sitemapBeforePublish.SitemapXml
    Write-Host ""  Original sitemap XML length: $($originalSitemapXml.Length) characters""
    
    # This was the original issue: Set-DataverseSitemap -UniqueName 'XYZ' -Publish
    # It should NOT empty the sitemap XML
    Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Set-DataverseSitemap -Connection $connection -UniqueName $sitemapUniqueName -Publish -Confirm:$false
    }
    Write-Host '  Publish-only operation completed'
    
    # Verify the sitemap XML is NOT empty after publish-only operation
    $sitemapAfterPublish = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemap -Connection $connection -UniqueName $sitemapUniqueName
    }
    
    if ([string]::IsNullOrWhiteSpace($sitemapAfterPublish.SitemapXml)) {
        throw 'REGRESSION: Sitemap XML became empty after publish-only operation! This is the original issue (bug #277).'
    }
    
    Write-Host ""  Sitemap XML after publish: $($sitemapAfterPublish.SitemapXml.Length) characters""
    
    if ($sitemapAfterPublish.SitemapXml -ne $originalSitemapXml) {
        Write-Warning '  Sitemap XML changed after publish (this may be expected due to Dataverse normalization)'
    } else {
        Write-Host '  Sitemap XML unchanged - correct behavior'
    }
    
    Write-Host 'Successfully verified original issue fix - sitemap XML not emptied by publish-only operation'
    
    # --- TEST 11: Remove SubArea entry ---
    Write-Host ""`nTest 11: Removing SubArea entry...""
    Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Remove-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea -EntryId $newSubAreaId -Confirm:$false
    }
    
    # Verify deletion
    $deletedSubArea = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -SubArea -EntryId $newSubAreaId
    }
    if ($deletedSubArea) {
        throw 'SubArea entry still exists after deletion'
    }
    Write-Host 'Successfully removed SubArea entry'
    
    # --- CLEANUP: Remove the test sitemap ---
    Write-Host ""`nCleaning up: Removing test sitemap...""
    Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Remove-DataverseSitemap -Connection $connection -Id $sitemapId -Confirm:$false
    }
    Write-Host 'Successfully removed test sitemap'
    
    # Verify deletion
    $deletedSitemap = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseSitemap -Connection $connection -Id $sitemapId
    }
    if ($deletedSitemap) {
        throw 'Sitemap still exists after deletion'
    }
    Write-Host 'Verified sitemap was deleted'
    
    # --- TEST 12: Test IfExists flag on non-existent sitemap ---
    Write-Host ""`nTest 12: Testing IfExists flag on non-existent sitemap...""
    Remove-DataverseSitemap -Connection $connection -Id $sitemapId -IfExists -Confirm:$false
    Write-Host 'IfExists flag worked correctly - no error on non-existent sitemap'
    
    Write-Host ""`n=== All sitemap tests passed successfully ===""
    
}
catch {
    # Attempt cleanup on failure
    Write-Host ""ERROR: Test failed with exception: $_""
    Write-Host ""Stack trace: $($_.ScriptStackTrace)""
    
    # Try to clean up test sitemap if it exists
    try {
        if ($sitemapId) {
            Write-Host 'Attempting cleanup of test sitemap...'
            Remove-DataverseSitemap -Connection $connection -Id $sitemapId -IfExists -Confirm:$false
            Write-Host 'Cleanup successful'
        }
    } catch {
        Write-Warning ""Failed to cleanup test sitemap: $_""
    }
    
    throw
}
");

<<<<<<< HEAD
            var result = RunScript(script);
=======
            var result = RunScript(script, timeoutSeconds: 600);
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("All sitemap tests passed successfully");
        }
    }
}
