# Manual Test for CTRL+C During Environment Selection
# 
# This test demonstrates that CTRL+C properly aborts the environment selection menu.
# 
# HOW TO RUN THIS TEST:
# 1. Ensure you have credentials that can authenticate to at least one Dataverse environment
# 2. Run this script in an interactive PowerShell session
# 3. When prompted to select an environment, press CTRL+C
# 4. The cmdlet should abort with an "OperationStopped" error
#
# EXPECTED BEHAVIOR:
# - The environment selection menu appears
# - When you press CTRL+C, the operation is cancelled immediately
# - An error is displayed indicating the operation was stopped
#
# BEFORE THE FIX:
# - CTRL+C was ignored and you had to enter a valid selection to exit
#
# AFTER THE FIX:
# - CTRL+C properly cancels the operation

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CTRL+C Environment Selection Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This test will authenticate and show available environments." -ForegroundColor Yellow
Write-Host "When the environment selection menu appears, press CTRL+C." -ForegroundColor Yellow
Write-Host "The operation should abort immediately." -ForegroundColor Yellow
Write-Host ""
Write-Host "Note: This test requires valid credentials and will show a browser login." -ForegroundColor Gray
Write-Host ""

$proceed = Read-Host "Do you want to proceed? (y/n)"
if ($proceed -ne "y") {
    Write-Host "Test cancelled." -ForegroundColor Gray
    exit 0
}

Write-Host ""
Write-Host "Starting authentication..." -ForegroundColor Cyan

try {
    # Attempt to connect without specifying -Url
    # This will trigger the environment selection menu
    # When prompted, the user should press CTRL+C
    $connection = Get-DataverseConnection -Interactive
    
    # If we get here, the user successfully selected an environment
    Write-Host ""
    Write-Host "WARNING: You successfully selected an environment instead of pressing CTRL+C!" -ForegroundColor Red
    Write-Host "To test CTRL+C handling, run this script again and press CTRL+C when prompted." -ForegroundColor Yellow
    
} catch {
    # Check if the error is due to cancellation
    if ($_.Exception -is [System.Management.Automation.PipelineStoppedException] -or 
        $_.FullyQualifiedErrorId -eq "OperationStopped") {
        Write-Host ""
        Write-Host "SUCCESS: Operation was properly cancelled!" -ForegroundColor Green
        Write-Host "CTRL+C handling is working correctly." -ForegroundColor Green
        exit 0
    } else {
        Write-Host ""
        Write-Host "ERROR: Unexpected error occurred:" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host ""
        Write-Host "Error Category: $($_.CategoryInfo.Category)" -ForegroundColor Gray
        Write-Host "Fully Qualified Error ID: $($_.FullyQualifiedErrorId)" -ForegroundColor Gray
        exit 1
    }
}
