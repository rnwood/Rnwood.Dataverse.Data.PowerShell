#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds and packs Sql4CDS submodule for local use

.DESCRIPTION
    This script builds the Sql4CDS Engine from the submodule and creates a local NuGet package.
    This is required before building the main solution because the Cmdlets project references
    the Sql4CDS package from the local-packages directory.

.EXAMPLE
    ./Build-Sql4Cds.ps1
#>

$ErrorActionPreference = "Stop"

# Resolve paths - PSScriptRoot is the scripts directory
$scriptsDir = $PSScriptRoot
$repoRoot = Split-Path -Parent $scriptsDir
$sql4CdsProject = Join-Path $repoRoot "Sql4Cds/MarkMpn.Sql4Cds.Engine/MarkMpn.Sql4Cds.Engine.csproj"
$localPackagesDir = Join-Path $repoRoot "local-packages"

Write-Host "Building Sql4CDS from submodule..." -ForegroundColor Cyan
Write-Host "Repository root: $repoRoot" -ForegroundColor Gray
Write-Host "Sql4CDS project: $sql4CdsProject" -ForegroundColor Gray

# Check if submodule is initialized
if (-not (Test-Path $sql4CdsProject)) {
    Write-Host "Sql4CDS submodule not found. Initializing submodule..." -ForegroundColor Yellow
    Push-Location $repoRoot
    try {
        git submodule update --init --recursive
    } finally {
        Pop-Location
    }
}

# Ensure local-packages directory exists
if (-not (Test-Path $localPackagesDir)) {
    New-Item -Path $localPackagesDir -ItemType Directory | Out-Null
}

# Build Sql4CDS in Release mode
Write-Host "Building Sql4CDS.Engine (Release)..." -ForegroundColor Green
dotnet build $sql4CdsProject -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build Sql4CDS.Engine"
    exit 1
}

# Pack Sql4CDS as NuGet package
Write-Host "Packing Sql4CDS.Engine..." -ForegroundColor Green
dotnet pack $sql4CdsProject -c Release --no-build -o $localPackagesDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to pack Sql4CDS.Engine"
    exit 1
}

Write-Host "Sql4CDS build and pack completed successfully!" -ForegroundColor Green
Write-Host "Package location: $localPackagesDir" -ForegroundColor Cyan
