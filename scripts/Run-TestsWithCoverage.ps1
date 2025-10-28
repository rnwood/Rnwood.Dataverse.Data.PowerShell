#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs tests with code coverage instrumentation using coverlet
.DESCRIPTION
    Uses coverlet to instrument the cmdlets assembly and collect code coverage
    during test execution. Generates per-cmdlet coverage reports from the results.
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$TestPath = "tests",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputDir = "coverage",
    
    [Parameter(Mandatory=$false)]
    [string]$BaselineCoverageFile
)

$ErrorActionPreference = "Stop"

Write-Host "=== Running Tests with Code Coverage Instrumentation ===" -ForegroundColor Cyan

# Ensure we have the module path
if (-not $env:TESTMODULEPATH) {
    $env:TESTMODULEPATH = (Resolve-Path "out/Rnwood.Dataverse.Data.PowerShell").Path
}
Write-Host "Module path: $env:TESTMODULEPATH" -ForegroundColor Green

# Create output directory
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# Install coverlet.console if needed
Write-Host "Installing coverlet.console..." -ForegroundColor Yellow
dotnet tool install --global coverlet.console --version 6.0.2 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne 1) {
    Write-Warning "coverlet.console installation returned exit code $LASTEXITCODE"
}

# Path to the cmdlets assembly to instrument
$cmdletsAssembly = Join-Path $env:TESTMODULEPATH "cmdlets/net8.0/Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll"
if (-not (Test-Path $cmdletsAssembly)) {
    throw "Cmdlets assembly not found at: $cmdletsAssembly"
}
Write-Host "Instrumenting: $cmdletsAssembly" -ForegroundColor Green

# Install Pester if needed
Write-Host "Ensuring Pester is installed..." -ForegroundColor Yellow
Install-Module -Force -Scope CurrentUser -SkipPublisherCheck Pester -MinimumVersion 5.0.0 -MaximumVersion 5.99.99 -ErrorAction SilentlyContinue

# Create test runner script for coverlet
$testRunnerScript = Join-Path $OutputDir "run-tests.ps1"
@"
`$ErrorActionPreference = 'Stop'
`$env:TESTMODULEPATH = '$env:TESTMODULEPATH'

Write-Host "Running tests..." -ForegroundColor Cyan

`$config = New-PesterConfiguration
`$config.Run.Path = '$TestPath'
`$config.Run.PassThru = `$true
`$config.Run.Exit = `$false
`$config.Output.Verbosity = 'Normal'
`$config.Should.ErrorAction = 'Continue'

`$result = Invoke-Pester -Configuration `$config

Write-Host ""
Write-Host "Test Results:" -ForegroundColor Cyan
Write-Host "  Total:   `$(`$result.TotalCount)" -ForegroundColor White
Write-Host "  Passed:  `$(`$result.PassedCount)" -ForegroundColor Green
Write-Host "  Failed:  `$(`$result.FailedCount)" -ForegroundColor `$(if (`$result.FailedCount -gt 0) { "Red" } else { "Green" })

