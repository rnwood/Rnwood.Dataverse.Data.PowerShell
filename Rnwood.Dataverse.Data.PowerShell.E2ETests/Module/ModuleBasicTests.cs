using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Module
{
    /// <summary>
    /// Basic module functionality tests against a real Dataverse environment.
    /// Converted from e2e-tests/Module.Tests.ps1
    /// </summary>
    public class ModuleBasicTests : E2ETestBase
    {
        [Fact]
        public void CanConnectToRealEnvAndQueryData()
        {


            var script = GetConnectionScript(@"
Get-DataverseRecord -Connection $connection -TableName systemuser | Out-Null
Write-Host 'Success'
");

            var result = RunScript(script, timeoutSeconds: 60);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void CanConnectToRealEnvAndQueryDataWithSQL()
        {


            var script = GetConnectionScript(@"
Invoke-DataverseSql -Connection $connection -Sql 'SELECT * FROM systemuser' | Out-Null
Write-Host 'Success'
");

            var result = RunScript(script, timeoutSeconds: 60);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void AllCmdletsHaveHelpAvailable()
        {
            var script = @"
Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop

$cmdlets = Get-Command -Module Rnwood.Dataverse.Data.PowerShell
Write-Host ""Testing help for $($cmdlets.Count) cmdlets""

$cmdletsWithoutHelp = @()
foreach ($cmdlet in $cmdlets) {
    $help = Get-Help $cmdlet.Name -ErrorAction SilentlyContinue
    if (-not $help) {
        $cmdletsWithoutHelp += $cmdlet.Name
    }
}

if ($cmdletsWithoutHelp.Count -gt 0) {
    throw ""The following cmdlets do not have help available: $($cmdletsWithoutHelp -join ', ')""
}

Write-Host 'Success: All cmdlets have help available'
";

            var result = RunScript(script, timeoutSeconds: 30);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void HelpContentReflectsHelpFilesWithExpectedStructure()
        {
            var script = @"
Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop

# Test a sample of important cmdlets to ensure help structure is correct
$testCmdlets = @(
    'Get-DataverseConnection',
    'Get-DataverseRecord',
    'Set-DataverseRecord',
    'Remove-DataverseRecord',
    'Invoke-DataverseRequest',
    'Invoke-DataverseSql',
    'Get-DataverseWhoAmI'
)

$issues = @()

foreach ($cmdletName in $testCmdlets) {
    Write-Host ""Testing help for $cmdletName""
    $help = Get-Help $cmdletName -Full
    
    # Verify help has a name
    if (-not $help.Name) {
        $issues += ""${cmdletName}: Missing Name""
    }
    
    # Verify help has syntax information
    if (-not $help.Syntax) {
        $issues += ""${cmdletName}: Missing Syntax""
    }
    
    # Verify help has parameters
    if (-not $help.Parameters) {
        $issues += ""${cmdletName}: Missing Parameters section""
    }
    
    # For cmdlets with parameters, verify parameter details exist
    if ($help.Parameters -and $help.Parameters.Parameter) {
        $paramCount = @($help.Parameters.Parameter).Count
        Write-Host ""  - Found $paramCount parameters""
        
        # Verify at least one parameter has description
        $paramsWithDescription = @($help.Parameters.Parameter | Where-Object { 
            ($_.Description -is [string] -and $_.Description) -or 
            ($_.Description.Text -and $_.Description.Text)
        })
        if ($paramsWithDescription.Count -eq 0) {
            $issues += ""${cmdletName}: No parameters have descriptions""
        }
    }
}

if ($issues.Count -gt 0) {
    throw ""Help validation issues found:`n$($issues -join ""`n"")""
}

Write-Host 'Success: All tested cmdlets have proper help structure'
";

            var result = RunScript(script, timeoutSeconds: 30);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void HelpFilesExistInEnGBDirectory()
        {
            var script = @"
Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop

# Check that the module directory has the en-GB help files
$modulePath = (Get-Module Rnwood.Dataverse.Data.PowerShell).ModuleBase
$helpPath = Join-Path $modulePath 'en-GB'

if (-not (Test-Path $helpPath)) {
    throw ""Help directory not found at: $helpPath""
}

$helpFiles = Get-ChildItem -Path $helpPath -Filter '*.xml'
Write-Host ""Found $($helpFiles.Count) help files in en-GB directory""

if ($helpFiles.Count -eq 0) {
    throw ""No help XML files found in $helpPath""
}

# Verify the main help file exists
$mainHelpFile = Join-Path $helpPath 'Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml'
if (-not (Test-Path $mainHelpFile)) {
    throw ""Main help file not found: $mainHelpFile""
}

Write-Host 'Success: Help files exist and are accessible'
";

            var result = RunScript(script, timeoutSeconds: 30);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

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

            var result = RunScript(script, timeoutSeconds: 90);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
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

            var result = RunScript(script, timeoutSeconds: 600); // 10 min timeout

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

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
