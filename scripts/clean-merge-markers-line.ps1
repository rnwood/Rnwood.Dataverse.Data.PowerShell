# Line-based merge conflict cleaner: keeps the 'theirs' (incoming) section for each conflict block.
# Usage: pwsh -NoProfile -File .\scripts\clean-merge-markers-line.ps1

param(
    [string]$Root = "$(Resolve-Path .)"
)

Write-Host "Scanning for files with conflict markers under $Root" -ForegroundColor Cyan

$files = Get-ChildItem -Path $Root -Recurse -File | Where-Object { $_.FullName -notmatch '\\.git\\' }
$modified = 0
foreach ($file in $files) {
    $lines = Get-Content -LiteralPath $file.FullName -ErrorAction SilentlyContinue
    if (-not $lines) { continue }
    if ($lines -notmatch '<<<<<<<') { continue }

    $backupDir = Join-Path -Path $env:TEMP -ChildPath "merge-backup-$(Get-Random)"
    New-Item -ItemType Directory -Force -Path $backupDir | Out-Null
    $backupPath = Join-Path -Path $backupDir -ChildPath ($(Split-Path $file.FullName -Leaf))
    Set-Content -LiteralPath $backupPath -Value ($lines -join "`n") -Encoding UTF8

    $outLines = New-Object System.Collections.Generic.List[string]
    $state = 'normal' # normal | in_head | in_theirs

    foreach ($line in $lines) {
        if ($state -eq 'normal') {
            if ($line -match '^<<<<<<<') { $state = 'in_head'; continue }
            else { $outLines.Add($line); continue }
        }

        if ($state -eq 'in_head') {
            if ($line -match '^={7}') { $state = 'in_theirs'; continue }
            elseif ($line -match '^======') { $state = 'in_theirs'; continue }
            else { continue }
        }

        if ($state -eq 'in_theirs') {
            if ($line -match '^>>>>>>>') { $state = 'normal'; continue }
            else { $outLines.Add($line); continue }
        }
    }

    Set-Content -LiteralPath $file.FullName -Value ($outLines -join "`n") -Encoding UTF8
    Write-Host "Fixed conflict markers in: $($file.FullName)" -ForegroundColor Green
    $modified++
}

Write-Host "Finished. Files modified: $modified" -ForegroundColor Cyan
