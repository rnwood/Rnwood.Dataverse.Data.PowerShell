#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Determines the next version number based on conventional commits.

.DESCRIPTION
    Parses conventional commit messages from a PR title, PR description, or commit history
    and determines the appropriate version bump (major, minor, or patch).
    
    Takes into account existing prerelease versions to avoid double-incrementing when
    multiple PRs with similar changes are created since the last stable release.
    
    Conventional commit rules:
    - feat: or feat(<scope>): → minor version bump
    - fix: or fix(<scope>): → patch version bump
    - feat!: or fix!: or BREAKING CHANGE: → major version bump
    - Other types (docs, chore, style, refactor, test, etc.) → patch version bump
    
    If no conventional commits are found, defaults to patch version bump.

.PARAMETER BaseVersion
    The current/base stable version (e.g., "1.4.0")

.PARAMETER CommitMessages
    Array of commit messages, PR title, or PR description text to analyze

.PARAMETER ExistingPrereleases
    Array of existing prerelease versions since the base version (e.g., @("1.5.0-ci20241103001"))
    Used to prevent double-incrementing when multiple PRs have similar changes

.EXAMPLE
    Get-NextVersion -BaseVersion "1.4.0" -CommitMessages @("feat: add new feature")
    Returns "1.5.0" (minor bump due to feat:)

.EXAMPLE
    Get-NextVersion -BaseVersion "1.4.0" -CommitMessages @("fix: bug fix")
    Returns "1.4.1" (patch bump)

.EXAMPLE
    Get-NextVersion -BaseVersion "1.4.0" -CommitMessages @("feat!: breaking change")
    Returns "2.0.0" (major bump)

.EXAMPLE
    Get-NextVersion -BaseVersion "1.4.0" -CommitMessages @("feat!: breaking change") -ExistingPrereleases @("2.0.0-ci20241101001")
    Returns "2.0.0" (major already bumped by existing prerelease, no double increment)
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$BaseVersion,
    
    [Parameter(Mandatory = $true)]
    [AllowEmptyCollection()]
    [string[]]$CommitMessages,
    
    [Parameter(Mandatory = $false)]
    [AllowEmptyCollection()]
    [string[]]$ExistingPrereleases = @()
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
Write-Verbose "New version calculated from commits: $newVersion (bump: $bumpLevel)"

# Check if existing prereleases already achieved a higher or equal version
if ($ExistingPrereleases.Count -gt 0) {
    Write-Verbose "Checking existing prereleases..."
    
    $highestPrerelease = $null
    $highestPrereleaseVersion = $null
    
    foreach ($prerelease in $ExistingPrereleases) {
        # Extract version number from prerelease (e.g., "1.5.0-ci20241103001" -> "1.5.0")
        if ($prerelease -match '^v?(\d+)\.(\d+)\.(\d+)') {
            $preMajor = [int]$matches[1]
            $preMinor = [int]$matches[2]
            $prePatch = [int]$matches[3]
            
            Write-Verbose "  Prerelease: $prerelease -> $preMajor.$preMinor.$prePatch"
            
            # Compare versions (major.minor.patch)
            $isHigher = $false
            if ($null -eq $highestPrereleaseVersion) {
                $isHigher = $true
            } elseif ($preMajor -gt $highestPrereleaseVersion[0]) {
                $isHigher = $true
            } elseif ($preMajor -eq $highestPrereleaseVersion[0] -and $preMinor -gt $highestPrereleaseVersion[1]) {
                $isHigher = $true
            } elseif ($preMajor -eq $highestPrereleaseVersion[0] -and $preMinor -eq $highestPrereleaseVersion[1] -and $prePatch -gt $highestPrereleaseVersion[2]) {
                $isHigher = $true
            }
            
            if ($isHigher) {
                $highestPrerelease = $prerelease
                $highestPrereleaseVersion = @($preMajor, $preMinor, $prePatch)
            }
        }
    }
    
    if ($null -ne $highestPrereleaseVersion) {
        Write-Verbose "  Highest prerelease version: $($highestPrereleaseVersion -join '.')"
        
        # Compare with calculated new version
        $newIsHigher = $false
        if ($newMajor -gt $highestPrereleaseVersion[0]) {
            $newIsHigher = $true
        } elseif ($newMajor -eq $highestPrereleaseVersion[0] -and $newMinor -gt $highestPrereleaseVersion[1]) {
            $newIsHigher = $true
        } elseif ($newMajor -eq $highestPrereleaseVersion[0] -and $newMinor -eq $highestPrereleaseVersion[1] -and $newPatch -gt $highestPrereleaseVersion[2]) {
            $newIsHigher = $true
        }
        
        if ($newIsHigher) {
            Write-Verbose "  New version $newVersion is higher than existing prereleases, using it"
        } else {
            Write-Verbose "  Existing prerelease version is >= new version, using existing prerelease version"
            $newMajor = $highestPrereleaseVersion[0]
            $newMinor = $highestPrereleaseVersion[1]
            $newPatch = $highestPrereleaseVersion[2]
            $newVersion = "$newMajor.$newMinor.$newPatch"
        }
    }
}

Write-Verbose "Final version: $newVersion"

return $newVersion
