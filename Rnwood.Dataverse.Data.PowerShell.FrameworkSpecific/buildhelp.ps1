param($projectdir)
$projectdir=$projectdir.Trim('""')
$ErrorActionPreference="Stop"
if (-not(get-installedmodule Platyps -MinimumVersion 0.14.1 -ErrorAction silentlycontinue)) {
	install-module -scope CurrentUser PlatyPs -force
}

New-ExternalHelp $projectdir/docs -OutputPath $PSScriptRoot\en-GB\ -force