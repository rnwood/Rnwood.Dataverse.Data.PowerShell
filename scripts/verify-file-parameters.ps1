#!/usr/bin/env pwsh
# Verification script for InFile/OutFile parameter functionality

$ErrorActionPreference = "Stop"

Write-Host "=== Verification of InFile/OutFile Parameter Enhancements ===" -ForegroundColor Cyan
Write-Host ""

# Set module path
$modulePath = Resolve-Path "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Release/netstandard2.0/Rnwood.Dataverse.Data.PowerShell.psd1"
Write-Host "Loading module from: $modulePath" -ForegroundColor Yellow
Import-Module $modulePath -Force

Write-Host ""
Write-Host "✓ Module loaded successfully" -ForegroundColor Green
Write-Host ""

# Verify import cmdlets have InFile parameter
Write-Host "Checking Import/Upload cmdlets for InFile parameter:" -ForegroundColor Cyan
$importCmdlets = @(
    "Invoke-DataverseImportSolution",
    "Invoke-DataverseImportSolutionAsync",
    "Invoke-DataverseStageSolution",
    "Invoke-DataverseStageAndUpgrade",
    "Invoke-DataverseStageAndUpgradeAsync",
    "Invoke-DataverseRetrieveMissingComponents",
    "Invoke-DataverseImportTranslation",
    "Invoke-DataverseImportTranslationAsync",
    "Invoke-DataverseImportFieldTranslation",
    "Invoke-DataverseUploadBlock",
    "Invoke-DataverseImportSolutions"
)

$passCount = 0
foreach ($cmdletName in $importCmdlets) {
    $cmd = Get-Command $cmdletName -ErrorAction Stop
    $hasInFile = $cmd.Parameters.ContainsKey('InFile')
    
    if ($hasInFile) {
        $paramSets = $cmd.Parameters['InFile'].ParameterSets.Keys
        $hasFromFileSet = $paramSets -contains 'FromFile'
        
        if ($hasFromFileSet) {
            Write-Host "  ✓ $cmdletName" -ForegroundColor Green
            $passCount++
        } else {
            Write-Host "  ✗ $cmdletName (missing FromFile parameter set)" -ForegroundColor Red
        }
    } else {
        Write-Host "  ✗ $cmdletName (missing InFile parameter)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Checking Export cmdlets for OutFile parameter:" -ForegroundColor Cyan
$exportCmdlets = @(
    "Invoke-DataverseExportSolution",
    "Invoke-DataverseExportTranslation",
    "Invoke-DataverseExportFieldTranslation"
)

foreach ($cmdletName in $exportCmdlets) {
    $cmd = Get-Command $cmdletName -ErrorAction Stop
    $hasOutFile = $cmd.Parameters.ContainsKey('OutFile')
    
    if ($hasOutFile) {
        $paramSets = $cmd.Parameters['OutFile'].ParameterSets.Keys
        $hasToFileSet = $paramSets -contains 'ToFile'
        
        if ($hasToFileSet) {
            Write-Host "  ✓ $cmdletName" -ForegroundColor Green
            $passCount++
        } else {
            Write-Host "  ✗ $cmdletName (missing ToFile parameter set)" -ForegroundColor Red
        }
    } else {
        Write-Host "  ✗ $cmdletName (missing OutFile parameter)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "Total cmdlets checked: $($importCmdlets.Count + $exportCmdlets.Count)"
Write-Host "Passed: $passCount" -ForegroundColor $(if ($passCount -eq ($importCmdlets.Count + $exportCmdlets.Count)) { "Green" } else { "Yellow" })

if ($passCount -eq ($importCmdlets.Count + $exportCmdlets.Count)) {
    Write-Host ""
    Write-Host "✓ All cmdlets have been successfully enhanced with file parameters!" -ForegroundColor Green
    exit 0
} else {
    Write-Host ""
    Write-Host "✗ Some cmdlets are missing expected parameters" -ForegroundColor Red
    exit 1
}
