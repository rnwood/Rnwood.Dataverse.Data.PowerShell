
$packagePath = "C:\Users\rob\.nuget\packages\microsoft.powershell.editorservices\0.6.2.389\lib\net45\Microsoft.PowerShell.EditorServices.dll"
if (-not (Test-Path $packagePath)) {
    Write-Error "File not found: $packagePath"
    exit
}
Add-Type -Path $packagePath
$enumType = [Microsoft.PowerShell.EditorServices.CompletionType]
[Enum]::GetNames($enumType) | ForEach-Object {
    $val = [Enum]::Format($enumType, [Enum]::Parse($enumType, $_), "d")
    Write-Host "$_ = $val"
}
