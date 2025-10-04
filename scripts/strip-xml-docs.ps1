param(
    [string[]]$Files = @("c:\src\ps\Rnwood.Dataverse.Data.PowerShell.Cmdlets\Commands\SetDataverseRecordCmdlet.cs")
)

foreach ($file in $Files) {
    if (-not (Test-Path $file)) { Write-Warning "File not found: $file"; continue }
    $content = Get-Content -Raw -Encoding UTF8 $file
    $new = $content -replace "(?m)^\s*///", "//"
    if ($new -ne $content) {
        Set-Content -LiteralPath $file -Value $new -Encoding UTF8
        Write-Host "Converted XML doc comments to line comments in: $file"
    } else {
        Write-Host "No XML doc comments found in: $file"
    }
}
