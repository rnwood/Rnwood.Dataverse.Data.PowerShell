using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.WebResource
{
    /// <summary>
    /// Tests for web resource management operations.
    /// </summary>
    public class WebResourceTests : E2ETestBase
    {
        [Fact]
        public void CanCreateRetrieveUpdateAndDeleteWebResources()
        {


            var script = GetConnectionScript(@"
# Create a unique name for test web resource
$testPrefix = 'test_e2e_' + [Guid]::NewGuid().ToString().Substring(0, 8)
$webResourceName = $testPrefix

Write-Host ""Creating test web resource: $webResourceName""

# Create a test file
$tempFile = [System.IO.Path]::GetTempFileName()
$tempJsFile = [System.IO.Path]::ChangeExtension($tempFile, '.js')
Move-Item $tempFile $tempJsFile -Force
""console.log('E2E test');"" | Out-File -FilePath $tempJsFile -NoNewline -Encoding utf8

# Test: Create web resource from file
Set-DataverseWebResource -Connection $connection -Name $webResourceName -Path $tempJsFile -DisplayName 'E2E Test Resource' -PublisherPrefix 'test' -PassThru | Out-Null
Write-Host ""Created web resource: $webResourceName""

# Test: Retrieve by name
$retrieved = Get-DataverseWebResource -Connection $connection -Name $webResourceName
if (-not $retrieved) {
    throw 'Failed to retrieve web resource by name'
}

# Test: Update web resource
""console.log('E2E test updated');"" | Out-File -FilePath $tempJsFile -NoNewline -Encoding utf8
Set-DataverseWebResource -Connection $connection -Name $webResourceName -Path $tempJsFile | Out-Null

# Cleanup
Remove-Item $tempJsFile -Force -ErrorAction SilentlyContinue

# Delete web resource
Remove-DataverseWebResource -Connection $connection -Name $webResourceName -Confirm:$false | Out-Null

Write-Host 'Success: Web resource operations completed'
");

            var result = RunScript(script, timeoutSeconds: 120);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void CanBatchUploadAndDownloadWebResourcesFromFolder()
        {


            var script = GetConnectionScript(@"
# Create test files in a temp folder
$testPrefix = 'test_e2e_batch_' + [Guid]::NewGuid().ToString().Substring(0, 8)
$tempFolder = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
New-Item -ItemType Directory -Path $tempFolder -Force | Out-Null

Write-Host ""Creating test files in: $tempFolder""

# Create 3 test files
""console.log('test1');"" | Out-File -FilePath (Join-Path $tempFolder 'test1.js') -NoNewline -Encoding utf8
""console.log('test2');"" | Out-File -FilePath (Join-Path $tempFolder 'test2.js') -NoNewline -Encoding utf8
""body { color: red; }"" | Out-File -FilePath (Join-Path $tempFolder 'test.css') -NoNewline -Encoding utf8

# Upload folder
Write-Host 'Uploading web resources from folder'
Set-DataverseWebResource -Connection $connection -Folder $tempFolder -PublisherPrefix $testPrefix -FileFilter '*.*' | Out-Null

# Verify uploads
$uploaded = Get-DataverseWebResource -Connection $connection -Name ""${testPrefix}_*""
if ($uploaded.Count -lt 3) {
    throw ""Expected at least 3 web resources, found $($uploaded.Count)""
}

# Cleanup web resources
Write-Host 'Cleaning up web resources'
Get-DataverseWebResource -Connection $connection -Name ""${testPrefix}_*"" | 
    Remove-DataverseWebResource -Connection $connection -Confirm:$false | Out-Null

# Cleanup temp folder
Remove-Item $tempFolder -Recurse -Force -ErrorAction SilentlyContinue

Write-Host 'Success: Batch web resource operations completed'
");

            var result = RunScript(script, timeoutSeconds: 120);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }
    }
}
