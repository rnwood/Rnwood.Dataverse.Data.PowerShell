<#
.SYNOPSIS
Cleans up leftover E2E test artifacts from Dataverse environment.

.DESCRIPTION
This script removes old test artifacts created by E2E tests that weren't cleaned up due to test failures or timeouts.
It targets test resources older than a specified age to avoid interfering with currently running tests.

.PARAMETER Url
The Dataverse environment URL.

.PARAMETER ClientId
The client ID for authentication.

.PARAMETER ClientSecret
The client secret for authentication.

.PARAMETER OlderThanHours
Remove artifacts older than this many hours. Default is 24 hours.

.PARAMETER WhatIf
Show what would be deleted without actually deleting.

.EXAMPLE
./Cleanup-E2ETestArtifacts.ps1 -Url "https://org.crm.dynamics.com" -ClientId "xxx" -ClientSecret "yyy"

.EXAMPLE
./Cleanup-E2ETestArtifacts.ps1 -Url "https://org.crm.dynamics.com" -ClientId "xxx" -ClientSecret "yyy" -OlderThanHours 48 -WhatIf
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Url,
    
    [Parameter(Mandatory = $true)]
    [string]$ClientId,
    
    [Parameter(Mandatory = $true)]
    [string]$ClientSecret,
    
    [Parameter(Mandatory = $false)]
    [int]$OlderThanHours = 24,
    
    [Parameter(Mandatory = $false)]
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"
$VerbosePreference = "Continue"

Write-Host "=============================================="
Write-Host "E2E Test Artifacts Cleanup Script"
Write-Host "=============================================="
Write-Host ""
Write-Host "Environment: $Url"
Write-Host "Cutoff: Remove artifacts older than $OlderThanHours hours"
Write-Host "WhatIf Mode: $($WhatIf.IsPresent)"
Write-Host ""

# Calculate cutoff time
$cutoffTime = [DateTime]::UtcNow.AddHours(-$OlderThanHours)
$cutoffTimestamp = $cutoffTime.ToString("yyyyMMddHHmm")

Write-Host "Cutoff time: $($cutoffTime.ToString('yyyy-MM-dd HH:mm:ss')) UTC"
Write-Host ""

# Connect to Dataverse
Write-Host "Connecting to Dataverse..."
$connection = Get-DataverseConnection -Url $Url -ClientId $ClientId -ClientSecret $ClientSecret
Write-Host "✓ Connected successfully"
Write-Host ""

# Initialize counters
$totalDeleted = 0
$totalSkipped = 0
$totalErrors = 0

#region Helper Functions
function Remove-WithRetry {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$ScriptBlock,
        [int]$MaxRetries = 3,
        [int]$DelaySeconds = 5
    )
    
    $attempt = 0
    while ($attempt -lt $MaxRetries) {
        try {
            $attempt++
            & $ScriptBlock
            return
        }
        catch {
            if ($attempt -eq $MaxRetries) {
                throw
            }
            Write-Warning "Attempt $attempt failed: $_. Retrying in $DelaySeconds seconds..."
            Start-Sleep -Seconds $DelaySeconds
        }
    }
}

function Write-CleanupSection {
    param([string]$Title)
    Write-Host ""
    Write-Host "=============================================="
    Write-Host $Title
    Write-Host "=============================================="
}
#endregion

#region Cleanup Forms
Write-CleanupSection "Cleaning up test forms"

try {
    $testForms = Get-DataverseRecord -Connection $connection -TableName systemform -FilterValues @{
        'name:Like' = 'E2ETestForm-%'
    } -Columns formid, name, createdon
    
    if ($testForms) {
        $formsToDelete = $testForms | Where-Object { 
            $_.createdon -and [DateTime]::Parse($_.createdon) -lt $cutoffTime 
        }
        
        Write-Host "Found $($testForms.Count) total test forms"
        Write-Host "  - $($formsToDelete.Count) are older than $OlderThanHours hours"
        Write-Host "  - $($testForms.Count - $formsToDelete.Count) are recent (will skip)"
        
        foreach ($form in $formsToDelete) {
            $age = ([DateTime]::UtcNow - [DateTime]::Parse($form.createdon)).TotalHours
            Write-Host "  Deleting form: $($form.name) (created $('{0:F1}' -f $age) hours ago)"
            
            if (-not $WhatIf) {
                try {
                    Remove-WithRetry {
                        Remove-DataverseForm -Connection $connection -Id $form.formid -Confirm:$false
                    }
                    $totalDeleted++
                }
                catch {
                    Write-Warning "    Failed to delete form $($form.name): $_"
                    $totalErrors++
                }
            }
            else {
                Write-Host "    [WhatIf] Would delete form"
            }
        }
        
        $totalSkipped += ($testForms.Count - $formsToDelete.Count)
    }
    else {
        Write-Host "No test forms found"
    }
}
catch {
    Write-Warning "Error querying forms: $_"
    $totalErrors++
}
#endregion

#region Cleanup Solutions
Write-CleanupSection "Cleaning up test solutions"

