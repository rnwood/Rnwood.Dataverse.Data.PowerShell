#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Tests the version calculation logic from the workflow

.DESCRIPTION
    Simulates the Build step of the publish.yml workflow to test
    conventional commit parsing and version calculation
#>

$ErrorActionPreference = "Stop"

Write-Host "=== Testing Workflow Version Logic ===" -ForegroundColor Cyan
Write-Host ""

# Simulate different scenarios
$testCases = @(
    @{
        Name = "feat commit (minor bump)"
        PRBody = @"
## Description
Add new feature

## Conventional Commits
- feat: add batch operations support
"@
        BaseVersion = "1.4.0"
        ExpectedVersion = "1.5.0"
    },
    @{
        Name = "fix commit (patch bump)"
        PRBody = @"
## Description
Bug fix

## Conventional Commits
- fix: resolve connection timeout
"@
        BaseVersion = "1.4.0"
        ExpectedVersion = "1.4.1"
    },
    @{
        Name = "breaking change (major bump)"
        PRBody = @"
## Description
Breaking changes

## Conventional Commits
- feat!: remove deprecated parameters

BREAKING CHANGE: removed old API
"@
        BaseVersion = "1.4.0"
        ExpectedVersion = "2.0.0"
    },
    @{
        Name = "multiple commits (highest wins)"
        PRBody = @"
## Conventional Commits
- fix: bug fix
- feat: new feature
- docs: update docs
"@
        BaseVersion = "1.4.0"
        ExpectedVersion = "1.5.0"
    },
    @{
        Name = "no conventional commits (defaults to patch)"
        PRBody = @"
## Description
Some changes

Just regular description without conventional commits format
"@
        BaseVersion = "1.4.0"
        ExpectedVersion = "1.4.1"
    },
    @{
        Name = "breaking change with existing prerelease major bump (should not increment again)"
        PRBody = @"
## Conventional Commits
- feat!: another breaking change
"@
        BaseVersion = "1.4.0"
        ExistingPrereleases = @("2.0.0-ci20241101001")
        ExpectedVersion = "2.0.0"
    },
    @{
        Name = "feature with existing prerelease major bump (major is higher, keep it)"
        PRBody = @"
## Conventional Commits
- feat: new feature
"@
        BaseVersion = "1.4.0"
        ExistingPrereleases = @("2.0.0-ci20241101001")
        ExpectedVersion = "2.0.0"
    },
    @{
        Name = "breaking change with existing prerelease minor bump (should bump to major)"
        PRBody = @"
## Conventional Commits
- feat!: breaking change
"@
        BaseVersion = "1.4.0"
        ExistingPrereleases = @("1.5.0-ci20241101001")
        ExpectedVersion = "2.0.0"
    },
    @{
        Name = "feature with existing prerelease minor bump (should not increment again)"
        PRBody = @"
## Conventional Commits
- feat: another feature
"@
        BaseVersion = "1.4.0"
        ExistingPrereleases = @("1.5.0-ci20241101001")
        ExpectedVersion = "1.5.0"
    },
    @{
        Name = "fix with existing prerelease minor bump (minor is higher, keep it)"
        PRBody = @"
## Conventional Commits
- fix: bug fix
"@
        BaseVersion = "1.4.0"
        ExistingPrereleases = @("1.5.0-ci20241101001")
        ExpectedVersion = "1.5.0"
    },
    @{
        Name = "multiple prereleases, use highest"
        PRBody = @"
## Conventional Commits
- feat: new feature
"@
        BaseVersion = "1.4.0"
        ExistingPrereleases = @("1.5.0-ci20241101001", "1.4.1-ci20241102001", "1.4.2-ci20241103001")
        ExpectedVersion = "1.5.0"
    }
)

$passed = 0
$failed = 0

foreach ($test in $testCases) {
    Write-Host "Test: $($test.Name)" -ForegroundColor Yellow
    Write-Host "  Base Version: $($test.BaseVersion)"
    if ($test.ExistingPrereleases) {
        Write-Host "  Existing Prereleases: $($test.ExistingPrereleases -join ', ')"
    }
    
    # Parse PR body into lines
    $commitMessages = $test.PRBody -split "`n" | Where-Object { $_ -match '\S' }
    
    # Call Get-NextVersion with optional prereleases
    $params = @{
        BaseVersion = $test.BaseVersion
        CommitMessages = $commitMessages
    }
    if ($test.ExistingPrereleases) {
        $params.ExistingPrereleases = $test.ExistingPrereleases
    }
    $result = & ./scripts/Get-NextVersion.ps1 @params
    
    Write-Host "  Calculated Version: $result"
    Write-Host "  Expected Version: $($test.ExpectedVersion)"
    
    if ($result -eq $test.ExpectedVersion) {
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
Write-Host "All tests passed!" -ForegroundColor Green
