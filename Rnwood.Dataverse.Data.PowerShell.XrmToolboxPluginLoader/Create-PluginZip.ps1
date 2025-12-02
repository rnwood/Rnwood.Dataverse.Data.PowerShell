param(
    [Parameter(Mandatory = $true)][string]$PluginFolder,
    [Parameter(Mandatory = $true)][string]$PluginZip
)

try {
    if (Test-Path $PluginZip) { Remove-Item -LiteralPath $PluginZip -Force }
    # Use Compress-Archive which is available in Windows PowerShell and PowerShell Core on Windows
    Compress-Archive -Path (Join-Path $PluginFolder '*') -DestinationPath $PluginZip -CompressionLevel NoCompression -Force | Out-Null
    if (Test-Path $PluginZip) { "OK" | Out-File -FilePath (Join-Path $PluginFolder 'o.zip.build.log') -Encoding UTF8 } else { "FAILED" | Out-File -FilePath (Join-Path $PluginFolder 'o.zip.build.log') -Encoding UTF8 }
}
catch {
    $_ | Out-File -FilePath (Join-Path $PluginFolder 'o.zip.build.log') -Encoding UTF8
    throw
}
