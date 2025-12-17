
$packagePath = "C:\Users\rob\.nuget\packages\microsoft.powershell.editorservices\0.6.2.389\lib\net45\Microsoft.PowerShell.EditorServices.dll"
if (-not (Test-Path $packagePath)) {
    Write-Error "File not found: $packagePath"
    exit
}
Add-Type -Path $packagePath
$assembly = [System.Reflection.Assembly]::LoadFrom($packagePath)
$types = $assembly.GetExportedTypes()
$types | Where-Object { $_.Name -eq "ScriptFile" } | ForEach-Object {
    Write-Host "Constructors of $($_.Name):"
    $_.GetConstructors() | Select-Object ToString
}

