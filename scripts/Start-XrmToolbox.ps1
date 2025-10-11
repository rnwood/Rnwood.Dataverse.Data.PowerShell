<#
.SYNOPSIS
    Starts XrmToolbox with the development plugin loaded.

.DESCRIPTION
    This script ensures XrmToolbox is downloaded, copies the plugin build output
    to the XrmToolbox Plugins folder, and starts XrmToolbox for debugging.

.PARAMETER XrmToolboxPath
    Path to XrmToolBox.exe. If not specified, uses .xrmtoolbox in the repo root.

.PARAMETER BuildConfiguration
    Build configuration to use (Debug or Release). Defaults to Debug.

.EXAMPLE
    .\Start-XrmToolbox.ps1
    Downloads XrmToolbox if needed, copies plugin, and starts XrmToolbox

.EXAMPLE
    .\Start-XrmToolbox.ps1 -BuildConfiguration Release
    Uses Release build of the plugin
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$XrmToolboxPath = $null,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$BuildConfiguration = "Debug"
)

$ErrorActionPreference = "Stop"

Write-Host "Starting XrmToolbox with Development Plugin" -ForegroundColor Cyan
Write-Host "=" * 60

# Get script directory and repo root
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir

# Determine XrmToolbox path
if (-not $XrmToolboxPath) {
    $XrmToolboxPath = Join-Path $repoRoot ".xrmtoolbox\XrmToolBox.exe"
}

Write-Host "XrmToolbox path: $XrmToolboxPath" -ForegroundColor Gray

# Download XrmToolbox if it doesn't exist
if (-not (Test-Path $XrmToolboxPath)) {
    Write-Host ""
    Write-Host "XrmToolbox not found. Downloading..." -ForegroundColor Yellow
    
    $downloadScript = Join-Path $scriptDir "Download-XrmToolbox.ps1"
    $xrmToolboxDir = Split-Path -Parent $XrmToolboxPath
    
    & powershell.exe -ExecutionPolicy Bypass -File $downloadScript -OutputPath $xrmToolboxDir
    
    if (-not (Test-Path $XrmToolboxPath)) {
        Write-Error "Failed to download XrmToolbox"
        exit 1
    }
}

Write-Host "Found XrmToolbox: $XrmToolboxPath" -ForegroundColor Green

# Get plugin build output path
$pluginProjectDir = Join-Path $repoRoot "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin"
$pluginBuildOutput = Join-Path $pluginProjectDir "bin\$BuildConfiguration\net48"

Write-Host ""
Write-Host "Plugin build output: $pluginBuildOutput" -ForegroundColor Gray

# Check if plugin is built
if (-not (Test-Path $pluginBuildOutput)) {
    Write-Host ""
    Write-Host "Plugin not built yet. Building..." -ForegroundColor Yellow
    
    $slnPath = Join-Path $repoRoot "Rnwood.Dataverse.Data.PowerShell.sln"
    & dotnet build $slnPath -c $BuildConfiguration
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed"
        exit 1
    }
}

$pluginDll = Join-Path $pluginBuildOutput "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin.dll"
if (-not (Test-Path $pluginDll)) {
    Write-Error "Plugin DLL not found at: $pluginDll"
    Write-Host "Please build the solution first:" -ForegroundColor Yellow
    Write-Host "  dotnet build Rnwood.Dataverse.Data.PowerShell.sln -c $BuildConfiguration"
    exit 1
}

Write-Host "Found plugin DLL: $pluginDll" -ForegroundColor Green

# Determine XrmToolbox Plugins folder
$xrmToolboxDir = Split-Path -Parent $XrmToolboxPath
$pluginsDir = Join-Path $xrmToolboxDir "Plugins"

# Create Plugins directory if it doesn't exist
if (-not (Test-Path $pluginsDir)) {
    Write-Host ""
    Write-Host "Creating Plugins directory: $pluginsDir" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $pluginsDir | Out-Null
}

# Create plugin-specific directory
$pluginTargetDir = Join-Path $pluginsDir "Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin"
if (-not (Test-Path $pluginTargetDir)) {
    New-Item -ItemType Directory -Path $pluginTargetDir | Out-Null
}

Write-Host ""
Write-Host "Copying plugin files to: $pluginTargetDir" -ForegroundColor Yellow

# Copy all files from build output to XrmToolbox Plugins folder
$filesToCopy = Get-ChildItem -Path $pluginBuildOutput -Recurse -File

foreach ($file in $filesToCopy) {
    $relativePath = $file.FullName.Substring($pluginBuildOutput.Length + 1)
    $targetPath = Join-Path $pluginTargetDir $relativePath
    $targetDir = Split-Path -Parent $targetPath
    
    if (-not (Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }
    
    Copy-Item -Path $file.FullName -Destination $targetPath -Force
    Write-Host "  Copied: $relativePath" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Plugin files copied successfully!" -ForegroundColor Green

Write-Host ""
Write-Host "Starting XrmToolbox..." -ForegroundColor Cyan
Write-Host ""
Write-Host "The plugin should appear in the Tools menu as 'PowerShell Console'" -ForegroundColor Yellow
Write-Host ""

# Start XrmToolbox
& $XrmToolboxPath

Write-Host ""
Write-Host "XrmToolbox closed." -ForegroundColor Gray
