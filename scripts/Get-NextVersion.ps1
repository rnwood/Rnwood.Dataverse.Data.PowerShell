#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Determines the next version number based on conventional commits.

.DESCRIPTION
    Parses conventional commit messages from a PR description or commit history
    and determines the appropriate version bump (major, minor, or patch).
    
    Conventional commit rules:
    - feat: or feat(<scope>): → minor version bump
    - fix: or fix(<scope>): → patch version bump
    - feat!: or fix!: or BREAKING CHANGE: → major version bump
    - Other types (docs, chore, style, refactor, test, etc.) → patch version bump
    
    If no conventional commits are found, defaults to patch version bump.

.PARAMETER BaseVersion
    The current/base version (e.g., "1.4.0")

.PARAMETER CommitMessages
    Array of commit messages or PR description text to analyze

.EXAMPLE
    Get-NextVersion -BaseVersion "1.4.0" -CommitMessages @("feat: add new feature", "fix: bug fix")
    Returns "1.5.0" (minor bump due to feat:)

.EXAMPLE
    Get-NextVersion -BaseVersion "1.4.0" -CommitMessages @("fix: bug fix")
    Returns "1.4.1" (patch bump)

.EXAMPLE
    Get-NextVersion -BaseVersion "1.4.0" -CommitMessages @("feat!: breaking change")
    Returns "2.0.0" (major bump)
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$BaseVersion,
    
    [Parameter(Mandatory = $true)]
    [AllowEmptyCollection()]
    [string[]]$CommitMessages
)

$ErrorActionPreference = "Stop"

# Parse base version
if ($BaseVersion -match '^v?(\d+)\.(\d+)\.(\d+)') {
    $major = [int]$matches[1]
    $minor = [int]$matches[2]
    $patch = [int]$matches[3]
} else {
    throw "Invalid base version format: $BaseVersion. Expected format: x.y.z or vx.y.z"
}

Write-Verbose "Base version: $major.$minor.$patch"

# Determine the highest bump level needed
$bumpLevel = "none" # none, patch, minor, major

foreach ($message in $CommitMessages) {
    # Skip empty messages
    if ([string]::IsNullOrWhiteSpace($message)) {
        continue
    }
    
    Write-Verbose "Analyzing commit: $message"
    
    # Normalize the message - remove leading list markers (-, *, +) and whitespace
    $normalizedMessage = $message -replace '^\s*[-*+]\s*', ''
    
    # Check for breaking changes (highest priority - major bump)
    # More explicit regex matching only valid conventional commit types
    if ($normalizedMessage -match '^(feat|fix|docs|style|refactor|perf|test|build|ci|chore)(\(.+?\))?!:' -or $message -match '\bBREAKING[- ]CHANGE\b') {
        Write-Verbose "  -> Breaking change detected"
        $bumpLevel = "major"
        break  # Major is the highest, no need to check further
    }
    
    # Check for features (minor bump)
    if ($normalizedMessage -match '^feat(\(.+?\))?:') {
        Write-Verbose "  -> Feature detected"
        if ($bumpLevel -ne "major") {
            $bumpLevel = "minor"
        }
        continue
    }
    
    # Check for fixes and other types (patch bump)
    if ($normalizedMessage -match '^(fix|docs|chore|style|refactor|perf|test|build|ci)(\(.+?\))?:') {
        Write-Verbose "  -> Fix or other conventional commit detected"
        if ($bumpLevel -eq "none") {
            $bumpLevel = "patch"
        }
        continue
    }
}

# Default to patch if no conventional commits found
if ($bumpLevel -eq "none") {
    Write-Verbose "No conventional commits found, defaulting to patch bump"
    $bumpLevel = "patch"
}

# Calculate new version
switch ($bumpLevel) {
    "major" {
        $newMajor = $major + 1
        $newMinor = 0
        $newPatch = 0
    }
    "minor" {
        $newMajor = $major
        $newMinor = $minor + 1
        $newPatch = 0
    }
    "patch" {
        $newMajor = $major
        $newMinor = $minor
        $newPatch = $patch + 1
    }
}

$newVersion = "$newMajor.$newMinor.$newPatch"
Write-Verbose "New version: $newVersion (bump: $bumpLevel)"

return $newVersion
