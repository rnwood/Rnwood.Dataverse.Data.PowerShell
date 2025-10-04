# Load the built Rnwood.Dataverse.Data.PowerShell module for interactive testing
$ErrorActionPreference = 'Stop'

Import-Module ./Rnwood.Dataverse.Data.PowerShell.psd1

Write-Host 'Exported commands:'
Get-Command -Module Rnwood.Dataverse.Data.PowerShell | Select-Object Name,CommandType | Format-Table -AutoSize
