#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Tests the PR validation logic from the workflow

.DESCRIPTION
    Simulates the workflow's PR validation to ensure it correctly
    fails for missing conventional commits
#>

$ErrorActionPreference = "Stop"

Write-Host "=== Testing PR Validation Logic ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Valid PR description
Write-Host "Test 1: Valid PR description with conventional commits" -ForegroundColor Yellow
$validPR = @"
## Description
Add new features

## Conventional Commits
- feat: add batch delete operation
- fix: resolve connection timeout
"@

try {
    $isValid = & ./scripts/Test-ConventionalCommits.ps1 -PRDescription $validPR
    if ($isValid) {
        Write-Host "✓ Valid PR passed validation" -ForegroundColor Green
    } else {
        Write-Host "✗ Valid PR failed validation (unexpected)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Valid PR threw exception (unexpected): $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Invalid PR description (no conventional commits)
Write-Host "Test 2: Invalid PR description without conventional commits" -ForegroundColor Yellow
$invalidPR = @"
## Description
Just some changes

No conventional commits here.
"@

try {
    $isValid = & ./scripts/Test-ConventionalCommits.ps1 -PRDescription $invalidPR
    if (-not $isValid) {
        Write-Host "✓ Invalid PR correctly failed validation" -ForegroundColor Green
    } else {
        Write-Host "✗ Invalid PR passed validation (unexpected)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Invalid PR threw exception (unexpected): $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 3: Empty PR description
Write-Host "Test 3: Empty PR description" -ForegroundColor Yellow
$emptyPR = ""

try {
    $isValid = & ./scripts/Test-ConventionalCommits.ps1 -PRDescription $emptyPR
    if (-not $isValid) {
        Write-Host "✓ Empty PR correctly failed validation" -ForegroundColor Green
    } else {
        Write-Host "✗ Empty PR passed validation (unexpected)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Empty PR threw exception (unexpected): $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 4: Simulate workflow behavior with error throwing
Write-Host "Test 4: Workflow error handling simulation" -ForegroundColor Yellow
try {
    $isValid = & ./scripts/Test-ConventionalCommits.ps1 -PRDescription "No commits here"
    if (-not $isValid) {
        Write-Host "Simulating workflow behavior: throwing error..." -ForegroundColor Yellow
        throw "PR description validation failed - missing conventional commit messages"
    }
} catch {
    Write-Host "✓ Workflow correctly throws error: $_" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== All PR Validation Tests Passed ===" -ForegroundColor Green
