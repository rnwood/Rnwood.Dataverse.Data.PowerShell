#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds the Sql4Cds submodule and creates a local NuGet package.

.DESCRIPTION
    This script initializes the Sql4Cds git submodule, builds the MarkMpn.Sql4Cds.Engine project,
    and creates a NuGet package in the local-packages directory. This package is required for
    building the main project.

.PARAMETER Configuration
    The build configuration (Debug or Release). Defaults to Release.

.PARAMETER Force
    Force rebuild even if the package already exists.

.EXAMPLE
    .\Build-Sql4Cds.ps1
    Builds Sql4Cds in Release configuration.

.EXAMPLE
    .\Build-Sql4Cds.ps1 -Configuration Debug -Force
    Rebuilds Sql4Cds in Debug configuration even if package exists.
#>
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [Parameter()]
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

# Get the root directory (where this script is located)
$RootDir = $PSScriptRoot
$Sql4CdsDir = Join-Path $RootDir "Sql4Cds"
$LocalPackagesDir = Join-Path $RootDir "local-packages"
$ProjectFile = Join-Path $Sql4CdsDir "MarkMpn.Sql4Cds.Engine" "MarkMpn.Sql4Cds.Engine.csproj"
$PackageFile = Join-Path $LocalPackagesDir "MarkMpn.Sql4Cds.Engine.1.0.0.nupkg"

Write-Host "Building Sql4Cds Engine..." -ForegroundColor Cyan

# Create local-packages directory if it doesn't exist
if (-not (Test-Path $LocalPackagesDir)) {
    Write-Host "Creating local-packages directory..." -ForegroundColor Yellow
    New-Item -Path $LocalPackagesDir -ItemType Directory | Out-Null
}

# Check if package already exists
if ((Test-Path $PackageFile) -and -not $Force) {
    Write-Host "Package already exists at: $PackageFile" -ForegroundColor Green
    Write-Host "Use -Force to rebuild." -ForegroundColor Yellow
    exit 0
}

# Initialize and update the Sql4Cds submodule
Write-Host "Checking Sql4Cds submodule..." -ForegroundColor Yellow
Push-Location $RootDir
try {
    # Check if submodule is initialized
    $submoduleStatus = git submodule status Sql4Cds
    if ($submoduleStatus -match "^-") {
        Write-Host "Initializing Sql4Cds submodule..." -ForegroundColor Yellow
        git submodule update --init --recursive Sql4Cds
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to initialize Sql4Cds submodule"
        }
    } else {
        Write-Host "Sql4Cds submodule already initialized" -ForegroundColor Green
    }
} finally {
    Pop-Location
}

# Verify the project file exists
if (-not (Test-Path $ProjectFile)) {
    throw "Sql4Cds project file not found at: $ProjectFile"
}

# Build the Sql4Cds.Engine project
Write-Host "Building MarkMpn.Sql4Cds.Engine ($Configuration)..." -ForegroundColor Yellow
dotnet build "$ProjectFile" -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "Failed to build Sql4Cds.Engine"
}

# Create NuGet package
Write-Host "Creating NuGet package..." -ForegroundColor Yellow
dotnet pack "$ProjectFile" -c $Configuration -o "$LocalPackagesDir" --no-build
if ($LASTEXITCODE -ne 0) {
    throw "Failed to create NuGet package"
}

Write-Host ""
Write-Host "Successfully built Sql4Cds and created package at: $PackageFile" -ForegroundColor Green
Write-Host "You can now build the main project." -ForegroundColor Green
