#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Automatically updates cmdlets to add PowerShell-friendly parameter sets for complex SDK types.

.DESCRIPTION
    This script systematically updates all cmdlets that use complex SDK types (QueryBase, ColumnSet, etc.)
    to add PowerShell-friendly parameter alternatives while maintaining backward compatibility.
    
    It reads the implementation status document and applies the appropriate pattern for each complex type.

.PARAMETER DryRun
    If specified, shows what would be changed without modifying files.

.PARAMETER CmdletName
    If specified, only updates the specified cmdlet. Otherwise updates all pending cmdlets.
#>

param(
    [switch]$DryRun,
    [string]$CmdletName
)

$ErrorActionPreference = "Stop"

# Read the implementation status
$statusDoc = Get-Content "COMPLEX_TYPES_IMPLEMENTATION_STATUS.md" -Raw

# Parse the cmdlet list from the status document
$cmdletPattern = '\| (.+?) \| (.+?) \| ⏳ Pending \|'
$matches = [regex]::Matches($statusDoc, $cmdletPattern)

$pendingCmdlets = @()
foreach ($match in $matches) {
    $pendingCmdlets += @{
        Name = $match.Groups[1].Value.Trim()
        ComplexType = $match.Groups[2].Value.Trim()
    }
}

Write-Host "Found $($pendingCmdlets.Count) pending cmdlets to update" -ForegroundColor Cyan

if ($CmdletName) {
    $pendingCmdlets = $pendingCmdlets | Where-Object { $_.Name -eq $CmdletName }
    if ($pendingCmdlets.Count -eq 0) {
        Write-Host "Cmdlet $CmdletName not found in pending list" -ForegroundColor Yellow
        exit 0
    }
}

function Add-QueryBaseParameters {
    param($Content, $CmdletName)
    
    # Find the QueryBase parameter
    if ($Content -notmatch '\[Parameter.*?\]\s+public\s+QueryBase\s+(\w+)\s*{\s*get;\s*set;\s*}') {
        Write-Warning "Could not find QueryBase parameter in $CmdletName"
        return $Content
    }
    
    $paramName = $Matches[1]
    
    # Add parameter sets to existing QueryBase parameter
    $oldParam = $Matches[0]
    $newParam = @"
[Parameter(ParameterSetName = "QueryObject", Mandatory = false)]
        public QueryBase $paramName { get; set; }

        [Parameter(ParameterSetName = "FetchXml", Mandatory = true, HelpMessage = "FetchXML query string for filtering records")]
        public string FetchXml { get; set; }

        [Parameter(ParameterSetName = "Filter", Mandatory = true, HelpMessage = "Hashtable filter for simple queries. Use @{column='value'} or @{column=@{operator='eq';value='value'}} for complex filters")]
        public Hashtable Filter { get; set; }

        [Parameter(ParameterSetName = "Filter", Mandatory = true, HelpMessage = "Logical name of the table to query when using Filter parameter")]
        [Parameter(ParameterSetName = "FetchXml", Mandatory = false)]
        public string TableName { get; set; }
"@
    
    $Content = $Content.Replace($oldParam, $newParam)
    
    # Add conversion logic in ProcessRecord
    if ($Content -match '(protected override void ProcessRecord\(\)\s*{)') {
        $insertPoint = $Matches[1]
        $conversionCode = @"
$insertPoint

            // Handle PowerShell-friendly parameter sets
            if (ParameterSetName == "FetchXml" || ParameterSetName == "Filter")
            {
                $paramName = DataverseComplexTypeConverter.ToQueryBase(FetchXml, Filter, TableName);
            }
"@
        $Content = $Content.Replace($insertPoint, $conversionCode)
    }
    
    return $Content
}

function Add-ColumnSetParameters {
    param($Content, $CmdletName)
    
    # Find the ColumnSet parameter
    if ($Content -notmatch '\[Parameter.*?\]\s+public\s+ColumnSet\s+(\w+)\s*{\s*get;\s*set;\s*}') {
        Write-Warning "Could not find ColumnSet parameter in $CmdletName"
        return $Content
    }
    
    $paramName = $Matches[1]
    
    # Add parameter sets
    $oldParam = $Matches[0]
    $newParam = @"
[Parameter(ParameterSetName = "ColumnSetObject", Mandatory = false)]
        public ColumnSet $paramName { get; set; }

        [Parameter(ParameterSetName = "Columns", Mandatory = true, HelpMessage = "Array of column logical names to retrieve")]
        public string[] Columns { get; set; }

        [Parameter(ParameterSetName = "AllColumns", Mandatory = true, HelpMessage = "Retrieve all columns")]
        public SwitchParameter AllColumns { get; set; }
"@
    
    $Content = $Content.Replace($oldParam, $newParam)
    
    # Add conversion logic
    if ($Content -match '(protected override void ProcessRecord\(\)\s*{)') {
        $insertPoint = $Matches[1]
        $conversionCode = @"
$insertPoint

            // Handle PowerShell-friendly parameter sets
            if (ParameterSetName == "Columns" || ParameterSetName == "AllColumns")
            {
                $paramName = DataverseComplexTypeConverter.ToColumnSet(Columns, AllColumns.IsPresent);
            }
"@
        $Content = $Content.Replace($insertPoint, $conversionCode)
    }
    
    return $Content
}

# Process each cmdlet
$updatedCount = 0
$failedCount = 0

foreach ($cmdlet in $pendingCmdlets) {
    $cmdletFileName = "$($cmdlet.Name)Cmdlet.cs"
    $cmdletPath = "Rnwood.Dataverse.Data.PowerShell.Cmdlets/Commands/$cmdletFileName"
    
    if (-not (Test-Path $cmdletPath)) {
        Write-Warning "File not found: $cmdletPath"
        $failedCount++
        continue
    }
    
    Write-Host "`nProcessing $($cmdlet.Name) ($($cmdlet.ComplexType))..." -ForegroundColor Yellow
    
    $content = Get-Content $cmdletPath -Raw
    $originalContent = $content
    
    # Apply the appropriate transformation
    switch ($cmdlet.ComplexType) {
        "QueryBase" {
            $content = Add-QueryBaseParameters $content $cmdlet.Name
        }
        "ColumnSet" {
            $content = Add-ColumnSetParameters $content $cmdlet.Name
        }
        default {
            Write-Warning "No handler for complex type: $($cmdlet.ComplexType)"
            $failedCount++
            continue
        }
    }
    
    if ($content -eq $originalContent) {
        Write-Host "  No changes needed or transformation failed" -ForegroundColor Gray
        $failedCount++
        continue
    }
    
    if ($DryRun) {
        Write-Host "  [DRY RUN] Would update $cmdletPath" -ForegroundColor Cyan
        $updatedCount++
    } else {
        Set-Content $cmdletPath $content -NoNewline
        Write-Host "  ✓ Updated $cmdletPath" -ForegroundColor Green
        $updatedCount++
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Updated: $updatedCount" -ForegroundColor Green
Write-Host "  Failed: $failedCount" -ForegroundColor $(if ($failedCount -gt 0) { "Red" } else { "Green" })
Write-Host "  Total: $($pendingCmdlets.Count)" -ForegroundColor Cyan

if (-not $DryRun -and $updatedCount -gt 0) {
    Write-Host "`nRebuild the project to verify changes compile successfully." -ForegroundColor Yellow
}