try {
    $allSolutions = Get-DataverseSolution -Connection $connection
    $testSolutions = $allSolutions | Where-Object { 
        $_.uniquename -like 'e2esolcomp_*' -or $_.uniquename -like 'test_solution_*'
    }
    
    if ($testSolutions) {
        # Parse timestamp from solution name (e2esolcomp_YYYYMMDDHHMM_*)
        $solutionsToDelete = $testSolutions | Where-Object {
            if ($_.uniquename -match 'e2esolcomp_(\d{12})_') {
                $timestamp = $matches[1]
                [int]$timestamp -lt [int]$cutoffTimestamp
            }
            else {
                # For other test solutions without timestamp, check created date if available
                if ($_.createdon) {
                    [DateTime]::Parse($_.createdon) -lt $cutoffTime
                }
                else {
                    $false
                }
            }
        }
        
        Write-Host "Found $($testSolutions.Count) total test solutions"
        Write-Host "  - $($solutionsToDelete.Count) are older than $OlderThanHours hours"
        Write-Host "  - $($testSolutions.Count - $solutionsToDelete.Count) are recent (will skip)"
        
        foreach ($solution in $solutionsToDelete) {
            Write-Host "  Deleting solution: $($solution.uniquename)"
            
            if (-not $WhatIf) {
                try {
                    Remove-WithRetry {
                        Remove-DataverseSolution -Connection $connection -UniqueName $solution.uniquename -Confirm:$false
                    }
                    $totalDeleted++
                }
                catch {
                    Write-Warning "    Failed to delete solution $($solution.uniquename): $_"
                    $totalErrors++
                }
            }
            else {
                Write-Host "    [WhatIf] Would delete solution"
            }
        }
        
        $totalSkipped += ($testSolutions.Count - $solutionsToDelete.Count)
    }
    else {
        Write-Host "No test solutions found"
    }
}
catch {
    Write-Warning "Error querying solutions: $_"
    $totalErrors++
}
#endregion

#region Cleanup Test Entities
Write-CleanupSection "Cleaning up test entities"

try {
    # Get all entities
    $allEntities = Get-DataverseEntityMetadata -Connection $connection
    $testEntities = $allEntities | Where-Object { 
        $_.LogicalName -like 'new_e2esolent_*' -or
        $_.LogicalName -like 'new_testent_*'
    }
    
    if ($testEntities) {
        # Parse timestamp from entity name (new_e2esolent_YYYYMMDDHHMM_*)
        $entitiesToDelete = $testEntities | Where-Object {
            if ($_.LogicalName -match '(\d{12})_') {
                $timestamp = $matches[1]
                [int]$timestamp -lt [int]$cutoffTimestamp
            }
            else {
                # Without timestamp, be conservative and skip
                $false
            }
        }
        
        Write-Host "Found $($testEntities.Count) total test entities"
        Write-Host "  - $($entitiesToDelete.Count) are older than $OlderThanHours hours"
        Write-Host "  - $($testEntities.Count - $entitiesToDelete.Count) are recent or don't have timestamp (will skip)"
        
        foreach ($entity in $entitiesToDelete) {
            Write-Host "  Deleting entity: $($entity.LogicalName)"
            
            if (-not $WhatIf) {
                try {
                    Remove-WithRetry {
                        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entity.LogicalName -Confirm:$false
                    }
                    $totalDeleted++
                }
                catch {
                    Write-Warning "    Failed to delete entity $($entity.LogicalName): $_"
                    $totalErrors++
                }
            }
            else {
                Write-Host "    [WhatIf] Would delete entity"
            }
        }
        
        $totalSkipped += ($testEntities.Count - $entitiesToDelete.Count)
    }
    else {
        Write-Host "No test entities found"
    }
}
catch {
    Write-Warning "Error querying entities: $_"
    $totalErrors++
}
#endregion

#region Cleanup Web Resources
Write-CleanupSection "Cleaning up test web resources"

try {
    $testWebResources = Get-DataverseRecord -Connection $connection -TableName webresource -FilterValues @{
        'name:Like' = 'new_e2etest_%'
    } -Columns webresourceid, name, createdon
    
    if ($testWebResources) {
        $webResourcesToDelete = $testWebResources | Where-Object { 
            $_.createdon -and [DateTime]::Parse($_.createdon) -lt $cutoffTime 
        }
        
        Write-Host "Found $($testWebResources.Count) total test web resources"
        Write-Host "  - $($webResourcesToDelete.Count) are older than $OlderThanHours hours"
        Write-Host "  - $($testWebResources.Count - $webResourcesToDelete.Count) are recent (will skip)"
        
        foreach ($wr in $webResourcesToDelete) {
            $age = ([DateTime]::UtcNow - [DateTime]::Parse($wr.createdon)).TotalHours
            Write-Host "  Deleting web resource: $($wr.name) (created $('{0:F1}' -f $age) hours ago)"
            
            if (-not $WhatIf) {
                try {
                    Remove-WithRetry {
                        Remove-DataverseRecord -Connection $connection -TableName webresource -Id $wr.webresourceid -Confirm:$false
                    }
                    $totalDeleted++
                }
                catch {
                    Write-Warning "    Failed to delete web resource $($wr.name): $_"
                    $totalErrors++
                }
            }
            else {
                Write-Host "    [WhatIf] Would delete web resource"
            }
        }
        
        $totalSkipped += ($testWebResources.Count - $webResourcesToDelete.Count)
    }
    else {
        Write-Host "No test web resources found"
    }
}
catch {
    Write-Warning "Error querying web resources: $_"
    $totalErrors++
}
#endregion

