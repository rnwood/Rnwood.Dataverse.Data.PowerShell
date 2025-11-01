# Load the built Rnwood.Dataverse.Data.PowerShell module for interactive testing
$ErrorActionPreference = 'Stop'

Import-Module ./Rnwood.Dataverse.Data.PowerShell.psd1

Get-DataverseConnection -name RWPROD2 -setasdefault