#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates that text contains a conventional commit message.

.DESCRIPTION
    Checks if the provided text (PR title, PR description, or commit message)
    contains at least one valid conventional commit message. Used to enforce
    conventional commits for automatic versioning.

.PARAMETER Text
    The text to validate (PR title, PR description, or commit message)

.EXAMPLE
    Test-ConventionalCommits -Text "feat: add new feature"
    Returns $true

.EXAMPLE
    Test-ConventionalCommits -Text "Random text without commits"
    Returns $false
#>

param(
    [Parameter(Mandatory = $true)]
    [AllowEmptyString()]
    [string]$Text
)

$ErrorActionPreference = "Stop"

# Check if text is empty or whitespace only
if ([string]::IsNullOrWhiteSpace($Text)) {
    Write-Host "ERROR: Text is empty or missing" -ForegroundColor Red
    return $false
}

# Split into lines
$lines = $Text -split "`n" | Where-Object { $_ -match '\S' }

# Look for at least one conventional commit message
$foundConventionalCommit = $false
$conventionalCommitPattern = '^\s*[-*+]?\s*(feat|fix|docs|style|refactor|perf|test|build|ci|chore)(\(.+?\))?!?:\s*.+'

foreach ($line in $lines) {
    if ($line -match $conventionalCommitPattern) {
        Write-Verbose "Found conventional commit: $line"
        $foundConventionalCommit = $true
        break
    }
}

if (-not $foundConventionalCommit) {
    Write-Host "ERROR: No conventional commit message found" -ForegroundColor Red
    Write-Host ""
    Write-Host "Text MUST contain at least one conventional commit message." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Required format: <type>(<scope>): <description>" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Valid types:" -ForegroundColor Yellow
    Write-Host "  - feat:     A new feature (minor version bump)" -ForegroundColor Yellow
    Write-Host "  - fix:      A bug fix (patch version bump)" -ForegroundColor Yellow
    Write-Host "  - docs:     Documentation changes" -ForegroundColor Yellow
    Write-Host "  - style:    Code style changes (formatting, etc.)" -ForegroundColor Yellow
    Write-Host "  - refactor: Code refactoring" -ForegroundColor Yellow
    Write-Host "  - perf:     Performance improvements" -ForegroundColor Yellow
    Write-Host "  - test:     Adding or updating tests" -ForegroundColor Yellow
    Write-Host "  - build:    Build system changes" -ForegroundColor Yellow
    Write-Host "  - ci:       CI/CD changes" -ForegroundColor Yellow
    Write-Host "  - chore:    Other changes" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  - feat: add batch delete operation" -ForegroundColor Yellow
    Write-Host "  - fix: resolve connection timeout" -ForegroundColor Yellow
    Write-Host "  - feat!: remove deprecated parameters" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "See CONTRIBUTING.md for full details." -ForegroundColor Yellow
    Write-Host ""
    return $false
}

Write-Host "✓ Found valid conventional commit message" -ForegroundColor Green
return $true