#region Cleanup Environment Variables
Write-CleanupSection "Cleaning up test environment variables"

try {
    $allEnvVars = Get-DataverseRecord -Connection $connection -TableName environmentvariabledefinition -Columns environmentvariabledefinitionid, schemaname, createdon
    $testEnvVars = $allEnvVars | Where-Object { 
        $_.schemaname -like 'new_e2eenvvar_*'
    }
    
    if ($testEnvVars) {
        $envVarsToDelete = $testEnvVars | Where-Object { 
            $_.createdon -and [DateTime]::Parse($_.createdon) -lt $cutoffTime 
        }
        
        Write-Host "Found $($testEnvVars.Count) total test environment variables"
        Write-Host "  - $($envVarsToDelete.Count) are older than $OlderThanHours hours"
        Write-Host "  - $($testEnvVars.Count - $envVarsToDelete.Count) are recent (will skip)"
        
        foreach ($envVar in $envVarsToDelete) {
            $age = ([DateTime]::UtcNow - [DateTime]::Parse($envVar.createdon)).TotalHours
            Write-Host "  Deleting environment variable: $($envVar.schemaname) (created $('{0:F1}' -f $age) hours ago)"
            
            if (-not $WhatIf) {
                try {
                    Remove-WithRetry {
                        Remove-DataverseRecord -Connection $connection -TableName environmentvariabledefinition -Id $envVar.environmentvariabledefinitionid -Confirm:$false
                    }
                    $totalDeleted++
                }
                catch {
                    Write-Warning "    Failed to delete environment variable $($envVar.schemaname): $_"
                    $totalErrors++
                }
            }
            else {
                Write-Host "    [WhatIf] Would delete environment variable"
            }
        }
        
        $totalSkipped += ($testEnvVars.Count - $envVarsToDelete.Count)
    }
    else {
        Write-Host "No test environment variables found"
    }
}
catch {
    Write-Warning "Error querying environment variables: $_"
    $totalErrors++
}
#endregion

#region Cleanup AppModules
Write-CleanupSection "Cleaning up test app modules"

try {
    $testAppModules = Get-DataverseRecord -Connection $connection -TableName appmodule -FilterValues @{
        'uniquename:Like' = 'test_appmodule_%'
    } -Columns appmoduleid, uniquename, name, createdon
    
    if ($testAppModules) {
        $appModulesToDelete = $testAppModules | Where-Object { 
            $_.createdon -and [DateTime]::Parse($_.createdon) -lt $cutoffTime 
        }
        
        Write-Host "Found $($testAppModules.Count) total test app modules"
        Write-Host "  - $($appModulesToDelete.Count) are older than $OlderThanHours hours"
        Write-Host "  - $($testAppModules.Count - $appModulesToDelete.Count) are recent (will skip)"
        
        foreach ($appModule in $appModulesToDelete) {
            $age = ([DateTime]::UtcNow - [DateTime]::Parse($appModule.createdon)).TotalHours
            Write-Host "  Deleting app module: $($appModule.uniquename) (created $('{0:F1}' -f $age) hours ago)"
            
            if (-not $WhatIf) {
                try {
                    Remove-WithRetry {
                        Remove-DataverseRecord -Connection $connection -TableName appmodule -Id $appModule.appmoduleid -Confirm:$false
                    }
                    $totalDeleted++
                }
                catch {
                    Write-Warning "    Failed to delete app module $($appModule.uniquename): $_"
                    $totalErrors++
                }
            }
            else {
                Write-Host "    [WhatIf] Would delete app module"
            }
        }
        
        $totalSkipped += ($testAppModules.Count - $appModulesToDelete.Count)
    }
    else {
        Write-Host "No test app modules found"
    }
}
catch {
    Write-Warning "Error querying app modules: $_"
    $totalErrors++
}
#endregion

#region Summary
Write-Host ""
Write-Host "=============================================="
Write-Host "Cleanup Summary"
Write-Host "=============================================="
Write-Host "Artifacts deleted: $totalDeleted"
Write-Host "Artifacts skipped (recent): $totalSkipped"
Write-Host "Errors encountered: $totalErrors"
Write-Host ""

if ($WhatIf) {
    Write-Host "WhatIf mode: No changes were actually made"
    Write-Host ""
}

if ($totalErrors -gt 0) {
    Write-Warning "Cleanup completed with $totalErrors errors"
    exit 1
}
else {
    Write-Host "✓ Cleanup completed successfully"
    exit 0
}
#endregion
