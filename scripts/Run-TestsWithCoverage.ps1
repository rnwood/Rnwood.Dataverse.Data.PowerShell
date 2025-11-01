#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs tests with code coverage using coverlet.console instrumentation
.DESCRIPTION
    Uses coverlet.console to instrument the cmdlets assembly and run tests
    with a modified test setup that doesn't copy the module (to preserve instrumentation).
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

Write-Host "=== Running Tests with Code Coverage (coverlet.console) ===" -ForegroundColor Cyan

# Create output directory
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# Build the project first (without instrumentation)
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build -c Release ./Rnwood.Dataverse.Data.PowerShell/Rnwood.Dataverse.Data.PowerShell.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}

# Copy built module to out directory
Write-Host "Copying built module to out directory..." -ForegroundColor Yellow
if (Test-Path out/Rnwood.Dataverse.Data.PowerShell) {
    Remove-Item -Force -Recurse out/Rnwood.Dataverse.Data.PowerShell
}
Copy-Item -Recurse Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0 out/Rnwood.Dataverse.Data.PowerShell

# Set module path for tests
$env:TESTMODULEPATH = (Resolve-Path "out/Rnwood.Dataverse.Data.PowerShell").Path
Write-Host "Module path: $env:TESTMODULEPATH" -ForegroundColor Green

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
Write-Host "Assembly to instrument: $cmdletsAssembly" -ForegroundColor Green

# Install Pester if needed
Write-Host "Ensuring Pester is installed..." -ForegroundColor Yellow
Install-Module -Force -Scope CurrentUser -SkipPublisherCheck Pester -MinimumVersion 5.0.0 -MaximumVersion 5.99.99 -ErrorAction SilentlyContinue

# Create a modified test runner that runs tests in-place without copying
# This is critical for coverlet to track coverage properly
$testRunnerScript = Join-Path (Resolve-Path $OutputDir).Path "run-tests-no-copy.ps1"
$absoluteTestPath = (Resolve-Path $TestPath).Path
@"
`$ErrorActionPreference = 'Stop'

Write-Host "Test Runner: Running tests WITHOUT module copy (for coverage)..." -ForegroundColor Cyan

# Set PSModulePath and env vars that tests expect
`$tempmodulefolder = Split-Path '$env:TESTMODULEPATH'
`$env:PSModulePath = `$tempmodulefolder + ";" + `$env:PSModulePath
`$env:ChildProcessPSModulePath = `$tempmodulefolder

# Override TESTMODULEPATH to skip the copy logic in All.Tests.ps1
# We create a flag that will be checked by a modified All.Tests.ps1 wrapper
`$env:COVERAGE_RUN = `$true
`$env:COVERAGE_MODULE_PATH = '$env:TESTMODULEPATH'

Write-Host "Module will be loaded from: $env:TESTMODULEPATH" -ForegroundColor Gray
Write-Host "PSModulePath set to: `$tempmodulefolder" -ForegroundColor Gray

# Import module using full path to the manifest (critical for coverlet instrumentation)
Import-Module '$env:TESTMODULEPATH/Rnwood.Dataverse.Data.PowerShell.psd1' -Force

# Define helper functions inline (same as All.Tests.ps1)
`$script:metadata = `$null

