#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates a cmdlet test coverage report
.DESCRIPTION
    Analyzes which cmdlets have tests and generates a coverage report
    showing test counts, pass rates, and identifies untested cmdlets.
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$TestPath = "tests",
    
    [Parameter(Mandatory=$false)]
    [string]$CmdletsPath = "Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputMarkdown = "coverage/test-coverage-report.md",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputJson = "coverage/test-coverage.json",
    
    [Parameter(Mandatory=$false)]
    [string]$BaselineJson
)

$ErrorActionPreference = "Stop"

Write-Host "=== Analyzing Cmdlet Test Coverage ===" -ForegroundColor Cyan

# Get all cmdlet classes
Write-Host "Finding cmdlets..." -ForegroundColor Yellow
$cmdletFiles = Get-ChildItem -Path $CmdletsPath -Filter "*Cmdlet.cs" -Recurse

$allCmdlets = @{}
foreach ($file in $cmdletFiles) {
    $content = Get-Content $file.FullName -Raw
    
    # Extract cmdlet attribute to get verb and noun
    # [Cmdlet(VerbsCommon.Get, "DataverseConnection")]
    if ($content -match '\[Cmdlet\([^\)]+,\s*"([^"]+)"\)') {
        $noun = $matches[1]
        
        # Extract verb from the VerbsXxx.Verb pattern or string literal
        $verbPattern = 'Cmdlet\((Verbs\w+\.(\w+)|"(\w+)")'
        if ($content -match $verbPattern) {
            $verb = if ($matches[2]) { $matches[2] } else { $matches[3] }
            $cmdletName = "$verb-$noun"
            
            $allCmdlets[$cmdletName] = @{
                ClassName = $file.BaseName
                FilePath = $file.FullName
                Verb = $verb
                Noun = $noun
                Tests = @()
                TestCount = 0
                HasTests = $false
            }
        }
    }
}

Write-Host "Found $($allCmdlets.Count) cmdlets" -ForegroundColor Green

# Install Pester if needed
Write-Host "Ensuring Pester is installed..." -ForegroundColor Yellow
Install-Module -Force -Scope CurrentUser -SkipPublisherCheck Pester -MinimumVersion 5.0.0 -MaximumVersion 5.99.99 -ErrorAction SilentlyContinue

# Run tests to get detailed results
Write-Host "Running tests..." -ForegroundColor Yellow
$env:TESTMODULEPATH = if ($env:TESTMODULEPATH) { $env:TESTMODULEPATH } else { (Resolve-Path "out/Rnwood.Dataverse.Data.PowerShell").Path }

$config = New-PesterConfiguration
$config.Run.Path = $TestPath
$config.Run.PassThru = $true
$config.Output.Verbosity = 'Minimal'
$config.Should.ErrorAction = 'Continue'

$testResult = Invoke-Pester -Configuration $config

Write-Host ""
Write-Host "Tests completed. Analyzing results..." -ForegroundColor Yellow

# Analyze which cmdlets were tested
foreach ($test in $testResult.Tests) {
    $testName = $test.Name
    $testBlock = $test.Block
    $testPath = $test.Path
    
    # Check if test name or path contains cmdlet name
    foreach ($cmdletName in $allCmdlets.Keys) {
        if ($testName -match $cmdletName -or $testPath -match $cmdletName) {
            $allCmdlets[$cmdletName].HasTests = $true
            $allCmdlets[$cmdletName].TestCount++
            $allCmdlets[$cmdletName].Tests += @{
                Name = $testName
                Result = $test.Result
                Duration = $test.Duration
            }
        }
    }
}

# Calculate statistics
$testedCmdlets = ($allCmdlets.Values | Where-Object { $_.HasTests }).Count
$untestedCmdlets = ($allCmdlets.Values | Where-Object { -not $_.HasTests }).Count
$totalTests = $testResult.TotalCount
$passedTests = $testResult.PassedCount
$failedTests = $testResult.FailedCount
$coveragePercent = if ($allCmdlets.Count -gt 0) { 
    [math]::Round(($testedCmdlets / $allCmdlets.Count) * 100, 2) 
} else { 
    0 
}

Write-Host ""
Write-Host "Coverage Statistics:" -ForegroundColor Cyan
Write-Host "  Cmdlets with tests: $testedCmdlets / $($allCmdlets.Count) ($coveragePercent%)" -ForegroundColor Green
Write-Host "  Total test cases: $totalTests" -ForegroundColor Cyan
Write-Host "  Passed: $passedTests" -ForegroundColor Green
Write-Host "  Failed: $failedTests" -ForegroundColor $(if ($failedTests -gt 0) { "Red" } else { "Green" })

# Load baseline if provided
$baseline = $null
if ($BaselineJson -and (Test-Path $BaselineJson)) {
    Write-Host "Loading baseline from: $BaselineJson" -ForegroundColor Yellow
    $baseline = Get-Content $BaselineJson -Raw | ConvertFrom-Json
}