if (`$result.FailedCount -gt 0) {
    Write-Host ""
    Write-Host "Failed Tests:" -ForegroundColor Red
    foreach (`$test in `$result.Failed) {
        Write-Host "  - `$(`$test.ExpandedPath)" -ForegroundColor Red
    }
    exit 1
}
exit 0
"@ | Set-Content -Path $testRunnerScript

# Run coverlet
Write-Host ""
Write-Host "Running tests under coverlet instrumentation..." -ForegroundColor Cyan
$coverageOutput = Join-Path $OutputDir "coverage"
$pwshPath = (Get-Command pwsh).Source

$coverletArgs = @(
    $cmdletsAssembly,
    "--target", $pwshPath,
    "--targetargs", "-NoProfile -File `"$testRunnerScript`"",
    "--format", "cobertura",
    "--format", "json",
    "--output", $coverageOutput,
    "--exclude", "[FakeXrmEasy*]*",
    "--exclude", "[*Tests*]*",
    "--exclude-by-attribute", "GeneratedCode",
    "--exclude-by-attribute", "ExcludeFromCodeCoverage"
)

& coverlet @coverletArgs
$testExitCode = $LASTEXITCODE

# Check if coverage files exist
$coberturaFile = "$coverageOutput.cobertura.xml"
$jsonFile = "$coverageOutput.json"

if (-not (Test-Path $coberturaFile)) {
    Write-Error "Coverage file not generated at: $coberturaFile"
    exit 1
}

Write-Host ""
Write-Host "Coverage files generated:" -ForegroundColor Green
Write-Host "  Cobertura XML: $coberturaFile" -ForegroundColor Cyan
Write-Host "  JSON: $jsonFile" -ForegroundColor Cyan

# Parse coverage and generate per-cmdlet report
Write-Host ""
Write-Host "Generating per-cmdlet coverage report..." -ForegroundColor Cyan

[xml]$coverage = Get-Content $coberturaFile

# Overall statistics
$overallLineRate = [math]::Round([double]$coverage.coverage.'line-rate' * 100, 2)
$overallBranchRate = [math]::Round([double]$coverage.coverage.'branch-rate' * 100, 2)
$linesCovered = [int]$coverage.coverage.'lines-covered'
$linesValid = [int]$coverage.coverage.'lines-valid'
$branchesCovered = [int]$coverage.coverage.'branches-covered'
$branchesValid = [int]$coverage.coverage.'branches-valid'

Write-Host "Overall Coverage: $overallLineRate% lines, $overallBranchRate% branches" -ForegroundColor Green

# Group coverage by cmdlet
$cmdletCoverage = @{}
$classes = $coverage.coverage.packages.package.classes.class

foreach ($class in $classes) {
    $className = $class.name
    
    # Extract cmdlet name from class name (e.g., GetDataverseRecordCmdlet -> Get-DataverseRecord)
    if ($className -match '\.(\w+)Cmdlet$') {
        $cmdletClassName = $matches[1]
        
        # Convert PascalCase to Verb-Noun format
        # This is a heuristic - we'll try to extract the verb
        $cmdletName = $cmdletClassName
        if ($cmdletClassName -match '^(Get|Set|New|Remove|Invoke|Add|Update|Export|Import|Compare|Publish|Test)(.+)$') {
            $verb = $matches[1]
            $noun = $matches[2]
            $cmdletName = "$verb-$noun"
        }
        
        $lineRate = [double]$class.'line-rate'
        $branchRate = [double]$class.'branch-rate'
        
        # Count lines
        $lines = $class.lines.line
        $totalLines = 0
        $coveredLines = 0
        
        if ($lines) {
            if ($lines -is [System.Array]) {
                $totalLines = $lines.Count
                $coveredLines = ($lines | Where-Object { [int]$_.hits -gt 0 }).Count
            } else {
                $totalLines = 1
                $coveredLines = if ([int]$lines.hits -gt 0) { 1 } else { 0 }
            }
        }
        
        $cmdletCoverage[$cmdletName] = @{
            ClassName = $cmdletClassName
            FullClassName = $className
            LineRate = [math]::Round($lineRate * 100, 2)
            BranchRate = [math]::Round($branchRate * 100, 2)
            LinesTotal = $totalLines
            LinesCovered = $coveredLines
        }
    }
}

Write-Host "Analyzed $($cmdletCoverage.Count) cmdlets" -ForegroundColor Cyan

# Load baseline if provided
$baseline = $null
if ($BaselineCoverageFile -and (Test-Path $BaselineCoverageFile)) {
    Write-Host "Loading baseline coverage..." -ForegroundColor Yellow
    $baseline = Get-Content $BaselineCoverageFile -Raw | ConvertFrom-Json
}

# Generate markdown report
$markdown = @"
# 📊 Code Coverage Report

## Overall Coverage

"@

if ($baseline) {
    $baselineLineRate = [double]$baseline.OverallLineRate
    $delta = $overallLineRate - $baselineLineRate
    $deltaStr = if ($delta -gt 0) {
        "📈 +$($delta.ToString('0.00'))%"
    } elseif ($delta -lt 0) {
        "📉 $($delta.ToString('0.00'))%"
    } else {
        "➡️ ±0.00%"
    }
    
    $markdown += @"
- **Line Coverage**: $overallLineRate% ($deltaStr from base)
- **Branch Coverage**: $overallBranchRate%
- **Lines Covered**: $linesCovered / $linesValid
- **Branches Covered**: $branchesCovered / $branchesValid

"@
} else {
    $markdown += @"
- **Line Coverage**: $overallLineRate%
- **Branch Coverage**: $overallBranchRate%
- **Lines Covered**: $linesCovered / $linesValid
- **Branches Covered**: $branchesCovered / $branchesValid

"@
}

$markdown += @"

## Coverage by Cmdlet

| Cmdlet | Line Coverage | Branch Coverage | Lines |
|--------|---------------|-----------------|-------|
"@

# Sort cmdlets by coverage (lowest first to highlight issues)
$sortedCmdlets = $cmdletCoverage.GetEnumerator() | Sort-Object { $_.Value.LineRate }

foreach ($entry in $sortedCmdlets) {
    $cmdlet = $entry.Key
    $data = $entry.Value
    
    $lineCov = "$($data.LineRate)%"
    $branchCov = "$($data.BranchRate)%"
    
    if ($baseline -and $baseline.CmdletCoverage.$cmdlet) {
        $baselineLineCov = [double]$baseline.CmdletCoverage.$cmdlet.LineRate
        $delta = $data.LineRate - $baselineLineCov
        if ($delta -gt 0) {
            $lineCov += " 📈+$($delta.ToString('0.0'))%"
        } elseif ($delta -lt 0) {
            $lineCov += " 📉$($delta.ToString('0.0'))%"
        }
    }
    
    $lines = "$($data.LinesCovered) / $($data.LinesTotal)"
    
    # Add indicator for low coverage
    $indicator = if ($data.LineRate -lt 30) {
        "⚠️ "
    } elseif ($data.LineRate -lt 60) {
        "⚡ "
    } else {
        "✅ "
    }
    
    $markdown += "`n| $indicator``$cmdlet`` | $lineCov | $branchCov | $lines |"
}

$markdown += @"


---
*Generated on $(Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC") with coverlet instrumentation*
"@

# Write markdown report
$markdownFile = Join-Path $OutputDir "coverage-report.md"
$markdown | Set-Content -Path $markdownFile -Encoding UTF8

Write-Host ""
Write-Host "Report written to: $markdownFile" -ForegroundColor Green

# Generate JSON for baseline comparison
$jsonData = @{
    Timestamp = (Get-Date).ToString("o")
    OverallLineRate = $overallLineRate
    OverallBranchRate = $overallBranchRate
    LinesCovered = $linesCovered
    LinesValid = $linesValid
    BranchesCovered = $branchesCovered
    BranchesValid = $branchesValid
    CmdletCoverage = @{}
}

foreach ($entry in $cmdletCoverage.GetEnumerator()) {
    $jsonData.CmdletCoverage[$entry.Key] = @{
        LineRate = $entry.Value.LineRate
        BranchRate = $entry.Value.BranchRate
        LinesCovered = $entry.Value.LinesCovered
        LinesTotal = $entry.Value.LinesTotal
    }
}

$jsonDataFile = Join-Path $OutputDir "coverage-data.json"
$jsonData | ConvertTo-Json -Depth 10 | Set-Content -Path $jsonDataFile -Encoding UTF8

Write-Host "Coverage data written to: $jsonDataFile" -ForegroundColor Green

Write-Host ""
Write-Host "✅ Coverage analysis complete!" -ForegroundColor Green
Write-Host "   Line Coverage: $overallLineRate%" -ForegroundColor $(if ($overallLineRate -ge 70) { "Green" } elseif ($overallLineRate -ge 50) { "Yellow" } else { "Red" })
Write-Host "   Branch Coverage: $overallBranchRate%" -ForegroundColor $(if ($overallBranchRate -ge 70) { "Green" } elseif ($overallBranchRate -ge 50) { "Yellow" } else { "Red" })

# Exit with test result
exit $testExitCode
