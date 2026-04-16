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
    /// <remarks>
    /// Placed in the SchemaAndPublishChanges collection because these tests publish sitemap
    /// customizations, which acquires a Dataverse customization lock that conflicts with other
    /// publish/schema-change operations when run in parallel.
    /// </remarks>
    [Collection(SchemaChangesCollection.Name)]
    public class SitemapTests : E2ETestBase
    {
        [Fact]
        public void CanPerformFullLifecycleOfSitemapManipulationIncludingEntries()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

try {
    # Generate unique test identifiers to avoid conflicts with parallel CI runs
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $sitemapUniqueName = ""test_sitemap_$testRunId""
    $sitemapName = ""Test Sitemap $testRunId""
    
    Write-Host ""Using test sitemap unique name: $sitemapUniqueName""
    
    # --- CLEANUP: Remove any previously failed test sitemaps ---
    Write-Host 'Cleaning up any existing test sitemaps from previous failed runs...'
    $existingSitemaps = Invoke-WithRetry {
        Get-DataverseSitemap -Connection $connection | Where-Object {
        public void PublishByUniqueName_PreservesSitemapXml_AndMakesPublishedVersionRetrievable()
        }
    }
    
    foreach ($existingSitemap in $existingSitemaps) {
        Write-Host ""  Removing old sitemap: $($existingSitemap.UniqueName) (ID: $($existingSitemap.Id))""
            Invoke-WithRetry {
                Remove-DataverseSitemap -Connection $connection -Id $existingSitemap.Id -IfExists -Confirm:$false
            }
    $updatedName = ""Updated $sitemapName""
        } catch {
            Write-Warning ""Failed to remove old sitemap $($existingSitemap.UniqueName): $_""

    Write-Host 'Creating unpublished sitemap...'
</SiteMap>
""@
    
    $sitemapId = Invoke-WithRetry {
        Set-DataverseSitemap -Connection $connection -Name $sitemapName -UniqueName $sitemapUniqueName -SitemapXml $sitemapXml -PassThru -Confirm:$false

    
    if (-not $sitemapId) {
        throw 'Failed to create sitemap - no ID returned'

    Write-Host ""Created sitemap with ID: $sitemapId""
    
    # --- TEST 2: Retrieve the created sitemap ---
    Write-Host ""`nTest 2: Retrieving sitemap by UniqueName...""

    Write-Host 'Updating sitemap and publishing it...'
    
    # Update name along with XML and publish (required for changes to take effect)
    Invoke-WithRetry {

    
    $updatedSitemap = Invoke-WithRetry {
        Get-DataverseSitemap -Connection $connection -Id $sitemapId

    if (-not $publishedSitemap) {
        throw 'Failed to retrieve published sitemap'
    }
    Write-Host 'Verified published sitemap is retrievable'
    
    # --- TEST 10a: Verify original issue - Publish by UniqueName without other parameters ---
    if ($publishedSitemap.Name -ne $updatedName) {
        throw ""Published sitemap name mismatch. Expected: $updatedName, Got: $($publishedSitemap.Name)""
    }
    Write-Host 'Verified published sitemap is retrievable after publish'

    Write-Host 'Testing publish-only by UniqueName regression path...'
    
    if ([string]::IsNullOrWhiteSpace($sitemapBeforePublish.SitemapXml)) {
        throw 'Sitemap XML is already empty before test'

    
    $originalSitemapXml = $sitemapBeforePublish.SitemapXml
    Write-Host ""  Original sitemap XML length: $($originalSitemapXml.Length) characters""

    # This was the original issue: Set-DataverseSitemap -UniqueName 'XYZ' -Publish

    Write-Host '  Publish-only operation completed'
    
    # Verify the sitemap XML is NOT empty after publish-only operation

    
    if ([string]::IsNullOrWhiteSpace($sitemapAfterPublish.SitemapXml)) {
        throw 'REGRESSION: Sitemap XML became empty after publish-only operation! This is the original issue (bug #277).'

    
    Write-Host ""  Sitemap XML after publish: $($sitemapAfterPublish.SitemapXml.Length) characters""
    

        Write-Host '  Sitemap XML unchanged - correct behavior'
    }
    
    Write-Host 'Successfully verified original issue fix - sitemap XML not emptied by publish-only operation'
    

    Write-Host 'Cleaning up test sitemap...'
    Write-Host 'Successfully removed test sitemap'
    
    # Verify deletion

    if ($deletedSitemap) {
        throw 'Sitemap still exists after deletion'
    }
    Write-Host 'Verified sitemap was deleted'
    
    # --- TEST 12: Test IfExists flag on non-existent sitemap ---
    Write-Host 'All sitemap publish tests passed successfully'
    Write-Host ""ERROR: Test failed with exception: $_""
    Write-Host ""Stack trace: $($_.ScriptStackTrace)""
    # Try to clean up test sitemap if it exists
    try {
        if ($sitemapId) {
            Remove-DataverseSitemap -Connection $connection -Id $sitemapId -IfExists -Confirm:$false
            Write-Host 'Cleanup successful'
        }
    } catch {
        Write-Warning ""Failed to cleanup test sitemap: $_""
    }
    
    throw
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("All sitemap publish tests passed successfully", because: result.GetFullOutput());
        }
    }
}