function global:getMockConnection([ScriptBlock]`$RequestInterceptor = `$null) {
    if (-not `$script:metadata) {
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        Add-Type -AssemblyName "System.Runtime.Serialization"

        # Define the DataContractSerializer
        `$serializer = New-Object System.Runtime.Serialization.DataContractSerializer([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
    
        Get-Item '$absoluteTestPath/*.xml' | ForEach-Object {
            `$stream = [IO.File]::OpenRead(`$_.FullName)
            `$script:metadata += `$serializer.ReadObject(`$stream)
            `$stream.Close()
        }
    }
   
    `$mockService = Get-DataverseConnection -url https://fake.crm.dynamics.com/ -mock `$script:metadata -RequestInterceptor `$RequestInterceptor
    return `$mockService
}

function global:newPwsh([scriptblock] `$scriptblock) {
    if ([System.Environment]::OSVersion.Platform -eq "Unix") {
        pwsh -noninteractive -noprofile -command `$scriptblock
    }
    else {
        cmd /c pwsh -noninteractive -noprofile -command `$scriptblock
    }
}

Write-Host "Helper functions loaded" -ForegroundColor Gray

# Dynamically discover and dot-source all test files (same as All.Tests.ps1)
`$testFiles = Get-ChildItem -Path '$absoluteTestPath' -Filter "*.ps1" | 
    Where-Object { 
        `$_.Name -ne "All.Tests.ps1" -and 
        `$_.Name -notlike "*.Tests.ps1" -and
        `$_.Name -notlike "generate-*.ps1" -and
        `$_.Name -notlike "updatemetadata.ps1"
    }

Write-Host "Dot-sourcing `$(`$testFiles.Count) test files..." -ForegroundColor Gray
foreach (`$testFile in `$testFiles) {
    Write-Host "  Loading `$(`$testFile.Name)..." -ForegroundColor Gray
    . `$testFile.FullName
}

Write-Host "Tests executed via dot-sourcing. Exit code: `$LASTEXITCODE" -ForegroundColor Cyan

# Don't fail on test failures during coverage collection
# We want the coverage data even if some tests fail
exit 0
"@ | Set-Content -Path $testRunnerScript

# Run coverlet to instrument and collect coverage
Write-Host ""
Write-Host "Running tests with coverlet instrumentation..." -ForegroundColor Cyan
$coverageOutput = Join-Path (Resolve-Path $OutputDir).Path "coverage"
$pwshPath = (Get-Command pwsh).Source

$coverletArgs = @(
    $cmdletsAssembly,
    "--target", $pwshPath,
    "--targetargs", "-NoProfile -File `"$testRunnerScript`"",
    "--format", "cobertura",
    "--format", "json",
    "--output", $coverageOutput,
    "--include", "[Rnwood.Dataverse.Data.PowerShell.Cmdlets]*",
    "--exclude", "[FakeXrmEasy*]*",
    "--exclude", "[*Tests*]*",
    "--exclude-by-attribute", "GeneratedCode",
    "--exclude-by-attribute", "ExcludeFromCodeCoverage",
    "--verbosity", "detailed"
)

Write-Host "Running: coverlet $cmdletsAssembly ..." -ForegroundColor Gray
Write-Host "Target: $pwshPath" -ForegroundColor Gray
Write-Host "Test script: $testRunnerScript" -ForegroundColor Gray

# Run from a temp directory to avoid auto-exclusions based on current directory structure
$originalDir = Get-Location
try {
    Set-Location -Path ([IO.Path]::GetTempPath())
    & coverlet @coverletArgs
    $coverletExitCode = $LASTEXITCODE
} finally {
    Set-Location -Path $originalDir
}

Write-Host ""
Write-Host "Coverlet exit code: $coverletExitCode" -ForegroundColor $(if ($coverletExitCode -eq 0) { "Green" } else { "Yellow" })

# Check for coverage file
$coberturaFile = "$coverageOutput.cobertura.xml"
if (-not (Test-Path $coberturaFile)) {
    Write-Warning "Coverage file not found at expected location: $coberturaFile"
    Write-Host "Searching for coverage files in output directory..." -ForegroundColor Yellow
    Get-ChildItem -Path $OutputDir -Filter "*.xml" -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Host "  Found: $($_.FullName)" -ForegroundColor Gray
    }
    $foundCoverage = Get-ChildItem -Path $OutputDir -Filter "*.cobertura.xml" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $foundCoverage) {
        Write-Host "Searching in entire workspace..." -ForegroundColor Yellow
        $foundCoverage = Get-ChildItem -Path . -Filter "*.cobertura.xml" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($foundCoverage) {
            Write-Host "  Found: $($foundCoverage.FullName)" -ForegroundColor Gray
        }
    }
    if ($foundCoverage) {
        Write-Host "Copying found coverage file to expected location..." -ForegroundColor Green
        Copy-Item $foundCoverage.FullName $coberturaFile
    } else {
        Write-Error "No coverage file generated - coverlet may have failed to instrument the assembly or tests may have failed to load the instrumented DLL"
        exit 1
    }
}

Write-Host ""
Write-Host "Coverage file: $coberturaFile" -ForegroundColor Green

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
