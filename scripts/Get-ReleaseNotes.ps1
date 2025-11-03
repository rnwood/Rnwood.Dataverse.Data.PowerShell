#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates release notes based on conventional commits between two versions.

.DESCRIPTION
    Analyzes commit messages between two git references and generates
    formatted release notes grouped by change type (Features, Bug Fixes, etc.).
    
    Supports conventional commit format and generates both markdown and
    plain text formats suitable for GitHub releases and PowerShell Gallery.

.PARAMETER FromRef
    Starting git reference (tag, commit, branch). For CI builds, this should
    be the last prerelease. For stable releases, this should be the last
    stable release.

.PARAMETER ToRef
    Ending git reference (tag, commit, branch). Defaults to HEAD.

.PARAMETER Format
    Output format: 'markdown' (default) or 'text'

.EXAMPLE
    Get-ReleaseNotes -FromRef "v1.4.0" -ToRef "v1.5.0"
    Generates release notes for commits between v1.4.0 and v1.5.0

.EXAMPLE
    Get-ReleaseNotes -FromRef "v1.4.0-ci20241101001" -Format text
    Generates text release notes from last CI build to HEAD
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$FromRef,
    
    [Parameter(Mandatory = $false)]
    [string]$ToRef = "HEAD",
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("markdown", "text")]
    [string]$Format = "markdown"
)

$ErrorActionPreference = "Stop"

# Get commits between the two references
$commits = git log "$FromRef..$ToRef" --format="%H|%s|%b" --no-merges 2>$null

if (-not $commits) {
    Write-Verbose "No commits found between $FromRef and $ToRef"
    return ""
}

# Parse commits and group by type
$features = @()
$fixes = @()
$breakingChanges = @()
$other = @()

foreach ($commitLine in $commits -split "`n`n") {
    if ([string]::IsNullOrWhiteSpace($commitLine)) {
        continue
    }
    
    $parts = $commitLine -split '\|', 3
    if ($parts.Count -lt 2) {
        continue
    }
    
    $hash = $parts[0].Substring(0, [Math]::Min(7, $parts[0].Length))
    $subject = $parts[1]
    $body = if ($parts.Count -gt 2) { $parts[2] } else { "" }
    
    # Normalize subject - remove leading list markers
    $normalizedSubject = $subject -replace '^\s*[-*+]\s*', ''
    
    # Check for breaking changes
    # Breaking change must be:
    # 1. In subject with ! before colon (e.g., "feat!: ...")
    # 2. OR in body as footer starting a line (e.g., "BREAKING CHANGE: ..." or "BREAKING-CHANGE: ...")
    $isBreaking = $normalizedSubject -match '^(feat|fix|docs|style|refactor|perf|test|build|ci|chore)(\(.+?\))?!:' -or 
                  $body -match '(?:^|\n)BREAKING[- ]CHANGE\s*:'
    
    if ($isBreaking) {
        # Extract the description
        $description = $normalizedSubject -replace '^[^:]+:\s*', ''
        $breakingChanges += @{
            Hash = $hash
            Description = $description
            Body = $body
        }
        continue
    }
    
    # Check for features
    if ($normalizedSubject -match '^feat(\(.+?\))?:\s*(.+)') {
        $description = $matches[2]
        $features += @{
            Hash = $hash
            Description = $description
        }
        continue
    }
    
    # Check for fixes
    if ($normalizedSubject -match '^fix(\(.+?\))?:\s*(.+)') {
        $description = $matches[2]
        $fixes += @{
            Hash = $hash
            Description = $description
        }
        continue
    }
    
    # Check for other conventional commit types
    if ($normalizedSubject -match '^(docs|chore|style|refactor|perf|test|build|ci)(\(.+?\))?:\s*(.+)') {
        $type = $matches[1]
        $description = $matches[3]
        $other += @{
            Hash = $hash
            Type = $type
            Description = $description
        }
        continue
    }
    
    # Non-conventional commits
    $other += @{
        Hash = $hash
        Type = "other"
        Description = $subject
    }
}

# Generate release notes
$releaseNotes = @()

if ($Format -eq "markdown") {
    # Markdown format for GitHub
    if ($breakingChanges.Count -gt 0) {
        $releaseNotes += "## ⚠️ BREAKING CHANGES"
        $releaseNotes += ""
        foreach ($change in $breakingChanges) {
            $releaseNotes += "- **$($change.Description)** ($($change.Hash))"
        }
        $releaseNotes += ""
    }
    
    if ($features.Count -gt 0) {
        $releaseNotes += "## ✨ Features"
        $releaseNotes += ""
        foreach ($feature in $features) {
            $releaseNotes += "- $($feature.Description) ($($feature.Hash))"
        }
        $releaseNotes += ""
    }
    
    if ($fixes.Count -gt 0) {
        $releaseNotes += "## 🐛 Bug Fixes"
        $releaseNotes += ""
        foreach ($fix in $fixes) {
            $releaseNotes += "- $($fix.Description) ($($fix.Hash))"
        }
        $releaseNotes += ""
    }
    
    if ($other.Count -gt 0) {
        $releaseNotes += "## 📝 Other Changes"
        $releaseNotes += ""
        foreach ($change in $other) {
            $typeLabel = switch ($change.Type) {
                "docs" { "📚 Documentation" }
                "chore" { "🔧 Chore" }
                "style" { "💄 Style" }
                "refactor" { "♻️ Refactor" }
                "perf" { "⚡ Performance" }
                "test" { "✅ Test" }
                "build" { "🏗️ Build" }
                "ci" { "👷 CI" }
                default { "Other" }
            }
            $releaseNotes += "- ${typeLabel}: $($change.Description) ($($change.Hash))"
        }
        $releaseNotes += ""
    }
} else {
    # Plain text format for PowerShell Gallery
    if ($breakingChanges.Count -gt 0) {
        $releaseNotes += "BREAKING CHANGES:"
        foreach ($change in $breakingChanges) {
            $releaseNotes += "  - $($change.Description)"
        }
        $releaseNotes += ""
    }
    
    if ($features.Count -gt 0) {
        $releaseNotes += "Features:"
        foreach ($feature in $features) {
            $releaseNotes += "  - $($feature.Description)"
        }
        $releaseNotes += ""
    }
    
    if ($fixes.Count -gt 0) {
        $releaseNotes += "Bug Fixes:"
        foreach ($fix in $fixes) {
            $releaseNotes += "  - $($fix.Description)"
        }
        $releaseNotes += ""
    }
    
    if ($other.Count -gt 0) {
        $releaseNotes += "Other Changes:"
        foreach ($change in $other) {
            $releaseNotes += "  - $($change.Description)"
        }
        $releaseNotes += ""
    }
}

# Join and return
$result = $releaseNotes -join "`n"
return $result.TrimEnd()
