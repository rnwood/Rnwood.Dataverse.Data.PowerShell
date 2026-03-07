#!/usr/bin/env pwsh
# Validates help markdown files for corruption issues
# Exit code: 0 = all healthy, 1 = issues found

param(
    [string]$DocsPath = "./Rnwood.Dataverse.Data.PowerShell/docs"
)

$ErrorActionPreference = "Stop"

Write-Host "Validating help files for corruption..." -ForegroundColor Cyan

$issues = @()
$filesChecked = 0

Get-ChildItem -Path $DocsPath -Filter "*.md" | ForEach-Object {
    $file = $_.FullName
    $filename = $_.Name
    
    # Skip the module index file
    if ($filename -eq "Rnwood.Dataverse.Data.PowerShell.md") {
        return
    }
    
    $filesChecked++
    $content = Get-Content -Path $file -Raw
    $lines = @(Get-Content -Path $file)
    
    # Check 1: File size - if >100KB, likely corrupted
    if ($_.Length -gt 100KB) {
        $issues += [PSCustomObject]@{
            File = $filename
            Issue = "File too large (likely corrupted)"
            Details = "$($_.Length) bytes, $($lines.Count) lines"
            Severity = "ERROR"
        }
        Write-Host "  [ERROR] $filename - File too large: $($_.Length) bytes, $($lines.Count) lines" -ForegroundColor Red
    }
    
    # Check 2: Very long lines (>1000 chars) might indicate enum duplication
    $maxLineLength = ($lines | ForEach-Object { $_.Length } | Measure-Object -Maximum).Maximum
    if ($maxLineLength -gt 1000) {
        $longLineNumber = 0
        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i].Length -eq $maxLineLength) {
                $longLineNumber = $i + 1
                break
            }
        }
        $issues += [PSCustomObject]@{
            File = $filename
            Issue = "Very long line detected (possible enum duplication)"
            Details = "Line $longLineNumber has $maxLineLength characters"
            Severity = "WARNING"
        }
        Write-Host "  [WARNING] $filename - Very long line ${longLineNumber}: $maxLineLength chars" -ForegroundColor Yellow
    }
    
    # Check 3: Count parameter sections vs unique parameters
    $paramMatches = [regex]::Matches($content, '^### -(.+)$', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $paramSections = @($paramMatches | ForEach-Object { $_.Groups[1].Value })
    $uniqueParams = $paramSections | Sort-Object -Unique
    
    if ($paramSections.Count -gt 0 -and $uniqueParams.Count -gt 0) {
        $ratio = [Math]::Round($paramSections.Count / $uniqueParams.Count, 1)
        if ($ratio -gt 10) {
            $issues += [PSCustomObject]@{
                File = $filename
                Issue = "Excessive parameter duplication"
                Details = "$($paramSections.Count) total sections, $($uniqueParams.Count) unique params (${ratio}x duplication)"
                Severity = "ERROR"
            }
            Write-Host "  [ERROR] $filename - Excessive parameter duplication: ${ratio}x" -ForegroundColor Red
        }
    }
}

Write-Host "`nValidation Summary:" -ForegroundColor Cyan
Write-Host "  Files checked: $filesChecked" -ForegroundColor White

if ($issues.Count -eq 0) {
    Write-Host "  ✓ All help files are healthy!" -ForegroundColor Green
    exit 0
} else {
    $errors = @($issues | Where-Object { $_.Severity -eq "ERROR" })
    $warnings = @($issues | Where-Object { $_.Severity -eq "WARNING" })
    
    if ($errors.Count -gt 0) {
        Write-Host "  ✗ Found $($errors.Count) error(s)" -ForegroundColor Red
    }
    if ($warnings.Count -gt 0) {
        Write-Host "  ⚠ Found $($warnings.Count) warning(s)" -ForegroundColor Yellow
    }
    
    Write-Host "`nIssues:" -ForegroundColor Cyan
    $issues | Format-Table -Property File, Issue, Details, Severity -AutoSize
    
    if ($errors.Count -gt 0) {
        Write-Host "`n❌ Validation FAILED - please regenerate corrupted help files" -ForegroundColor Red
        exit 1
    } else {
        Write-Host "`n⚠ Validation passed with warnings" -ForegroundColor Yellow
        exit 0
    }
}
