# Cleans leftover Git merge conflict markers by keeping the incoming (theirs) section of each conflict block.
# Usage: pwsh -NoProfile -File .\scripts\clean-merge-markers.ps1

param (
    [string]$Root = "$(Resolve-Path .)"
)

Write-Host "Scanning for files with conflict markers under $Root"

$files = Get-ChildItem -Path $Root -Recurse -File | Where-Object { $_.FullName -notmatch '\\.git\\' }

$conflictedFiles = @()

foreach ($file in $files) {
    $text = Get-Content -Raw -LiteralPath $file.FullName -ErrorAction SilentlyContinue
    if ($null -eq $text) { continue }

    if ($text -match '<<<<<<< HEAD') {
        $original = $text
        # Replace each conflict block by keeping the 'theirs' section (the part between ======= and >>>>>>>)
        # Use a strict capture of both sides: group1 = our HEAD, group2 = incoming/theirs
    # Allow for optional branch labels glued to the ======= line (e.g. '=======rebase-resolved')
    $pattern = '(?s)<<<<<<< HEAD\r?\n(.*?)\r?\n={7}[^\r\n]*\r?\n(.*?)\r?\n>>>>>>>[^\r\n]*\r?\n'
        while ($text -match $pattern) {
            $text = [System.Text.RegularExpressions.Regex]::Replace($text, $pattern, '$2')
        }

        if ($text -ne $original) {
            $backupDir = Join-Path -Path $env:TEMP -ChildPath "merge-backup-$(Get-Random)"
            New-Item -ItemType Directory -Force -Path $backupDir | Out-Null
            $backupPath = Join-Path -Path $backupDir -ChildPath ($(Split-Path $file.FullName -Leaf))
            Set-Content -LiteralPath $backupPath -Value $original -Encoding UTF8

            Set-Content -LiteralPath $file.FullName -Value $text -Encoding UTF8
            Write-Host "Cleaned markers in: $($file.FullName)" -ForegroundColor Green
            $conflictedFiles += $file.FullName
        }
    }
}

if ($conflictedFiles.Count -eq 0) {
    Write-Host "No conflict markers found." -ForegroundColor Yellow
} else {
    Write-Host "Finished cleaning. Files modified: $($conflictedFiles.Count)" -ForegroundColor Cyan
}
