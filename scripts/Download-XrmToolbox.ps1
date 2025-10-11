<#
.SYNOPSIS
    Downloads and extracts XrmToolbox for development and debugging.

.DESCRIPTION
    This script downloads the latest version of XrmToolbox from GitHub releases
    and extracts it to a local directory for development and debugging purposes.

.PARAMETER OutputPath
    The directory where XrmToolbox should be installed. Defaults to ./.xrmtoolbox

.PARAMETER Version
    Optional specific version to download. If not specified, downloads the latest.

.EXAMPLE
    .\Download-XrmToolbox.ps1
    Downloads the latest XrmToolbox to ./.xrmtoolbox

.EXAMPLE
    .\Download-XrmToolbox.ps1 -OutputPath "C:\Dev\XrmToolbox" -Version "1.2024.9.23"
    Downloads specific version to custom location
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ".xrmtoolbox",
    
    [Parameter(Mandatory=$false)]
    [string]$Version = $null
)

$ErrorActionPreference = "Stop"

Write-Host "XrmToolbox Download Script" -ForegroundColor Cyan
Write-Host "=" * 50

# Resolve output path
$OutputPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)

# Create output directory if it doesn't exist
if (-not (Test-Path $OutputPath)) {
    Write-Host "Creating output directory: $OutputPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

# Check if XrmToolbox.exe already exists
$xrmToolboxExe = Join-Path $OutputPath "XrmToolBox.exe"
if (Test-Path $xrmToolboxExe) {
    Write-Host "XrmToolbox already exists at: $xrmToolboxExe" -ForegroundColor Green
    Write-Host "To re-download, delete the directory first." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Executable path: $xrmToolboxExe" -ForegroundColor Cyan
    exit 0
}

Write-Host ""
Write-Host "Fetching XrmToolbox releases from GitHub..." -ForegroundColor Yellow

try {
    # Get releases from GitHub API
    $releasesUrl = "https://api.github.com/repos/MscrmTools/XrmToolBox/releases"
    $releases = Invoke-RestMethod -Uri $releasesUrl -UseBasicParsing
    
    if ($Version) {
        $release = $releases | Where-Object { $_.tag_name -eq $Version -or $_.name -eq $Version } | Select-Object -First 1
        if (-not $release) {
            Write-Error "Version $Version not found in releases"
            exit 1
        }
    } else {
        $release = $releases | Select-Object -First 1
    }
    
    Write-Host "Found release: $($release.name)" -ForegroundColor Green
    Write-Host "Published: $($release.published_at)" -ForegroundColor Gray
    Write-Host ""
    
    # Find the ZIP asset
    $zipAsset = $release.assets | Where-Object { $_.name -like "*.zip" -and $_.name -notlike "*symbols*" } | Select-Object -First 1
    
    if (-not $zipAsset) {
        Write-Error "No ZIP file found in release assets"
        exit 1
    }
    
    $downloadUrl = $zipAsset.browser_download_url
    $zipFileName = $zipAsset.name
    $zipPath = Join-Path $env:TEMP $zipFileName
    
    Write-Host "Downloading: $zipFileName" -ForegroundColor Yellow
    Write-Host "From: $downloadUrl" -ForegroundColor Gray
    Write-Host "Size: $([math]::Round($zipAsset.size / 1MB, 2)) MB" -ForegroundColor Gray
    Write-Host ""
    
    # Download with progress
    $ProgressPreference = 'SilentlyContinue'
    Invoke-WebRequest -Uri $downloadUrl -OutFile $zipPath -UseBasicParsing
    $ProgressPreference = 'Continue'
    
    Write-Host "Download complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Extracting to: $OutputPath" -ForegroundColor Yellow
    
    # Extract ZIP
    Expand-Archive -Path $zipPath -DestinationPath $OutputPath -Force
    
    # Clean up ZIP
    Remove-Item $zipPath -Force
    
    Write-Host "Extraction complete!" -ForegroundColor Green
    Write-Host ""
    
    # Verify XrmToolBox.exe exists
    if (Test-Path $xrmToolboxExe) {
        Write-Host "SUCCESS: XrmToolbox installed successfully" -ForegroundColor Green
        Write-Host ""
        Write-Host "Executable path: $xrmToolboxExe" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "You can now use this path in your Visual Studio or VS Code launch configurations." -ForegroundColor Yellow
    } else {
        Write-Warning "XrmToolBox.exe not found in extracted files. The ZIP structure may have changed."
        Write-Host "Contents of $OutputPath :" -ForegroundColor Yellow
        Get-ChildItem $OutputPath | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
    }
    
} catch {
    Write-Error "Failed to download or extract XrmToolbox: $_"
    exit 1
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Cyan
