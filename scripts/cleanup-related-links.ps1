# Remove leftover merge/branch fragments from docs
# - Removes the specific commit suffix "c188683 (feat: auto completion of table and column names)"
# - Removes any leftover conflict markers like >>>>>>>, <<<<<<<, and =======
# - Operates on Rnwood.Dataverse.Data.PowerShell/docs/*.md

$repoRoot = Split-Path -Parent $PSScriptRoot
$docsPath = Join-Path $repoRoot 'Rnwood.Dataverse.Data.PowerShell\docs'

Write-Host "Scanning docs in: $docsPath"

$files = Get-ChildItem -Path $docsPath -Filter '*.md' -File -Recurse
$modified = @()

foreach ($f in $files) {
    $text = Get-Content -Raw -Encoding UTF8 $f.FullName
    $orig = $text

    # Remove exact commit/branch fragment wherever it appears
    $text = $text -replace 'c188683 \(feat: auto completion of table and column names\)', ''

    # Remove any leftover full-line conflict markers like '>>>>>>> c188683 (...)'
    $text = $text -replace '(?m)^[ \t]*>{7}.*\r?\n?', ''
    $text = $text -replace '(?m)^[ \t]*<{7}.*\r?\n?', ''
    $text = $text -replace '(?m)^[ \t]*={7}.*\r?\n?', ''

    # Normalise '## RELATED LINKS' headers that may have been left with extra spaces
    $text = $text -replace '(?m)^##\s+RELATED LINKS\s*\r?\n', "## RELATED LINKS`n"

    if ($text -ne $orig) {
        Set-Content -LiteralPath $f.FullName -Value $text -Encoding UTF8
        $modified += $f.FullName
        Write-Host "Modified: $($f.Name)"
    }
}

# Additional pass: detect and remove whole-file duplication (file is two identical halves)
foreach ($f in $files) {
    $lines = Get-Content -Encoding UTF8 $f.FullName
    $count = $lines.Count
    if ($count -gt 2 -and ($count % 2) -eq 0) {
        $half = [int]($count / 2)
        $firstHalf = $lines[0..($half-1)] -join "`n"
        $secondHalf = $lines[$half..($count-1)] -join "`n"
        if ($firstHalf -eq $secondHalf) {
            Set-Content -LiteralPath $f.FullName -Value $firstHalf -Encoding UTF8
            Write-Host "De-duplicated: $($f.Name)"
            if (-not ($modified -contains $f.FullName)) { $modified += $f.FullName }
            continue
        }
    }

    # Remove any stray YAML front-matter marker '---' that appears after the initial document section
    $allLines = Get-Content -Encoding UTF8 $f.FullName
    $indexes = for ($i=0; $i -lt $allLines.Count; $i++) { if ($allLines[$i].Trim() -eq '---') { $i } }
    if ($indexes.Count -gt 2) {
        # Keep only the first front matter (from index 0 to second index), and remove any further '---' markers that start a new frontmatter block
        $firstClose = $indexes[1]
        $newLines = $allLines[0..$firstClose] + $allLines[($firstClose+1)..($allLines.Count-1)]
        # Remove any '---' lines beyond the initial closing marker
        $cleaned = $newLines | Where-Object { $_.Trim() -ne '---' -or ($_ -eq $newLines[0]) }
        Set-Content -LiteralPath $f.FullName -Value ($cleaned -join "`n") -Encoding UTF8
        Write-Host "Removed extra front-matter markers in: $($f.Name)"
        if (-not ($modified -contains $f.FullName)) { $modified += $f.FullName }
    }
}

Write-Host "Modified files: $($modified.Count)"
if ($modified.Count -gt 0) { $modified | ForEach-Object { Write-Host "  $_" } }
else { Write-Host "No changes made." }
