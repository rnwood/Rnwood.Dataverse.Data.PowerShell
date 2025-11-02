#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Simulates the workflow Build step to validate the logic

.DESCRIPTION
    This script simulates the GitHub Actions workflow Build step
    to validate that version calculation from conventional commits works correctly
#>

$ErrorActionPreference = "Stop"

Write-Host "=== Simulating Workflow Build Step ===" -ForegroundColor Cyan
Write-Host ""

# Create a mock event.json file to simulate GitHub Actions PR event
$mockEventJson = @{
    pull_request = @{
        body = @"
## Description
Add new features and fix bugs

## Conventional Commits
- feat: add batch delete operation
- fix: resolve connection timeout
- docs: update documentation
"@
    }
} | ConvertTo-Json -Depth 5

$eventPath = "/tmp/mock-event.json"
$mockEventJson | Set-Content -Path $eventPath -Encoding UTF8

# Set environment variables
$env:GITHUB_EVENT_NAME = "pull_request"
$env:GITHUB_EVENT_PATH = $eventPath
$env:GITHUB_RUN_NUMBER = "123"
$manifestpath = "Rnwood.Dataverse.Data.PowerShell/Rnwood.Dataverse.Data.PowerShell.psd1"

Write-Host "Simulating PR build with event:"
Write-Host (Get-Content $eventPath -Raw)
Write-Host ""

# Simulate the workflow logic
$latestTag = "1.4.0"
$baseVersion = $latestTag

Write-Host "Latest tag version: $baseVersion"

# For PRs, analyze PR description for conventional commits
Write-Host "PR detected - analyzing PR description for conventional commits"

# Get PR body from GitHub event
$event = Get-Content $eventPath -Raw | ConvertFrom-Json
$prBody = $event.pull_request.body

if ($prBody) {
    Write-Host "PR Description:"
    Write-Host $prBody
    Write-Host ""
    
    # Split PR body into lines for analysis
    $commitMessages = $prBody -split "`n" | Where-Object { $_ -match '\S' }
    
    # Use Get-NextVersion script to determine version bump
    $nextVersion = & ./scripts/Get-NextVersion.ps1 -BaseVersion $baseVersion -CommitMessages $commitMessages
    Write-Host "Next version determined from PR description: $nextVersion"
    $baseVersion = $nextVersion
} else {
    Write-Host "No PR description found, defaulting to patch bump"
}

# Get current date in yyyyMMdd format (UTC)
$nowUtc = [DateTime]::UtcNow
$dateString = $nowUtc.ToString("yyyyMMdd")

$runNumber = $env:GITHUB_RUN_NUMBER
if (-not $runNumber) {
    $runNumber = 1
}

# Build CI version with date-based prerelease suffix
$seqNum = "{0:000}" -f [int]$runNumber
$prereleaseString = "ci$dateString$seqNum"

$ciVersion = "$baseVersion-$prereleaseString"

Write-Host ""
Write-Host "=== Result ===" -ForegroundColor Green
Write-Host "Base Version: $baseVersion"
Write-Host "CI Version: $ciVersion"
Write-Host ""

# Validate the version is greater than 1.4.0
if ($baseVersion -eq "1.5.0") {
    Write-Host "✓ Version correctly bumped from 1.4.0 to 1.5.0 (minor bump due to feat:)" -ForegroundColor Green
} elseif ($baseVersion -eq "1.4.1") {
    Write-Host "⚠ Version bumped to 1.4.1 (patch bump) - expected 1.5.0 due to feat:" -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "✗ Unexpected version: $baseVersion" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✓ Workflow simulation successful!" -ForegroundColor Green

# Cleanup
Remove-Item $eventPath -Force
