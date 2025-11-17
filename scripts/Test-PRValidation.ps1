#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Tests the PR validation logic from the workflow

.DESCRIPTION
    Simulates the workflow's PR validation to ensure it correctly
    validates PR titles for conventional commits
#>

$ErrorActionPreference = "Stop"

Write-Host "=== Testing PR Validation Logic ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Valid PR title
Write-Host "Test 1: Valid PR title with conventional commit" -ForegroundColor Yellow
$validTitle = "feat: add batch delete operation"

try {
    $isValid = & ./scripts/Test-ConventionalCommits.ps1 -Text $validTitle
    if ($isValid) {
        Write-Host "✓ Valid PR title passed validation" -ForegroundColor Green
    } else {
        Write-Host "✗ Valid PR title failed validation (unexpected)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Valid PR title threw exception (unexpected): $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Another valid PR title with scope
Write-Host "Test 2: Valid PR title with scope" -ForegroundColor Yellow
$validTitleWithScope = "fix(auth): resolve connection timeout issue"

try {
    $isValid = & ./scripts/Test-ConventionalCommits.ps1 -Text $validTitleWithScope
    if ($isValid) {
        Write-Host "✓ Valid PR title with scope passed validation" -ForegroundColor Green
    } else {
        Write-Host "✗ Valid PR title with scope failed validation (unexpected)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Valid PR title with scope threw exception (unexpected): $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 3: Invalid PR title (no conventional commit)
Write-Host "Test 3: Invalid PR title without conventional commit" -ForegroundColor Yellow
$invalidTitle = "Just some changes to the code"

try {
    $isValid = & ./scripts/Test-ConventionalCommits.ps1 -Text $invalidTitle
    if (-not $isValid) {
        Write-Host "✓ Invalid PR title correctly failed validation" -ForegroundColor Green
    } else {
        Write-Host "✗ Invalid PR title passed validation (unexpected)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Invalid PR title threw exception (unexpected): $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 4: Empty PR title
Write-Host "Test 4: Empty PR title" -ForegroundColor Yellow
$emptyTitle = ""

try {
    $isValid = & ./scripts/Test-ConventionalCommits.ps1 -Text $emptyTitle
    if (-not $isValid) {
        Write-Host "✓ Empty PR title correctly failed validation" -ForegroundColor Green
    } else {
        Write-Host "✗ Empty PR title passed validation (unexpected)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Empty PR title threw exception (unexpected): $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 5: Breaking change in title
Write-Host "Test 5: PR title with breaking change" -ForegroundColor Yellow
$breakingTitle = "feat!: remove deprecated cmdlet parameters"

try {
    $isValid = & ./scripts/Test-ConventionalCommits.ps1 -Text $breakingTitle
    if ($isValid) {
        Write-Host "✓ Breaking change PR title passed validation" -ForegroundColor Green
    } else {
        Write-Host "✗ Breaking change PR title failed validation (unexpected)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Breaking change PR title threw exception (unexpected): $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 6: Breaking change with all conventional commit types
Write-Host "Test 6: Breaking changes with all types (! should work for all)" -ForegroundColor Yellow
$allTypes = @("feat", "fix", "docs", "style", "refactor", "perf", "test", "build", "ci", "chore")
$allTestsPassed = $true

foreach ($type in $allTypes) {
    $testTitle = "$type!: breaking change for $type"
    try {
        $isValid = & ./scripts/Test-ConventionalCommits.ps1 -Text $testTitle
        if ($isValid) {
            Write-Host "  ✓ $type!: passed validation" -ForegroundColor Green
        } else {
            Write-Host "  ✗ $type!: failed validation (unexpected)" -ForegroundColor Red
            $allTestsPassed = $false
        }
    } catch {
        Write-Host "  ✗ $type!: threw exception (unexpected): $_" -ForegroundColor Red
        $allTestsPassed = $false
    }
}

if (-not $allTestsPassed) {
    Write-Host ""
    Write-Host "✗ Some breaking change type tests failed" -ForegroundColor Red
    exit 1
}

Write-Host "✓ All breaking change types passed validation" -ForegroundColor Green

Write-Host ""

# Test 7: Breaking change with scope for all types
Write-Host "Test 7: Breaking changes with scope (all types)" -ForegroundColor Yellow
$allTestsPassed = $true

foreach ($type in $allTypes) {
    $testTitle = "$type(scope)!: breaking change for $type with scope"
    try {
        $isValid = & ./scripts/Test-ConventionalCommits.ps1 -Text $testTitle
        if ($isValid) {
            Write-Host "  ✓ $type(scope)!: passed validation" -ForegroundColor Green
        } else {
            Write-Host "  ✗ $type(scope)!: failed validation (unexpected)" -ForegroundColor Red
            $allTestsPassed = $false
        }
    } catch {
        Write-Host "  ✗ $type(scope)!: threw exception (unexpected): $_" -ForegroundColor Red
        $allTestsPassed = $false
    }
}

if (-not $allTestsPassed) {
    Write-Host ""
    Write-Host "✗ Some breaking change with scope tests failed" -ForegroundColor Red
    exit 1
}

Write-Host "✓ All breaking change types with scope passed validation" -ForegroundColor Green

Write-Host ""

# Test 8: Simulate workflow behavior with error throwing
Write-Host "Test 8: Workflow error handling simulation" -ForegroundColor Yellow
try {
    $isValid = & ./scripts/Test-ConventionalCommits.ps1 -Text "No commits here"
    if (-not $isValid) {
        Write-Host "Simulating workflow behavior: throwing error..." -ForegroundColor Yellow
        throw "PR title validation failed - missing conventional commit message"
    }
} catch {
    Write-Host "✓ Workflow correctly throws error: $_" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== All PR Validation Tests Passed ===" -ForegroundColor Green
