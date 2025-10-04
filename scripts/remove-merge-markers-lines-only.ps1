# Remove any leftover full-line merge marker lines (<<<<<, =======, >>>>>>>) after prior cleaning.
param(
    [string]$Root = "$(Resolve-Path .)"
)

$files = Get-ChildItem -Path $Root -Recurse -File | Where-Object { $_.FullName -notmatch '\\.git\\' }
$modified = 0
foreach ($file in $files) {
    $lines = Get-Content -LiteralPath $file.FullName -ErrorAction SilentlyContinue
    if (-not $lines) { continue }
    if ($lines -notmatch '^(<<<<<<<|={7}|>>>>>>> )') { continue }

    $backupDir = Join-Path -Path $env:TEMP -ChildPath "merge-backup-$(Get-Random)"
    New-Item -ItemType Directory -Force -Path $backupDir | Out-Null
    $backupPath = Join-Path -Path $backupDir -ChildPath ($(Split-Path $file.FullName -Leaf))
    Set-Content -LiteralPath $backupPath -Value ($lines -join "`n") -Encoding UTF8

    $filtered = $lines | Where-Object { $_ -notmatch '^(<<<<<<<|={7}|>>>>>>>.*)' }
    Set-Content -LiteralPath $file.FullName -Value ($filtered -join "`n") -Encoding UTF8
    Write-Host "Removed marker lines in: $($file.FullName)" -ForegroundColor Green
    $modified++
}
Write-Host "Finished. Files modified: $modified" -ForegroundColor Cyan
