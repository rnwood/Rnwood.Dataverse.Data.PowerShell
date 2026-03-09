#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Tests the prerelease filtering logic in the workflow

.DESCRIPTION
    Validates that the workflow correctly filters out prerelease tags that are
    created before the stable release tag, preventing incorrect version inheritance.
#>

$ErrorActionPreference = "Stop"

Write-Host "=== Testing Prerelease Filtering Logic ===" -ForegroundColor Cyan
Write-Host ""

# Get current state
$latestStableTag = git tag --list --sort=-version:refname 2>$null | 
    Where-Object { $_ -notmatch '-' } | 
    Select-Object -First 1

Write-Host "Latest stable release: $latestStableTag" -ForegroundColor Green
Write-Host ""

# Test scenarios
$testScenarios = @(
    @{
        Name = "Prerelease created BEFORE stable tag should be excluded"
        Tag = "v3.0.0-ci202511172077"
        ExpectedResult = "Excluded"
    },
    @{
        Name = "Prerelease created AFTER stable tag should be included"
        Tag = "v3.0.0-ci202511192146"
        ExpectedResult = "Included"
    }
)

$passed = 0
$failed = 0

foreach ($scenario in $testScenarios) {
    Write-Host "Test: $($scenario.Name)" -ForegroundColor Yellow
    Write-Host "  Tag: $($scenario.Tag)"
    
    # Check if tag exists
    $tagExists = git tag --list $scenario.Tag 2>$null
    
    if (-not $tagExists) {
        Write-Host "  Result: Tag not found (skipping test)" -ForegroundColor Gray
        Write-Host ""
        continue
    }
    
    # Apply the workflow filtering logic
    $tagCommit = git rev-list -n 1 $scenario.Tag 2>$null
    $stableCommit = git rev-list -n 1 $latestStableTag 2>$null
    
    $isIncluded = $false
    
    if ($tagCommit -and $stableCommit) {
        # Check if stable tag is an ancestor of this prerelease tag
        git merge-base --is-ancestor $stableCommit $tagCommit 2>$null
        $isAfterStable = $LASTEXITCODE -eq 0
        
        # Also verify the prerelease is not the same commit as the stable tag
        $isSameCommit = $tagCommit -eq $stableCommit
        
        if ($isAfterStable -and -not $isSameCommit) {
            $isIncluded = $true
        }
    }
    
    $actualResult = if ($isIncluded) { "Included" } else { "Excluded" }
    
    Write-Host "  Expected: $($scenario.ExpectedResult)"
    Write-Host "  Actual: $actualResult"
    
    if ($actualResult -eq $scenario.ExpectedResult) {
        Write-Host "  ✓ PASSED" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "  ✗ FAILED" -ForegroundColor Red
        $failed++
    }
    
    Write-Host ""
}

Write-Host "=== Test Summary ===" -ForegroundColor Cyan
Write-Host "  Passed: $passed" -ForegroundColor Green
Write-Host "  Failed: $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Green" })

if ($failed -gt 0) {
    exit 1
}

Write-Host ""
Write-Host "All prerelease filtering tests passed!" -ForegroundColor Green