# Generate markdown report
Write-Host ""
Write-Host "Generating markdown report..." -ForegroundColor Yellow

$markdown = @"
# 🧪 Cmdlet Test Coverage Report

## Summary

"@

if ($baseline) {
    $baselineCoveragePercent = $baseline.CoveragePercent
    $delta = $coveragePercent - $baselineCoveragePercent
    $deltaStr = if ($delta -gt 0) {
        "📈 +$($delta.ToString('0.00'))%"
    } elseif ($delta -lt 0) {
        "📉 $($delta.ToString('0.00'))%"
    } else {
        "➡️ ±0.00%"
    }
    
    $markdown += @"
- **Cmdlet Coverage**: $coveragePercent% ($deltaStr from base)
- **Cmdlets with Tests**: $testedCmdlets / $($allCmdlets.Count)
- **Total Tests**: $totalTests
- **Test Results**: ✅ $passedTests passed, ❌ $failedTests failed

"@
} else {
    $markdown += @"
- **Cmdlet Coverage**: $coveragePercent%
- **Cmdlets with Tests**: $testedCmdlets / $($allCmdlets.Count)
- **Total Tests**: $totalTests
- **Test Results**: ✅ $passedTests passed, ❌ $failedTests failed

"@
}

# Tested cmdlets section
$markdown += @"

## ✅ Cmdlets with Tests ($testedCmdlets)

| Cmdlet | Test Count | Status |
|--------|------------|--------|
"@

$testedList = $allCmdlets.GetEnumerator() | 
    Where-Object { $_.Value.HasTests } | 
    Sort-Object Name

foreach ($entry in $testedList) {
    $cmdlet = $entry.Key
    $data = $entry.Value
    
    $testCount = $data.TestCount
    $passCount = ($data.Tests | Where-Object { $_.Result -eq 'Passed' }).Count
    $failCount = ($data.Tests | Where-Object { $_.Result -eq 'Failed' }).Count
    
    $status = if ($failCount -gt 0) {
        "❌ $failCount failed"
    } else {
        "✅ All passed"
    }
    
    $markdown += "`n| ``$cmdlet`` | $testCount tests | $status |"
}

# Untested cmdlets section
$markdown += @"


## ⚠️ Cmdlets without Tests ($untestedCmdlets)

"@

if ($untestedCmdlets -gt 0) {
    $markdown += @"
| Cmdlet | Class Name |
|--------|------------|
"@
    
    $untestedList = $allCmdlets.GetEnumerator() | 
        Where-Object { -not $_.Value.HasTests } | 
        Sort-Object Name
    
    foreach ($entry in $untestedList) {
        $cmdlet = $entry.Key
        $data = $entry.Value
        $markdown += "`n| ``$cmdlet`` | $($data.ClassName) |"
    }
} else {
    $markdown += @"
🎉 **All cmdlets have tests!**
"@
}

$markdown += @"


---
*Generated on $(Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC")*
"@

# Write markdown report
New-Item -ItemType Directory -Force -Path (Split-Path $OutputMarkdown) | Out-Null
$markdown | Set-Content -Path $OutputMarkdown -Encoding UTF8

Write-Host "Markdown report written to: $OutputMarkdown" -ForegroundColor Green

# Generate JSON output for baseline comparison
$jsonData = @{
    Timestamp = (Get-Date).ToString("o")
    CoveragePercent = $coveragePercent
    TotalCmdlets = $allCmdlets.Count
    TestedCmdlets = $testedCmdlets
    UntestedCmdlets = $untestedCmdlets
    TotalTests = $totalTests
    PassedTests = $passedTests
    FailedTests = $failedTests
    Cmdlets = @{}
}

foreach ($entry in $allCmdlets.GetEnumerator()) {
    $jsonData.Cmdlets[$entry.Key] = @{
        HasTests = $entry.Value.HasTests
        TestCount = $entry.Value.TestCount
        ClassName = $entry.Value.ClassName
    }
}

New-Item -ItemType Directory -Force -Path (Split-Path $OutputJson) | Out-Null
$jsonData | ConvertTo-Json -Depth 10 | Set-Content -Path $OutputJson -Encoding UTF8

Write-Host "JSON data written to: $OutputJson" -ForegroundColor Green

Write-Host ""
Write-Host "✅ Coverage analysis complete!" -ForegroundColor Green
Write-Host "   Coverage: $coveragePercent%" -ForegroundColor $(if ($coveragePercent -ge 50) { "Green" } else { "Yellow" })

# Exit with error code if there are failed tests
if ($failedTests -gt 0) {
    Write-Host ""
    Write-Warning "$failedTests test(s) failed"
    exit 1
}

exit 0
