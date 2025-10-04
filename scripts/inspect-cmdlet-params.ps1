Import-Module "$PSScriptRoot\..\Rnwood.Dataverse.Data.PowerShell\bin\Debug\netstandard2.0\Rnwood.Dataverse.Data.PowerShell.psd1" -Force
$cmd = Get-Command -Name 'Set-DataverseRecord' -ErrorAction Stop
Write-Output "Command: $($cmd.Name)"
if ($null -eq $cmd.Parameters) { Write-Output "Parameters is null (no metadata)"; $cmd | Format-List *; exit }
$cmd.Parameters.GetEnumerator() | Sort-Object Name | ForEach-Object {
    $name = $_.Name
    $param = $_.Value
    Write-Output "Parameter: $name"
    Write-Output "  ParameterSets: $($param.ParameterSets -join ', ')"
    Write-Output "  Aliases: $($param.Aliases -join ', ')"
    Write-Output "  Attributes: $($param.Attributes | ForEach-Object { $_.GetType().Name } -join ', ')"
}
