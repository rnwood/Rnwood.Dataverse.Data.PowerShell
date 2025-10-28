#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs tests with code coverage by building with coverlet.msbuild instrumentation
.DESCRIPTION
    Builds the cmdlets project with coverlet.msbuild instrumentation, then runs tests
    to collect coverage. This approach ensures the instrumented DLL is what gets loaded.
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

Write-Host "=== Running Tests with Code Coverage (coverlet.msbuild) ===" -ForegroundColor Cyan

# Create output directory
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# Add coverlet.msbuild to the cmdlets project temporarily
Write-Host "Adding coverlet.msbuild package..." -ForegroundColor Yellow
$cmdletsProject = "Rnwood.Dataverse.Data.PowerShell.Cmdlets/Rnwood.Dataverse.Data.PowerShell.Cmdlets.csproj"
dotnet add $cmdletsProject package coverlet.msbuild --version 6.0.2
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to add coverlet.msbuild package"
    exit 1
}

# Build with coverage instrumentation
Write-Host "Building with code coverage instrumentation..." -ForegroundColor Yellow
$coverageFile = (Resolve-Path $OutputDir).Path + "/coverage.cobertura.xml"
$buildArgs = @(
    "build",
    "Rnwood.Dataverse.Data.PowerShell/Rnwood.Dataverse.Data.PowerShell.csproj",
    "-c", "Release",
    "/p:CollectCoverage=true",
    "/p:CoverletOutputFormat=cobertura",
    "/p:CoverletOutput=$coverageFile",
    "/p:ExcludeByAttribute=GeneratedCode%2cExcludeFromCodeCoverage",
    "/p:Exclude=[FakeXrmEasy*]*%2c[*Tests*]*"
)

dotnet @buildArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build with coverage failed"
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

# Install Pester if needed
Write-Host "Ensuring Pester is installed..." -ForegroundColor Yellow
Install-Module -Force -Scope CurrentUser -SkipPublisherCheck Pester -MinimumVersion 5.0.0 -MaximumVersion 5.99.99 -ErrorAction SilentlyContinue

# Run tests (coverage data is collected automatically via instrumented DLL)
Write-Host ""
Write-Host "Running tests..." -ForegroundColor Cyan

$config = New-PesterConfiguration
$config.Run.Path = $TestPath
$config.Run.PassThru = $true
$config.Run.Exit = $false
$config.Output.Verbosity = 'Normal'
$config.Should.ErrorAction = 'Continue'

$result = Invoke-Pester -Configuration $config

Write-Host ""
Write-Host "Test Results:" -ForegroundColor Cyan
Write-Host "  Total:   $($result.TotalCount)" -ForegroundColor White
Write-Host "  Passed:  $($result.PassedCount)" -ForegroundColor Green
Write-Host "  Failed:  $($result.FailedCount)" -ForegroundColor $(if ($result.FailedCount -gt 0) { "Red" } else { "Green" })

$testExitCode = if ($result.FailedCount -gt 0) { 1 } else { 0 }

# Check for coverage file
$coberturaFile = "$OutputDir/coverage.cobertura.xml"
if (-not (Test-Path $coberturaFile)) {
    Write-Warning "Coverage file not found at expected location: $coberturaFile"
    Write-Host "Searching for coverage files..." -ForegroundColor Yellow
    $foundCoverage = Get-ChildItem -Path . -Filter "coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($foundCoverage) {
        Write-Host "Found coverage file at: $($foundCoverage.FullName)" -ForegroundColor Green
        Copy-Item $foundCoverage.FullName $coberturaFile
    } else {
        Write-Error "No coverage file generated"
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
