param($projectdir)
$projectdir=$projectdir.Trim('""')
$ErrorActionPreference="Stop"
install-module -scope CurrentUser PlatyPs -force

New-ExternalHelp $projectdir/docs -OutputPath $PSScriptRoot\en-GB\ -force